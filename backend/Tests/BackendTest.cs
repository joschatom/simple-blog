using backend.Controllers;
using backend.Data;
using backend.DTOs.Shared.Response;
using backend.Helpers;
using backend.Interfaces;
using backend.Models;
using backend.Profiles;
using Bogus;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Expressions;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using System.Collections;
using System.ComponentModel.Design;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.IdentityModel.Tokens.Jwt;
using System.Linq.Expressions;
using System.Net.Http.Headers;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace backend.Tests;


public class TestWebApplicationFactory<TProgram>
    : WebApplicationFactory<TProgram> where TProgram : class
{
    protected override IWebHostBuilder? CreateWebHostBuilder()
    {
        var builder = base.CreateWebHostBuilder();

        if (builder == null) return null;


        return builder.UseConfiguration(
            new ConfigurationBuilder()
                .AddJsonFile("appsettings.tests.json")
                .Build());


    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        var config = new ConfigurationBuilder()
                  .AddJsonFile("appsettings.tests.json")
                  .Build();

        builder.UseTestServer();
        builder.UseConfiguration(config);

        builder.ConfigureTestServices(services =>
        {


            services.AddSingleton<IConfiguration>(config);



            var dbContextDescriptor = services.SingleOrDefault(
                d => d.ServiceType ==
                    typeof(DataContext));

            services.Remove(dbContextDescriptor);

            var dbConnectionDescriptor = services.SingleOrDefault(
                d => d.ServiceType ==
                    typeof(DbConnection));

            services.Remove(dbConnectionDescriptor);

            // Create open SqliteConnection so EF won't automatically close it.
            services.AddSingleton<DbConnection>(container =>
            {
                var connection = new SqliteConnection("DataSource=:memory:");
                connection.Open();

                return connection;
            });

            services.AddDbContext<DataContext>((container, options) =>
            {
                var connection = container.GetRequiredService<DbConnection>();
                options.UseSqlite(connection);


            });

        });

        builder.UseEnvironment("Testing");
    }
}

internal class BackendTests : BackendTests<Program>;
internal class BackendTests<T>
{
    protected TestWebApplicationFactory<Program>
      _factory;

    [SetUp]
    public void BaseSetUp()
    {
        _factory = new TestWebApplicationFactory<Program>();
        _factory.Services.GetRequiredService<DataContext>().Database.EnsureCreated();
        CreateData();
    }

    protected HttpClient CreateClient(User user)
    {
        DataContext.SaveChanges();
        return Authenticated(_factory.CreateClient(), user);
    }

    protected HttpClient CreateAnonymouseClient()
    {
        DataContext.SaveChanges();
        return _factory.CreateClient();
    }

    protected virtual Task CreateData() { return Task.CompletedTask; }

    [TearDown]
    public void BaseTearDown()
    {
        _factory.Dispose();
    }

    protected TRepository Service<TRepository>()
    {
        var postRepo = _factory.Services.GetService<TRepository>();
        Assert.That(postRepo, Is.Not.Null);
        return postRepo;
    }

    /// <summary>
    /// Helper function that assert the correctness of the response
    /// and parsed it as a list of DTOs and joins them(using a given shared property)
    /// with the corresponding model and calls a callback for each.
    /// </summary>
    /// <typeparam name="TDto">Element Type of list returned from the API.</typeparam>
    /// <typeparam name="TModel">Model Type to joined each DTO with.</typeparam>
    /// <param name="expected">Expected Models</param>
    /// <param name="apiResponse">The API Response</param>
    /// <param name="callback">Callback that gets invoked for each model and DTO pairing.</param>
    /// <param name="propertyName">The name of a SHARED property to use to join.</param>
    /// <returns>Nothing, just a Task to be awaited.</returns>
    /// <exception cref="TypeAccessException"/>
    protected async Task AssertForeachDTOJoined
        <[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TDto,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TModel>
    (
        IEnumerable<TModel> expected,
        HttpResponseMessage apiResponse,
        Action<TModel, TDto> callback,
        [DefaultParameterValue("Id")] string propertyName
    )
    {
        if (!apiResponse.IsSuccessStatusCode)
            Assert.Fail($"API Endpoint returned an non-successful status code: {apiResponse.StatusCode}");

        var parsed = await apiResponse.Content.ReadFromJsonAsync<TDto[]>();

        if (parsed is null)
            Assert.Fail("API Endpoint returned malformed data.");

        Assert.That(
            parsed,
            Has.Length.EqualTo(expected.Count()),
            "API Endpoint returned a different amount DTOs than expected."
        );

        var modelProperty = typeof(TModel).GetProperty(propertyName)
            ?? throw new TypeAccessException(propertyName);
        var dtoProperty = typeof(TDto).GetProperty(propertyName)
            ?? throw new TypeAccessException(propertyName);

        var joined = expected.Join(
           parsed,
           (v) => modelProperty.GetValue(v),
           (v) => dtoProperty.GetValue(v),
           (expect, got) => (expect, got)
        );

        Assert.That(
            joined.Count(),
            Is.EqualTo(expected.Count()),
            "Joined count differs from expected."
        );

        foreach (var (expect, got) in joined) callback(expect, got);
    }

    public IUserRepository UserRepository => _factory.Services.GetRequiredService<IUserRepository>();
    public IPostRepository PostRepository => _factory.Services.GetRequiredService<IPostRepository>();
    public DataContext DataContext => _factory.Services.GetRequiredService<DataContext>();
    public JwtTokenGenerator TokenGenerator => _factory.Services.GetRequiredService<JwtTokenGenerator>();

    public async Task Reload<TModel>(params TModel[] entities)
        where TModel : class
    {
        foreach (var entity in entities)
            await DataContext.Entry<TModel>(entity).ReloadAsync();
    }



    protected HttpClient Authenticated(HttpClient client, User user)
    {
        var token = Service<JwtTokenGenerator>().GenerateToken(user);
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, token);
        return client;
    }

    protected TDto ToDTO<TDto, TModel>(TModel model)
    {
        var mapper = _factory.Services.GetRequiredService<AutoMapper.IMapper>();
        return mapper.Map<TDto>(model);
    }

    protected Faker<User> UserFaker => new Faker<User>()
        .RuleFor(u => u.Username, f => f.Internet.UserName())
        .RuleFor(u => u.Email, f => f.Internet.Email())
        .RuleFor(u => u.PasswordHash, f => PasswordHasher.HashPassword(f.Internet.Password()))
        .RuleFor(u => u.CreatedAt, f => f.Date.Past())
        .FinishWith((f, u) =>
        {
            if (DataContext.Users.Any(existing => existing.Username == u.Username))
            {
                u.Username += f.UniqueIndex;
            }

            u = DataContext.Add(u).Entity;
            DataContext.SaveChanges();
        });

    protected IEnumerable<Post> PostFaker(
        int count,
        Func<Faker<Post>, Faker<Post>>? cb = null)
    {
        var faker = new Faker<Post>()
            .RuleFor(p => p.UserId, f => f.PickRandom(DataContext.Users.AsEnumerable()).Id)
            .RuleFor(p => p.Caption, f => f.Lorem.Sentence())
            .RuleFor(p => p.Content, f => f.Lorem.Paragraphs(1, 4))
            .RuleFor(p => p.CreatedAt, f => f.Date.Past());

        faker = cb is null ? faker : cb(faker);

        var posts = faker.Generate(count);
        return posts.Select(p => DataContext.Add(p).Entity);
    }

}


class BackendIntegrationTests : BackendTests<BackendTests>
{

    [Test]
    public void Test_ServiceResolution_Works()
    {
        Assert.Multiple(() =>
        {
            Assert.That(UserRepository, Is.Not.Null);
            Assert.That(PostRepository, Is.Not.Null);
            Assert.That(DataContext, Is.Not.Null);
            Assert.That(TokenGenerator, Is.Not.Null);
        });

        Assert.Pass("Base test to ensure required services exist.");
    }

    [Test]
    public async Task Test_Healthy()
    {
        var client = _factory.CreateClient();

        var resp = await client.GetAsync("/health");
        Assert.Multiple(async () =>
        {
            Assert.That(resp.IsSuccessStatusCode, Is.True);
            Assert.That(await resp.Content.ReadAsStringAsync(), Is.EqualTo("Healthy"));
        });

        Assert.Pass("Base test to ensure setup/teardown works.");
    }

    [Test]
    public async Task Test_Generators()
    {
        var users = UserFaker.Generate(5);
        var posts = PostFaker(12).ToList();
        Assert.Multiple(() =>
        {
            Assert.That(users, Has.Count.EqualTo(5));
            Assert.That(posts, Has.Count.EqualTo(12));
        });
        Assert.Multiple(() =>
        {
            Assert.That(users, Has.Count.EqualTo(5));
            Assert.That(posts, Has.Count.EqualTo(12));
        });

        Assert.Pass("Base test to ensure generators/fakers works.");
    }

    [Test]
    public async Task Test_Authenticated()
    {
        var user = UserFaker.Generate();

        DataContext.SaveChanges();

        var client = _factory.CreateClient();
        client = Authenticated(client, user);

        var resp = await client.GetAsync("/api/users/me");
        Assert.Multiple(async () =>
        {
            Assert.That(resp.IsSuccessStatusCode, Is.True);
            Assert.That(await resp.Content.ReadAsStringAsync(), Contains.Substring(user.Id.ToString()));
        });
    }
}

public class MockAuthSchemaOptions : AuthenticationSchemeOptions
{
    public User? LoggedInUser { get; set; }
}

public class MockAuthHandler : AuthenticationHandler<MockAuthSchemaOptions>
{
    public MockAuthHandler(IOptionsMonitor<MockAuthSchemaOptions> options,
        ILoggerFactory logger, UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {

        if (Options.LoggedInUser is null)
            return Task.FromResult(AuthenticateResult.Fail("not logged in"));

        var principal = new ClaimsPrincipal(
            Helpers.Helpers.GetClaimsIdentity(Options.LoggedInUser)
        );
        var ticket = new AuthenticationTicket(principal, "TestScheme");

        var result = AuthenticateResult.Success(ticket);

        return Task.FromResult(result);
    }
}

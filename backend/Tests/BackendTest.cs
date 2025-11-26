using backend.Controllers;
using backend.Data;
using backend.DTOs.Shared.Response;
using backend.Helpers;
using backend.Interfaces;
using backend.Models;
using backend.Profiles;
using Bogus;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Expressions;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using System.Collections;
using System.ComponentModel.Design;
using System.Data.Common;
using System.Net.Http.Headers;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace backend.Tests;


public class TestWebApplicationFactory<TProgram>
    : WebApplicationFactory<TProgram> where TProgram : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseConfiguration(
            new ConfigurationBuilder()
                .AddJsonFile("appsettings.tests.json")
                .Build());

        builder.UseTestServer();

        builder.ConfigureServices(services =>
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.tests.json")
                .Build();

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
    }

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

    public IUserRepository UserRepository => _factory.Services.GetRequiredService<IUserRepository>();
    public IPostRepository PostRepository => _factory.Services.GetRequiredService<IPostRepository>();
    public DataContext DataContext => _factory.Services.GetRequiredService<DataContext>();
    public JwtTokenGenerator TokenGenerator => _factory.Services.GetRequiredService<JwtTokenGenerator>();


    protected HttpClient Authenticated(HttpClient client, User user)
    {
        var token = Service<JwtTokenGenerator>().GenerateToken(user);
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
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

    protected IEnumerable<Post> PostFaker(int? count = 1, Func<Faker<Post>, Faker<Post>>? cb = null)
    {
        var faker = new Faker<Post>()
            .RuleFor(p => p.UserId, f => f.PickRandom(DataContext.Users.AsEnumerable()).Id)
            .RuleFor(p => p.Caption, f => f.Lorem.Sentence())
            .RuleFor(p => p.Content, f => f.Lorem.Paragraphs(1, 4))
            .RuleFor(p => p.CreatedAt, f => f.Date.Past());

        faker = cb is null ? faker : cb(faker);

        var posts = count.HasValue
            ? faker.Generate(count.Value)
            : faker.GenerateForever();

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
}
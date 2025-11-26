using backend.Data;
using backend.DTOs.Post.Response;
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
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using System.Collections;
using System.ComponentModel.Design;
using System.Data.Common;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace backend.Tests;


internal class BackendTests
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

    public TRepository Service<TRepository>()
    {
        var postRepo = _factory.Services.GetService<TRepository>();
        Assert.That(postRepo, Is.Not.Null);
        return postRepo;
    }

    public IUserRepository UserRepository => _factory.Services.GetRequiredService<IUserRepository>();
    public IPostRepository PostRepository => _factory.Services.GetRequiredService<IPostRepository>();
    public DataContext DataContext => _factory.Services.GetRequiredService<DataContext>();



    public HttpClient Authenticated(HttpClient client, User user)
    {
        var token = Service<JwtTokenGenerator>().GenerateToken(user);
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    public TDto ToDTO<TDto, TModel>(TModel model)
    {
        var mapper = _factory.Services.GetRequiredService<AutoMapper.IMapper>();
        return mapper.Map<TDto>(model);
    }

    public Faker<User> UserFaker => new Faker<User>()
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

    public IEnumerable<Post> PostFaker(int? count = 1, Func<Faker<Post>, Faker<Post>>? cb = null)
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



class PostsTests: BackendTests
{
  

    [Test]
    public async Task GetPosts()
    {
        var users = UserFaker.Generate(3);

        DataContext.SaveChanges();

        var expected = PostFaker(null, null)
            .Take(5)
            .ToArray();

        DataContext.SaveChanges();


        var client = _factory.CreateClient();
        
        var resp = await client.GetAsync("/api/posts");
        var posts = await resp.Content.ReadFromJsonAsync<PostDTO[]>();

        Assert.Multiple(() =>
        {
            Assert.That(resp.IsSuccessStatusCode, Is.True);
            Assert.That(posts, Is.Not.Null);
        });
        
        foreach (var post in expected)
        {
           var got = posts!.SingleOrDefault(p => p.Id == post.Id);

            Assert.Multiple(() =>
            {
                Assert.That(got, Is.Not.Null);
                Assert.That(got!.Caption, Is.EqualTo(post.Caption));
                Assert.That(got!.Content, Is.EqualTo(post.Content));
                Assert.That(got!.UserId, Is.EqualTo(post.UserId));
                Assert.That(got!.CreatedAt, Is.EqualTo(post.CreatedAt).Within(1).Seconds);
            });
        }
    }

    [Test]
    public async Task CreatePost_Unauthenticated_Fails()
    {
        var client = _factory.CreateClient();
        var newPost = new
        {
            Caption = "Test Caption",
            Content = "Test Content"
        };
        var resp = await client.PostAsJsonAsync("/api/posts", newPost);
        Assert.That(resp.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task CreatePost_Authenticated_Succeeds()
    {
        var user = UserFaker.Generate();
        DataContext.SaveChanges();
        var client = _factory.CreateClient();
        Authenticated(client, user);
        var newPost = new
        {
            Caption = "Test Caption",
            Content = "Test Content"
        };
        var resp = await client.PostAsJsonAsync("/api/posts", newPost);
        Assert.That(resp.IsSuccessStatusCode, Is.True);
        var createdPost = DataContext.Posts
            .OrderByDescending(p => p.CreatedAt)
            .FirstOrDefault();
        Assert.Multiple(() =>
        {
            Assert.That(createdPost, Is.Not.Null);
            Assert.That(createdPost!.Caption, Is.EqualTo(newPost.Caption));
            Assert.That(createdPost!.Content, Is.EqualTo(newPost.Content));
            Assert.That(createdPost!.UserId, Is.EqualTo(user.Id));
        });
    }

    [Test]
    public async Task CreatePost_MissingFields_Fails()
    {
        var user = UserFaker.Generate();
        DataContext.SaveChanges();
        var client = _factory.CreateClient();
        Authenticated(client, user);
        var newPost = new
        {
            Caption = "Test Caption"
        };
        var resp = await client.PostAsJsonAsync("/api/posts", newPost);
        Assert.That(resp.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task CreatePost_EmptyContent_Fails()
    {
        var user = UserFaker.Generate();
        DataContext.SaveChanges();
        var client = _factory.CreateClient();
        Authenticated(client, user);
        var newPost = new
        {
            Caption = "Test Caption",
            Content = ""
        };
        var resp = await client.PostAsJsonAsync("/api/posts", newPost);
        Assert.That(resp.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task CreatePost_ExcessivelyLongContent_Fails()
    {
        var user = UserFaker.Generate();
        DataContext.SaveChanges();
        var client = _factory.CreateClient();
        Authenticated(client, user);
        var longContent = new string('a', 10001); 
        var newPost = new
        {
            Caption = "Test Caption",
            Content = longContent
        };
        var resp = await client.PostAsJsonAsync("/api/posts", newPost);
        Assert.That(resp.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task GetPostById()
    {
        var user = UserFaker.Generate();
        var post = PostFaker(1, f => f.RuleFor(p => p.UserId, user.Id)).First();
        DataContext.SaveChanges();
        var client = _factory.CreateClient();
        var resp = await client.GetAsync($"/api/posts/{post.Id}");

        var got = await resp.Content.ReadFromJsonAsync<PostDTO>();

        Assert.Multiple(() =>
        {
            Assert.That(resp.IsSuccessStatusCode, Is.True);
            Assert.That(got, Is.Not.Null);
            Assert.That(got!.Id, Is.EqualTo(post.Id));
            Assert.That(got!.Caption, Is.EqualTo(post.Caption));
            Assert.That(got!.Content, Is.EqualTo(post.Content));
            Assert.That(got!.UserId, Is.EqualTo(post.UserId));
            Assert.That(got!.CreatedAt, Is.EqualTo(post.CreatedAt).Within(1).Seconds);
        });
    }

    [Test]
    public async Task GetPostById_NotFound()
    {
        var guid = Guid.NewGuid();
        var client = _factory.CreateClient();
        var resp = await client.GetAsync($"/api/posts/{guid}");
        Assert.That(resp.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.NotFound));
    }

    [Test]
    public async Task GetPosts_RegistredUsersOnly_Unauthenticated_Fails()
    {
        var user = UserFaker.Generate();
        var post = PostFaker(1, f => f
            .RuleFor(p => p.UserId, user.Id)
            .RuleFor(p => p.RegistredUsersOnly, true))
            .First();
        DataContext.SaveChanges();
        var client = _factory.CreateClient();
        var resp = await client.GetAsync("/api/posts");
        var posts = await resp.Content.ReadFromJsonAsync<PostDTO[]>();
        Assert.Multiple(() =>
        {
            Assert.That(resp.IsSuccessStatusCode, Is.True);
            Assert.That(posts, Is.Not.Null);
            Assert.That(posts!.Any(p => p.Id == post.Id), Is.False);
        });
    }

    [Test]
    public async Task GetPosts_RegistredUsersOnly_Authenticated_Succeeds()
    {
        var user = UserFaker.Generate();
        var post = PostFaker(1, f => f
            .RuleFor(p => p.UserId, user.Id)
            .RuleFor(p => p.RegistredUsersOnly, true))
            .First();
        DataContext.SaveChanges();
        var client = _factory.CreateClient();
        Authenticated(client, user);
        var resp = await client.GetAsync("/api/posts");
        var posts = await resp.Content.ReadFromJsonAsync<PostDTO[]>();
        Assert.Multiple(() =>
        {
            Assert.That(resp.IsSuccessStatusCode, Is.True);
            Assert.That(posts, Is.Not.Null);
            Assert.That(posts!.Any(p => p.Id == post.Id), Is.True);
        });
    }

    [Test]
    public async Task GetPostById_RegistredUsersOnly_Unauthenticated_Fails()
    {
        var user = UserFaker.Generate();
        var post = PostFaker(1, f => f
            .RuleFor(p => p.UserId, user.Id)
            .RuleFor(p => p.RegistredUsersOnly, true))
            .First();
        DataContext.SaveChanges();
        var client = _factory.CreateClient();
        var resp = await client.GetAsync($"/api/posts/{post.Id}");
        Assert.That(resp.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.Forbidden));
    }

    [Test]
    public async Task GetPostById_RegistredUsersOnly_Authenticated_Succeeds()
    {
        var user = UserFaker.Generate();
        var post = PostFaker(1, f => f
            .RuleFor(p => p.UserId, user.Id)
            .RuleFor(p => p.RegistredUsersOnly, true))
            .First();
        DataContext.SaveChanges();
        var client = _factory.CreateClient();
        Authenticated(client, user);
        var resp = await client.GetAsync($"/api/posts/{post.Id}");
        var got = await resp.Content.ReadFromJsonAsync<PostDTO>();
        Assert.Multiple(() =>
        {
            Assert.That(resp.IsSuccessStatusCode, Is.True);
            Assert.That(got, Is.Not.Null);
            Assert.That(got!.Id, Is.EqualTo(post.Id));
            Assert.That(got!.Caption, Is.EqualTo(post.Caption));
            Assert.That(got!.Content, Is.EqualTo(post.Content));
            Assert.That(got!.UserId, Is.EqualTo(post.UserId));
            Assert.That(got!.CreatedAt, Is.EqualTo(post.CreatedAt).Within(1).Seconds);
        });
    }

    [Test]
    public async Task UpdatePost()
    {
        var user = UserFaker.Generate();
        var post = PostFaker(1, f => f.RuleFor(p => p.UserId, user.Id)).First();
        DataContext.SaveChanges();

        var client = _factory.CreateClient();
        client = Authenticated(client, user);
        var updateData = new
        {
            Caption = "Updated Caption",
            Content = "Updated Content"
        };

        var resp = await client.PutAsJsonAsync($"/api/posts/{post.Id}", updateData);
        Assert.That(resp.IsSuccessStatusCode, Is.True);

        var updatedPost = DataContext.Posts.Find(post.Id);
        Assert.Multiple(() =>
        {
            Assert.That(updatedPost, Is.Not.Null);
            Assert.That(updatedPost!.Caption, Is.EqualTo(updateData.Caption));
            Assert.That(updatedPost!.Content, Is.EqualTo(updateData.Content));
        });
    }

    [Test]
    public async Task UpdatePost_Unauthorized_Fails()
    {
        var user = UserFaker.Generate();
        var otherUser = UserFaker.Generate();
        var post = PostFaker(1, f => f.RuleFor(p => p.UserId, user.Id)).First();
        DataContext.SaveChanges();
        var client = _factory.CreateClient();
        client = Authenticated(client, otherUser);
        var updateData = new
        {
            Caption = "Updated Caption",
            Content = "Updated Content"
        };
        var resp = await client.PutAsJsonAsync($"/api/posts/{post.Id}", updateData);
        Assert.That(resp.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.Forbidden));
    }

    [Test]
    public async Task DeletePost()
    {
        var user = UserFaker.Generate();
        var post = PostFaker(1, f => f.RuleFor(p => p.UserId, user.Id)).First();
        DataContext.SaveChanges();

        var client = _factory.CreateClient();
        client = Authenticated(client, user);

        var resp = await client.DeleteAsync($"/api/posts/{post.Id}");

        Assert.That(resp.IsSuccessStatusCode, Is.True);
        var deletedPost = DataContext.Posts.Find(post.Id);
        Assert.That(deletedPost, Is.Null);
    }
}

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

            services.AddDbContext<DataContext>(( container, options) =>
            {
                var connection = container.GetRequiredService<DbConnection>();
                options.UseSqlite(connection);
               
                
            });

        });

        builder.UseEnvironment("Testing");
    }
}
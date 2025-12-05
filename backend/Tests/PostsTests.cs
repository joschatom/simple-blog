using AutoMapper;
using backend.Controllers;
using backend.Data;
using backend.DTOs.Post.Request;
using backend.DTOs.Post.Response;
using backend.DTOs.Shared.Response;
using backend.Helpers;
using backend.Interfaces;
using backend.Models;
using backend.Profiles;
using backend.Tests;
using Bogus;
using DeepEqual.Syntax;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Tokens;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using System.Collections;
using System.ComponentModel.Design;
using System.Data.Common;
using System.Net;
using System.Net.Http.Headers;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace backend.Tests.Posts;

[TestFixture]
[TestOf(nameof(PostsController.Put))]
class Update : BackendTests
{
    [Test]
    public async Task UpdatePost()
    {
        var user = UserFaker.Generate();
        var post = PostFaker(1, f => f.RuleFor(p => p.UserId, user.Id))
            .First();
        DataContext.SaveChanges();

        var client = _factory.CreateClient();
        client = Authenticated(client, user);
        var updateData = new UpdatePostDTO
        {
            Caption = "Updated Caption",
            Content = "Updated Content"
        };

        var resp = await client.PutAsJsonAsync($"/api/posts/{post.Id}", updateData);
        Assert.That(resp.IsSuccessStatusCode, Is.True);

        var updated = await resp.Content.ReadFromJsonAsync<UpdatedDTO>();

        DataContext.Entry(post).Reload();

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
}

[TestFixture]
[TestOf(nameof(PostsController.Post))]
class Create : BackendTests
{

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
}

[TestFixture]
class Get : BackendTestsWithData
{
    protected override int USERS => 2;
    protected override int POSTS => 6;
    protected override int OWN_POSTS => 2;

    [Test]
    public async Task GetPosts()
    {
        var client = CreateAnonymouseClient();
        
        var resp = await client.GetAsync("/api/posts");

        await AssertForeachDTOJoined<PostDTO, Post>(
            othersPosts,
            resp,
            (expected, got) => Assert.Multiple(() =>
            {
                Assert.That(got, Is.Not.Null);
                Assert.That(got!.Caption, Is.EqualTo(expected.Caption));
                Assert.That(got!.Content, Is.EqualTo(expected.Content));
                Assert.That(got!.UserId, Is.EqualTo(expected.UserId));
                Assert.That(got!.CreatedAt, Is.EqualTo(expected.CreatedAt).Within(1).Seconds);
            }),
            nameof(Post.Id)
        );
    }

    [Test]
    public async Task GetPosts_MutedUsers_Removed()
    {
        var muter = otherUsers[0];

        await UserRepository.MuteUser(muter, actor);

        var client = CreateClient(muter);

        var resp = await client.GetAsync("/api/posts");
        Assert.Multiple(async () =>
        {
            Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            Assert.That(await resp.Content.ReadAsStringAsync(),
                Does.Not.Contains(actor.Id.ToString()));
        });
    }

    [Test]
    public async Task GetPostById()
    {
        var client = CreateAnonymouseClient();

        var resp = await client.GetAsync($"/api/posts/{(Guid)othersPosts[0]}");

        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var got = await resp.Content.ReadFromJsonAsync<PostDTO>();

        got.WithDeepEqual(othersPosts[0])
            .IgnoreUnmatchedProperties()
            .Assert();
    }

    [Test]
    public async Task GetPostById_NotFound()
    {
        var client = CreateAnonymouseClient();

        var resp = await client.GetAsync($"/api/posts/{Guid.Empty}");

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

        Assert.That(post.RegistredUsersOnly, Is.True);

        var client = _factory.CreateClient();
        var resp = await client.GetAsync($"/api/posts/{post.Id}");
        Assert.That(resp.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.Forbidden));
    }

    [Test]
    public async Task GetPostById_RegistredUsersOnly_Authenticated_Succeeds()
    {
        var post = PostFaker(1, f => f
            .RuleFor(p => p.UserId, actor)
            .RuleFor(p => p.RegistredUsersOnly, true))
            .First();

        DataContext.SaveChanges();

        var client = CreateClient();
        var resp = await client.GetAsync($"/api/posts/{post.Id}");
        var got = await resp.Content.ReadFromJsonAsync<PostDTO>();

        got.WithDeepEqual(post)
            .IgnoreUnmatchedProperties()
            .Assert();
    }




}

class BackendTestsWithData: BackendTests
{
    protected virtual int USERS { get; } = 1;
    protected virtual int POSTS { get; } = 0;
    protected virtual int OWN_POSTS { get; } = 0;

    protected User actor = null!;
    protected Post[] ownPosts = [];

    protected User[] otherUsers = [];
    protected Post[] othersPosts = [];

    protected HttpClient CreateClient()
        => base.CreateClient(actor);

    protected virtual Task OnDataCreated() { return Task.CompletedTask; }
    protected virtual Task OnDataCreating() { return Task.CompletedTask; }

    [SetUp]
    public async Task SetupData()
    {
        await OnDataCreating();
    
        actor = UserFaker
            .RuleFor(p => p.Username, _ => "actor")
            .Generate();

        if (USERS > 0) otherUsers = UserFaker
            .RuleFor(p => p.Username, g => $"user-{g.UniqueIndex}")
            .Generate(USERS).ToArray();

        await DataContext.SaveChangesAsync();

        if (POSTS > 0) othersPosts = PostFaker(POSTS)
            .ToArray();

        if (OWN_POSTS > 0) ownPosts = PostFaker(
            OWN_POSTS,
            f => f.RuleFor(p => p.UserId, actor)
        ).ToArray();

        await DataContext.SaveChangesAsync();

        await Reload([actor, .. otherUsers]);
        await Reload([.. ownPosts, .. othersPosts]);
        
        await OnDataCreated();
    }

    [Test]
    public async Task EnsureValidData()
    {
        Assert.Multiple(() =>
        {
            Assert.That(actor, Is.Not.Null);
            Assert.That(otherUsers, Is.Not.Null);
            Assert.That(ownPosts, Is.Not.Null);
        });
        Assert.Multiple(() =>
        {
            Assert.That(otherUsers, Has.Length.AtLeast(USERS));
            Assert.That(othersPosts, Has.Length.AtLeast(POSTS));
            Assert.That(ownPosts, Has.Length.AtLeast(OWN_POSTS));
            Assert.That(actor.Username, Is.EqualTo("actor"));
        });

        Assert.That(
            await UserRepository.ExistsAsync(actor), Is.True,
            $"{actor} doesn't exist in database."
        );

        foreach (var user in otherUsers)
            Assert.That(
                await UserRepository.ExistsAsync(user),
                Is.True, $"{user} doesn't exist in database."
            );

        foreach (var post in (IEnumerable<Post>)[..ownPosts, ..othersPosts])
            Assert.That(
                await PostRepository.ExistsAsync(post),
                Is.True, $"{post} doesn't exist in database."
            );
    }

}

[TestFixture]
[TestOf(nameof(PostsController.Delete))]
class Delete : BackendTests
{
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
        var updatedPosts = DataContext.Posts.ToList();
        Assert.That(updatedPosts, Does.Not.Contain(post));
    }

    [Test]
    public async Task DeletePost_Unauthorized_Fails()
    {
        var user = UserFaker.Generate();
        var otherUser = UserFaker.Generate();

        var post = PostFaker(1, f => f.RuleFor(p => p.UserId, user.Id)).First();
        DataContext.SaveChanges();

        var client = _factory.CreateClient();
        client = Authenticated(client, otherUser);

        var resp = await client.DeleteAsync($"/api/posts/{post.Id}");
        Assert.That(resp.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.Forbidden));

        var updatedPosts = DataContext.Posts.ToList();
        Assert.That(updatedPosts, Does.Contain(post));
    }
}


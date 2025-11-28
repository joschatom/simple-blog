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
class Get : BackendTests<PostsController>
{
    [Test]
    public async Task GetPosts()
    {
        var users = UserFaker.Generate(3);

        DataContext.SaveChanges();

        var expected = PostFaker(5)
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

        Assert.That(post.RegistredUsersOnly, Is.True);

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
            Assert.That(got!.RegistredUsersOnly, Is.True);
        });
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


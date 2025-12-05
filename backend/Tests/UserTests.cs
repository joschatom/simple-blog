using AutoMapper;
using AutoMapper.Configuration.Annotations;
using AutoMapper.Internal;
using backend.Data;
using backend.DTOs.Post.Response;
using backend.DTOs.User.Response;
using backend.Models;
using backend.Repositories;
using DeepEqual.Syntax;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using System;
using System.Collections;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace backend.Tests.Users;

[TestFixture]
class Create : BackendTests { }

[TestFixture]
class Query : BackendTests { }

[TestFixture]
class Delete : BackendTests { }

[TestFixture]
class Update : BackendTests { }

[TestFixture]
class Other : BackendTests
{
    [Test]
    public async Task GetPosts_Unauthenticated()
    {
        var user = UserFaker.Generate();

        var posts = PostFaker(8, (f) => f.RuleFor(p => p.UserId, user.Id))
            .ToArray();

        DataContext.SaveChanges();

        var client = _factory.CreateClient();

        var resp = await client.GetAsync($"/api/users/{user.Id}/posts");

        Assert.That(resp.IsSuccessStatusCode, Is.True);

        var got = await resp.Content.ReadFromJsonAsync<PostDTO[]>();

        var postsDTOs = Service<IMapper>().Map<PostDTO[]>(posts);

        foreach (var post in postsDTOs)
        {
            var gotPost = got!.SingleOrDefault(p => p.Id == post.Id);

            Assert.Multiple(() =>
            {
                Assert.That(gotPost, Is.Not.Null);
                Assert.That(gotPost!.Caption, Is.EqualTo(post.Caption));
                Assert.That(gotPost!.Content, Is.EqualTo(post.Content));
                Assert.That(gotPost!.UserId, Is.EqualTo(post.UserId));
                Assert.That(gotPost!.CreatedAt, Is.EqualTo(post.CreatedAt).Within(1).Seconds);
            });
        }
    }

    [Test]
    public async Task GetMutedUsers()
    {
        var user = UserFaker.Generate();
        var otherUsers = UserFaker.Generate(4);

        await DataContext.SaveChangesAsync();

        await Reload(user);
        await Reload(otherUsers.ToArray());

        foreach (var otherUser in otherUsers)
            await UserRepository.MuteUser(user.Id, otherUser.Id);

        await DataContext.SaveChangesAsync();

        var client = Authenticated(_factory.CreateClient(), user);

        var resp = await client.GetAsync($"/api/muted-users");

        Assert.That(resp.IsSuccessStatusCode, Is.True);

        var result = await resp.Content.ReadFromJsonAsync<MutedUserDTO[]>();

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Has.Length.EqualTo(otherUsers.Count));

        foreach (var (got, expected) in result.Join(otherUsers, p => p.Id, p => p.Id, (a, b) => (a, b)))
        {
            Assert.Multiple(() =>
            {
                Assert.That(got.Id, Is.EqualTo(expected.Id));
                Assert.That(got.Username, Is.EqualTo(expected.Username));
                Assert.That(got.CreatedAt, Is.EqualTo(expected.CreatedAt));
            });
        }
    }

}

[TestFixture]
class Muting : BackendTests
{
    private User actor = null!;
    private User[] otherUsers = null!;

    const int VICTIM_COUNT = 4;

    protected override async Task CreateData()
    {
        actor = UserFaker
            .RuleFor(p => p.Username, _ => "actor")
            .Generate();

        otherUsers = UserFaker
            .RuleFor(p => p.Username, g => $"victim-{g.UniqueIndex}")
            .Generate(VICTIM_COUNT)
            .ToArray();

        await DataContext.SaveChangesAsync();

        await Reload(actor);
        await Reload(otherUsers);
    }

    [Test]
    public async Task EnsureValidData()
    {
        Assert.Multiple(() =>
        {
            Assert.That(actor, Is.Not.Null);
            Assert.That(otherUsers, Is.Not.Null);
        });
        Assert.Multiple(() =>
        {
            Assert.That(otherUsers, Has.Length.EqualTo(VICTIM_COUNT));
            Assert.That(actor.Username, Is.EqualTo("actor"));
        });

        Assert.That(
            await UserRepository.ExistsAsync(actor.Id), Is.True,
            $"The actor user, {actor} doesn't exist in database."
        );

        foreach (var victim in otherUsers)
            Assert.That(
                await UserRepository.ExistsAsync(victim.Id),
                Is.True,
                $"The victim, {victim} doesn't exist in database."
            );
    }

    [Test]
    public async Task GetMutedUsers()
    {

        foreach (var victim in otherUsers)
            await UserRepository.MuteUser(actor.Id, victim.Id);

        await DataContext.SaveChangesAsync();

        var client = Authenticated(_factory.CreateClient(), actor);

        var resp = await client.GetAsync($"/api/muted-users");

        await AssertForeachDTOJoined<MutedUserDTO, User>(
            otherUsers,
            resp,
            (expected, got) => Assert.Multiple(() =>
            {
                Assert.That(got.Id, Is.EqualTo(expected.Id));
                Assert.That(got.Username, Is.EqualTo(expected.Username));
                Assert.That(got.CreatedAt, Is.EqualTo(expected.CreatedAt));
            }),
            nameof(User.Id)
        );
    }

    [Test]
    public async Task MuteUser()
    {
        var target = otherUsers.First()!;

        var client = Authenticated(_factory.CreateClient(), actor);

        var resp = await client.PostAsJsonAsync(
            "/api/muted-users",
            target.Id
        );

        Assert.That(resp.StatusCode, Is.InRange(200, 299));

        var muted = (await UserRepository.GetMutedUsers(actor.Id))
            .Select(u => u.MutedUser.Id)
            .ToArray();

        Assert.That(muted, Is.Not.Null);
        Assert.That(muted, Has.Length.EqualTo(1));
        Assert.That(muted, Does.Contain(target.Id));
    }

    [Test]
    public async Task UnmuteUser()
    {
        var target = otherUsers.First()!;

        await UserRepository.MuteUser(actor.Id, target.Id);

        await DataContext.SaveChangesAsync();

        var client = Authenticated(_factory.CreateClient(), actor);

        var resp = await client.DeleteAsync($"/api/muted-users/{target.Id}");

        Assert.That(resp.StatusCode, Is.InRange(200, 299));

        Assert.That(
            await UserRepository.IsMuted(actor, target),
            Is.False, "Expected target to no longer be muted after API call."
        );
    }

    [Test]
    public async Task IsMuted_Internal()
    {
        TestContext.WriteLine((Guid?)UserByName["actor"]);

        await UserRepository.MuteUser(actor, otherUsers[0]);

        await DataContext.SaveChangesAsync();

        Assert.That(
            await UserRepository.IsMuted(actor, otherUsers[0]),
            Is.True
        );

        Assert.That(
            await UserRepository.IsMuted(actor, otherUsers[1]),
            Is.False
        );
    }


    public class ModelAccessorHelper<TModel, TKey>(
        DataContext context,
        Func<IQueryable<TModel>, TKey, IQueryable<TModel>> predicate
    ) where TModel : class
    {

        public TModel? this[TKey key]
        {
            get => predicate(context.Set<TModel>(), key)
                .FirstOrDefault();

        }
    }

    ModelAccessorHelper<User, string> UserByName
        => new(DataContext, (q, k) => q.Where(u => u.Username == k));


    // ...
}


/// <summary>
/// Constraint for if a status code is succesfull.
/// </summary>
public class SuccessfulStatusCodeConstraint : Constraint
{
    /// <inheritdoc/>
    public override ConstraintResult ApplyTo<TActual>(TActual actual)
    {
        ConstraintStatus result;
        int status;

        if (actual is int n) status = n;
        else if (actual is HttpResponseMessage msg) status = (int)msg.StatusCode;
        else return new ConstraintResult(this, actual, ConstraintStatus.Error);

        if (status >= 200 && status < 299) result = ConstraintStatus.Success;
        else result = ConstraintStatus.Failure;

        Description = "Sucessfull status code.";


        return new ConstraintResult(this, status, result);
    }

}
/*
[System.AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
sealed class IgnoreEqualityAttribute : Attribute;

public class DeepEqualityComparer: DeepEqualityComparer<object>
{
    public static bool CompareRefected(Type type, object a, object b)
    {
        if (a is null) return b is null;
        if (b is null) return a is null;

        if (a.GetType() != b.GetType() || a.GetType() != type)
            throw new ArgumentException(
                $"invalid compare {a.GetType().Name} == {b.GetType().Name} using (reflected) type {type.Name}");

        if (type.IsByRef)
            Is.

    }
}  
public class DeepEqualityComparer<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>: EqualityComparer<T>
{
    private static ImmutableArray<MemberInfo> Members = typeof(T).GetMembers()
        .Where(m => m.GetCustomAttribute<IgnoreEqualityAttribute>() is null)
        .ToImmutableArray();

    public override bool Equals(T? a, T? b)
    {
        foreach (var member in Members)
            if (member.ReflectedType.IsByRef)
                return typeof(DeepEqualityComparer<>).MakeGenericType(member.ReflectedType)
                    .GetConstructor([])
    }

 
}
*/


public class DataTransferObject<TModel>
{
}

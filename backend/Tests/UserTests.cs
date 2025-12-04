using AutoMapper;
using AutoMapper.Configuration.Annotations;
using AutoMapper.Internal;
using backend.DTOs.Post.Response;
using backend.DTOs.User.Response;
using backend.Models;
using Microsoft.VisualBasic;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using System;
using System.Collections;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace backend.Tests.Users;

[TestFixture]
class Create: BackendTests { }

[TestFixture]
class Query: BackendTests { }

[TestFixture]
class Delete: BackendTests { }

[TestFixture]
class Update: BackendTests { }

[TestFixture]
class Other: BackendTests
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

        foreach (var (got, expected) in result.Join(otherUsers, p => p.Id, p => p.Id, (a,b) => (a, b)))
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
class Muting: BackendTests
{
    private User actor = null!;
    private User[] victims = null!;

    const int VICTIM_COUNT = 4;

    protected override async Task CreateData()
    {
        actor = UserFaker
            .RuleFor(p => p.Username, _ => "actor")
            .Generate();

        victims = UserFaker
            .RuleFor(p => p.Username, g => $"victim-{g.UniqueIndex}")
            .Generate(VICTIM_COUNT)
            .ToArray();

        await DataContext.SaveChangesAsync();

        await Reload(actor); 
        await Reload(victims);
    }

    [Test]
    public async Task GetMutedUsers()
    {
        
        foreach (var victim in victims)
            await UserRepository.MuteUser(actor.Id, victim.Id);

        await DataContext.SaveChangesAsync();

        var client = Authenticated(_factory.CreateClient(), actor);

        var resp = await client.GetAsync($"/api/muted-users");

        await AssertForeachDTOJoined<MutedUserDTO, User>(
            victims,
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

    // ...
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


public class DataTransferObject<TModel> :
    IEquatable<TModel>, IEquatable<DataTransferObject<TModel>>
{
    bool IEquatable<TModel>.Equals(TModel? other)
    {
        var compared = 0;

        foreach (var mem in this.GetType().GetProperties())
        {
            var modelProp = typeof(TModel).GetProperty(mem.Name);

            if (modelProp is null)
                continue;

            compared++;

            if (mem.GetValue(this) != modelProp.GetValue(other))
                return false;
        }

        return compared > 0;
    }

    bool IEquatable<DataTransferObject<TModel>>.Equals(DataTransferObject<TModel>? other)
    {
        var compared = 0;

        foreach (var mem in this.GetType().GetProperties())
        {
            var prop = typeof(TModel).GetProperty(mem.Name);

            if (prop is null)
                continue;

            compared++;

            if (mem.GetValue(this) != prop.GetValue(other))
                return false;
        }

        return compared > 0;
    }
}


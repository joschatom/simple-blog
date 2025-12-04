using backend.Models;
using Newtonsoft.Json;
using System.Security.Claims;
using System.Security.Cryptography;
namespace backend.Helpers;

public class Helpers
{
    public static int GenerateRanInt(int min, int max)
    {
        RandomNumberGenerator rng = RandomNumberGenerator.Create();
        byte[] data = new byte[4];
        rng.GetBytes(data);
        int value = Math.Abs(BitConverter.ToInt32(data, 0));
        return min + (value % (max - min));
    }

    /// <summary>
    /// Get a <see cref="ClaimsIdentity"/> for the given user.
    /// </summary>
    /// <param name="user">The user to create an identity for.</param>
    /// <returns>A new <see cref="ClaimsIdentity"/> object.</returns>
    public static ClaimsIdentity GetClaimsIdentity(User user)
    {
        var userData = JsonConvert.SerializeObject(user.Id);
        var claims = new[] {
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.UserData, userData)
        };
       
        return new(claims);
    }
}

using backend.Models;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace backend.Helpers;

public class JwtTokenGenerator
{
    private readonly string _TokenSecret;
    private readonly string _Audience;
    private readonly string _Issuer;

    public JwtTokenGenerator(IConfiguration _configuration)
    {
        string tempTokenHolder = _configuration.GetValue<string>("JWT:TokenSecret")
            ?? throw new KeyNotFoundException("Token Secret");
        string tempAudienceHolder = _configuration.GetValue<string>("JWT:Audience")
            ?? throw new KeyNotFoundException("JWT Audience");
        string tempIssuerHolder = _configuration.GetValue<string>("JWT:Issuer")
            ?? throw new KeyNotFoundException("Token Issuer");

        _TokenSecret = tempTokenHolder;
        _Audience = tempAudienceHolder;
        _Issuer = tempIssuerHolder;
    }

    public string GenerateToken(User user, int daysToExpire = 5) // default 5 days can be changed
    {
        var key = Encoding.UTF8.GetBytes(_TokenSecret);
        var userData = JsonConvert.SerializeObject(user.Id);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[] { 
                new Claim(ClaimTypes.UserData, userData),
                new Claim(ClaimTypes.Name, user.Username)
            }),
            Expires = DateTime.UtcNow.AddDays(daysToExpire),
            Issuer = _Issuer,
            Audience = _Audience,
            SigningCredentials = new SigningCredentials
            (new SymmetricSecurityKey(key),
            SecurityAlgorithms.HmacSha512Signature),
            IssuedAt = DateTime.Now
        };
        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        var stringToken = tokenHandler.WriteToken(token);
        return stringToken;
    }
}
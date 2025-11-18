
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace backend.Helpers;

public class JwtTokens
{
    private static readonly IConfigurationRoot _configuration;
    private static readonly string _TokenSecret;
    private static readonly string _Audience;
    private static readonly string _Issuer;

    static JwtTokens()
    {
        _configuration = new ConfigurationBuilder()
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .Build();
        string? tempTokenHolder = _configuration.GetValue<string>("JWT:TokenSecret");
        string? tempAudienceHolder = _configuration.GetValue<string>("JWT:Audience");
        string? tempIssuerHolder = _configuration.GetValue<string>("JWT:Issuer");
        if (tempTokenHolder == null || tempAudienceHolder == null || tempIssuerHolder == null)
        {
            throw new ArgumentNullException();
        }
        else
        {
            _TokenSecret = tempTokenHolder;
            _Audience = tempAudienceHolder;
            _Issuer = tempIssuerHolder;
        }

    }

    public static string GenerateToken(Guid userId, int daysToExpire = 5) // default 5 days can be changed
    {
        var key = Encoding.UTF8.GetBytes(_TokenSecret);
        var userData = JsonConvert.SerializeObject(userId);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[] { new Claim(ClaimTypes.UserData, userData), }),
            Expires = DateTime.UtcNow.AddDays(daysToExpire),
            Issuer = _Issuer,
            Audience = _Audience,
            SigningCredentials = new SigningCredentials
            (new SymmetricSecurityKey(key),
            SecurityAlgorithms.HmacSha512Signature)
        };
        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        var stringToken = tokenHandler.WriteToken(token);
        return stringToken;
    }
}
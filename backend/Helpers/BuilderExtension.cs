using backend;
using backend.Data;
using backend.Helpers;
using backend.Interfaces;
using backend.Models;
using backend.Profiles;
using backend.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NUnit.Framework;
using System;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace backend.Helpers;

internal static class BuilderExtension
{
    public static void AddBackendServices(this IServiceCollection services, IConfiguration config)
    {
        services.AddAutoMapper(o => {
           o.AddProfile<UserProfile>();
            o.AddProfile<PostProfile>();
          
        });
        
        services.AddSingleton<JwtTokenGenerator>();

       // services.AddSingleton<IConfiguration>(config);

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IPostRepository, PostRepository>();

        

        // Add JWT Authentication
        services.AddAuthentication(o =>
        {
            o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            o.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(jwtConfig =>
        {
            
            string? _TokenSecret = config.GetValue<string>("JWT:TokenSecret") ?? throw new NullReferenceException("missing token secret!");
            Debug.WriteLine($"Token Validation Secret: {_TokenSecret}");
            var key = Encoding.UTF8.GetBytes(_TokenSecret);
            jwtConfig.SaveToken = true;
            jwtConfig.TokenValidationParameters = new TokenValidationParameters
            {
                ValidIssuer = config.GetValue<string>("JWT:Issuer"),
                ValidAudience = config.GetValue<string>("JWT:Audience"),
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateIssuerSigningKey = true,
            };
        });
    }
}

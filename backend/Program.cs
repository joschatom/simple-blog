using backend;
using backend.Data;
using backend.Helpers;
using backend.Interfaces;
using backend.Profiles;
using backend.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddUserSecrets<IBlogBackendMarker>(false, true);

ConfigurationManager _configuration = builder.Configuration;
builder.Services.AddSingleton<IConfiguration>(_configuration);

// Add services to the container.
builder.Services.AddControllers();

builder.Services.AddBackendServices(_configuration);

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement { { new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } }, Array.Empty<string>() } });
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Blog API", Version = "v1" });
});

if (builder.Environment.EnvironmentName != "Testing")
{
    builder.Services.AddDbContext<DataContext>(options => {
        options.UseSqlServer(builder.Configuration.GetConnectionString("Default"));
    });
}
    

builder.Services.AddHealthChecks();

var app = builder.Build();

// add health checks endpoint
app.MapHealthChecks("/health");
app.UseCors(x => x
  .AllowAnyMethod()
  .AllowAnyHeader()
  .AllowAnyOrigin());

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();


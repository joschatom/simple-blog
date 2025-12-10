using AutoMapper;
using backend.DTOs.User.Request;
using backend.DTOs.User.Response;
using backend.Helpers;
using backend.Interfaces;
using backend.Models;
using backend.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.IdentityModel.JsonWebTokens;
using System.ComponentModel;
using System.Security.Claims;

namespace backend.Controllers;

[Authorize]
[ApiController]
[Route("/api/auth")]
public class AuthController(
    IUserRepository repository,
    IMapper mapper,
    JwtTokenGenerator jwtTokenGenerator
    ) : ControllerBase
{
    [EndpointDescription("Validates the JWT token.")]
    [HttpGet("validate")]
    public ActionResult<bool> Validate() => Ok(true);

    [EndpointDescription("Logs in a user and returns the user info and JWT token.")]
    [AllowAnonymous] // This will allow the user to access this method without being authenticated (without JWT Token)
    [HttpPost, Route("login")]
    public async Task<ActionResult<AuthUserDTO>> Login(LoginUserDTO login)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        User? userFromDB = await repository.GetByNameAsync(login.Username);

        if (userFromDB == null) return Unauthorized("Invalid username or password");
        else if (userFromDB.Token != null)
            Console.WriteLine("[WARN] User already logged in. Existing token will be replaced.");

        if (!PasswordHasher.CompareHashAndPassword(userFromDB.PasswordHash, login.Password))
        {
            return Unauthorized("Invalid username or password");
        }

        AuthUserDTO loginResponseDTO = mapper.Map<AuthUserDTO>(userFromDB);
        loginResponseDTO.Token = jwtTokenGenerator.GenerateToken(userFromDB);

        userFromDB.LastLogin = DateTime.UtcNow;
        userFromDB.Token = loginResponseDTO.Token;
        await repository.UpdateAsync(userFromDB.Id, userFromDB);

        await repository.SaveChangesAsync();


        return Ok(loginResponseDTO);
    }

    [EndpointDescription("Registers a new user and returns a user info as wll as a token for that new users")]
    [AllowAnonymous]
    [HttpPost, Route("register")]
    public async Task<ActionResult<AuthUserDTO>> Register(RegisterUserDTO register)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        User? userAlreadyExists = await repository.GetByNameAsync(register.Username);
        if (userAlreadyExists != null)
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Type = "Bad Request",
                Title = "Username Already in Use",
                Detail = "The username provided is already associated with an existing account."
            });
        }

        userAlreadyExists = await repository.GetByEmailAsync(register.Email);
        if (userAlreadyExists != null)
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Type = "Bad Request",
                Title = "Email Already in Use",
                Detail = "The email provided is already associated with an existing account."
            });
        }

        User userToAddToDB = mapper.Map<User>(register);
        userToAddToDB.PasswordHash = PasswordHasher.HashPassword(register.Password);
        userToAddToDB.CreatedAt = DateTime.UtcNow;
        User? responseAddUser = await repository.CreateAsync(userToAddToDB);
        if (responseAddUser != null && (await repository.SaveChangesAsync()))
        {
            string token = jwtTokenGenerator.GenerateToken(responseAddUser);
            AuthUserDTO userToReturn = mapper.Map<AuthUserDTO>(responseAddUser);
            userToReturn.Token = token;
            return Ok(userToReturn);
        }
        else
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Type = "Bad Request",
                Title = "Error while registering new user",
                Detail = "There was an error while registering the new user."
            });
        }
    }

    [EndpointDescription("Changes the password for the authenticated user.")]
    [HttpPost, Route("change-password")]
    public async Task<ActionResult> ChangePassword(ChangePasswordDTO request)
    {
        var user = await this.CurrentUser();

        if (user is null) return Unauthorized();

        var passwordHash = PasswordHasher.HashPassword(request.Password);

        user.PasswordHash = passwordHash;

        await repository.UpdateAsync(user.Id, user);
        await repository.SaveChangesAsync();

        return Ok();
    }

    [EndpointDescription("Refresh Token")]
    [HttpPost, Route("refresh-token")]
    public async Task<ActionResult<AuthUserDTO>> RefreshToken()
    {
        var user = await this.CurrentUser();

        if (user is null) return Unauthorized();

        user.Token = jwtTokenGenerator.GenerateToken(user);

        user = await repository.UpdateAsync(user.Id, user);

        await repository.SaveChangesAsync();

        if (user is null) return StatusCode(StatusCodes.Status500InternalServerError);

        return mapper.Map<AuthUserDTO>(user); 
    }

    [EndpointDescription("Logout current user.")]
    [HttpPost, Route("logout")]
    public async Task<ActionResult> Logout()
    {
        var user = await this.CurrentUser();

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        user!.Token = null;

        await repository.UpdateAsync(user.Id, user);
        await repository.SaveChangesAsync();

        return Ok();
    }
}


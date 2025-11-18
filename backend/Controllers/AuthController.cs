using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[Authorize]
[ApiController]
[Route("/api/v1/auth")]
public class AuthController : ControllerBase
{
    [EndpointDescription("Validates the JWT token.")]
    [HttpGet("validate")]
    public ActionResult<bool> Validate() => Ok(true);

    [EndpointDescription("Logs in a user and returns the user info and JWT token.")]
    [AllowAnonymous] // This will allow the user to access this method without being authenticated (without JWT Token)
    [HttpPost, Route("login")]
    public async Task<ActionResult> Login()
    {
        throw new NotImplementedException();
    }

    [EndpointDescription("Registers a new user and returns a user info as wll as a token for that new users")]
    [AllowAnonymous]
    [HttpPost, Route("register")]
    public async Task<ActionResult> Register()
    {
        throw new NotImplementedException();
    }

    [EndpointDescription("Changes the password for the authenticated user.")]
    [HttpPost, Route("change-password")]
    public async Task<ActionResult> ChangePassword()
    {
        throw new NotImplementedException();
    }
}

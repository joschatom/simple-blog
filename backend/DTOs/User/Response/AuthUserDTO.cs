namespace backend.DTOs.User.Response;

/// <summary>
/// A response that include both user data and a JWT Token.
/// </summary>
public class AuthUserDTO: UserDTO
{
    /// <summary>
    /// The JWT Token for the session.
    /// </summary>
    /// <remarks>    
    /// See also <seealso cref="Helpers.JwtTokenGenerator.GenerateToken"/>
    /// for how it's generated.
    /// </remarks>
    public required string Token { get; set; }  
}

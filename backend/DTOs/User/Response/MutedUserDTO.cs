namespace backend.DTOs.User.Response;

/// <summary>
/// DTO for a user that has been muted by another user.
/// </summary>
/// <remarks>
/// Inherits all fields from <see cref="PublicUserDTO"/> currently.
/// May be expanded with things like date of muting, reason, etc. later on.
/// </remarks>
public class MutedUserDTO: PublicUserDTO { }

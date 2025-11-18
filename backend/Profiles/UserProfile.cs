namespace backend.Profiles;

public class UserProfile: AutoMapper.Profile
{
    public UserProfile()
    {
        CreateMap<backend.DTOs.User.Request.RegisterUserDTO, backend.Models.User>();
        CreateMap<backend.DTOs.User.Request.LoginUserDTO, backend.Models.User>();
        CreateMap<backend.Models.User, backend.DTOs.User.Response.UserDTO>()
            .ReverseMap();
        CreateMap<backend.Models.User, backend.DTOs.User.Response.AuthUserDTO>()
            .ReverseMap();
        CreateMap<backend.Models.User, backend.DTOs.User.Response.PublicUserDTO>();
    }
}

using AutoMapper.Internal;
using backend.DTOs.User.Response;

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

        CreateMap<Models.MuteUser, MutedUserDTO>()
            .BeforeMap((src, dest, ctx) =>
            {
                var mapped = ctx.Mapper.Map<PublicUserDTO>(src.MutedUser);

                foreach (var property in typeof(PublicUserDTO).GetProperties())
                {
                    var value = property.GetValue(mapped);
                    var destProperty = typeof(MutedUserDTO).GetInheritedProperty(property.Name);
                    destProperty.SetValue(dest, value); 
                }
            })
            .ForAllMembers(c =>
            {
                if (c.DestinationMember.DeclaringType == typeof(PublicUserDTO))
                    c.Ignore();
            });
    }
}

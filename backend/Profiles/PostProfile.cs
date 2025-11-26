namespace backend.Profiles;

public class PostProfile: AutoMapper.Profile
{
    public PostProfile()
    {
        CreateMap<backend.DTOs.Post.Request.CreatePostDTO, backend.Models.Post>();
        CreateMap<backend.DTOs.Post.Request.UpdatePostDTO, backend.Models.Post>()
            .ForMember(m => m.Content, opt => opt.Condition(src => src.Content != null));
        CreateMap<backend.Models.Post, backend.DTOs.Post.Response.PostDTO>()
            .ReverseMap();
    }
}

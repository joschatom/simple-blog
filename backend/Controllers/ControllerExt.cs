using backend.Interfaces;
using backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;
using System.Diagnostics.Metrics;
using System.Reflection;
using System.Security.Claims;

namespace backend.Controllers;

public class ControllerExt: ControllerBase
{

}

public class CurrentUserValueProvider(IUserRepository repository) : IModelBinder, IModelBinderProvider
{
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        if (context.Metadata.ModelType == typeof(User) 
            )
        {
            _ = context.Metadata.ModelType;

            return new CurrentUserValueProvider(context.Services.GetService<IUserRepository>()!);
        }
        return null;
    }
    public async Task BindModelAsync(ModelBindingContext bindingContext)
    {



    }
}
public static class ControllerExtensions
{
    public static Guid GetCurrentUserId(this ControllerBase controller)
    {
        var userData = controller.User.FindFirstValue(ClaimTypes.UserData)
            ?? throw new Exception("Invalid Token");
        var id = JsonConvert.DeserializeObject<Guid>(userData!);
        return id;
    }

    public static async Task<User?> CurrentUser(this ControllerBase controller) {


        var identity = controller.User.Identity;
        if (identity is null || !identity.IsAuthenticated || identity.Name is null)
        {
            return null;
        }

        var user = await controller.HttpContext.RequestServices
            .GetService<IUserRepository>()!.GetByNameAsync(identity.Name);

        if (user is null)
        {
            controller.ModelState.AddModelError("CurrentUser", "User not found");
            return null;
        }

        return user;
    }
}

using backend.Interfaces;
using backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;
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

        bindingContext.BindingSource = BindingSource.Special;
        

        var identity = bindingContext.HttpContext.User.Identity;
        if (identity is null || !identity.IsAuthenticated || identity.Name is null)
        {
            bindingContext.Result = ModelBindingResult.Failed();
            return;
        }
       
        var user = await repository.GetByNameAsync(identity.Name);

        if (user is null)
        {
            bindingContext.Result = ModelBindingResult.Failed();
            bindingContext.ModelState.AddModelError(bindingContext.ModelName, "User not found");
            return;
        }

        bindingContext.Result = ModelBindingResult.Success(user);
        return;
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
}

[System.AttributeUsage(AttributeTargets.Parameter )]
sealed class CurrentUserAttribute : Attribute
{

}
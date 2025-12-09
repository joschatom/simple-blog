using backend.Interfaces;
using backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;
using System.Diagnostics.Metrics;
using System.Reflection;
using System.Security.Claims;

namespace backend.Controllers;

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
        if (identity is null || !identity.IsAuthenticated)
            return null;

        var userData = controller.User.FindFirstValue(ClaimTypes.UserData);

        if (userData is null) return null;

        var userID = JsonConvert.DeserializeObject<Guid>(userData);

        var user = await controller.HttpContext.RequestServices
            .GetService<IUserRepository>()!.GetByIdAsync(userID);

        if (user is null)
        {
            controller.ModelState.AddModelError("CurrentUser", "User not found");
            return null;
        }

        return user;
    }
}

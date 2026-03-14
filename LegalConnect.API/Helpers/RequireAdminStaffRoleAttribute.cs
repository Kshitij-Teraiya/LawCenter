using LegalConnect.API.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace LegalConnect.API.Helpers;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
public class RequireAdminStaffRoleAttribute : Attribute, IAuthorizationFilter
{
    private readonly AdminStaffRole[] _requiredRoles;

    public RequireAdminStaffRoleAttribute(params AdminStaffRole[] requiredRoles)
    {
        _requiredRoles = requiredRoles;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;
        if (!(user.Identity?.IsAuthenticated ?? false))
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        // Full Admin (Super Admin) always has access
        var primaryRole = user.FindFirstValue(ClaimTypes.Role);
        if (primaryRole == "Admin")
            return;

        // AdminStaff must have at least one of the required sub-roles
        if (primaryRole == "AdminStaff")
        {
            var staffRoles = user.FindAll("adminStaffRole").Select(c => c.Value).ToHashSet();
            if (_requiredRoles.Any(required => staffRoles.Contains(required.ToString())))
                return;
        }

        context.Result = new ForbidResult();
    }
}

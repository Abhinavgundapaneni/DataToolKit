using DataToolKit.Areas.Identity.Data;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;


namespace DataToolKit.DataAccess
{
    public class Helper
    {

        public bool isLoggedInUserAdmin(ClaimsPrincipal user)
        {
            if (user.Identity?.IsAuthenticated ?? false)
            {
                return  user.FindAll(ClaimTypes.Role)
                                .ToList()
                                .Exists(x => x.Value == "Admin");
            }
            return false;
        }
    }
}
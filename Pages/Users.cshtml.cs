using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using DataToolKit.Models;
using DataToolKit.DataAccess;
using System.Globalization;
using System.Text;
using X.PagedList;
using System.Data.SqlClient;
using DataToolKit.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;

namespace DataToolKit.Pages.Home
{
    [Authorize(Policy = "AdminOnly")]
    public class UsersModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        private readonly UserManager<DataToolKitUser> _userManager;
        public List<DataToolKitUser> UsersList { get; set; }
        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "New Password")]
            public string NewPassword { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("NewPassword", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }
        }


        public UsersModel(UserManager<DataToolKitUser> userManager, ILogger<IndexModel> logger)
        {
            _logger = logger;
            _userManager = userManager;
            UsersList = new List<DataToolKitUser>();
        }

        public Task OnGetAsync()
        {
            UsersList = _userManager.Users.ToList();
            return Task.CompletedTask;
        }

        public async Task<IActionResult> OnPostResetPasswordAsync(string id, string returnUrl = null)
        {
            try
            {
                returnUrl ??= Url.Content("~/");
                var user = await _userManager.FindByIdAsync(id);
                var newPassword = Input.NewPassword;
                if (await _userManager.HasPasswordAsync(user) == true)
                {
                    await _userManager.RemovePasswordAsync(user);

                }
                 var result = await _userManager.AddPasswordAsync(user, newPassword);
                return LocalRedirect(returnUrl);
            }
            catch
            {
                return Page();
            }

        }

        public async Task<IActionResult> OnPostDeleteAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                var result = await _userManager.DeleteAsync(user);
                if (!result.Succeeded)
                {
                    return RedirectToPage("/Users");
                }
            }
            return RedirectToPage("/Users");

        }


    }
}


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

namespace DataToolKit.Pages.Account
{
    public class AccountModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        private readonly UserManager<DataToolKitUser> _userManager;

        [BindProperty]
        public InputModel? Input { get; set; }

        public class InputModel
        {
            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Current Password")]
            public string? CurrentPassword { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "New Password")]
            public string? NewPassword { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("NewPassword", ErrorMessage = "The password and confirmation password do not match.")]
            public string? ConfirmPassword { get; set; }
        }


        public AccountModel(UserManager<DataToolKitUser> userManager, ILogger<IndexModel> logger)
        {
            _logger = logger;
            _userManager = userManager;
        }

        public Task OnGetAsync()
        {
            TempData["Response"] = null;
            return Task.CompletedTask;
        }

        public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
        {
            try
            {
                returnUrl ??= Url.Content("~/");
                var user = await _userManager.FindByNameAsync(User.Identity?.Name);
                var newPassword = Input?.NewPassword;
                if (await _userManager.CheckPasswordAsync(user, Input?.CurrentPassword) == true)
                {
                    await _userManager.RemovePasswordAsync(user);
                    await _userManager.AddPasswordAsync(user, newPassword);
                    TempData["Response"] = "Success";
                }
                else
                {
                    TempData["Response"] = "Failed";
                }
            }
            catch
            {
                TempData["Response"] = "Failed";
            }
            return Page();

        }
    }
}


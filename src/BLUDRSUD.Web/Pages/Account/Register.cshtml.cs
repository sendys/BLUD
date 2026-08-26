using System.ComponentModel.DataAnnotations;
using BLUDRSUD.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BLUDRSUD.Web.Pages.Account;

[AllowAnonymous]
public class RegisterModel : PageModel
{
    private readonly UserManager<ApplicationUser> _users;
    private readonly ILogger<RegisterModel> _log;

    public RegisterModel(UserManager<ApplicationUser> users, ILogger<RegisterModel> log)
    {
        _users = users;
        _log = log;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        [Required(ErrorMessage = "Nama lengkap wajib diisi")]
        [Display(Name = "Nama Lengkap")]
        public string FullName { get; set; } = "";

        [Required(ErrorMessage = "Username wajib diisi")]
        [Display(Name = "Username")]
        public string Username { get; set; } = "";

        [Required(ErrorMessage = "Email wajib diisi")]
        [EmailAddress(ErrorMessage = "Format email tidak valid")]
        [Display(Name = "Email")]
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "Password wajib diisi")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Password minimal 8 karakter")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = "";

        [Required(ErrorMessage = "Konfirmasi password wajib diisi")]
        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "Konfirmasi password tidak sama")]
        [Display(Name = "Konfirmasi Password")]
        public string ConfirmPassword { get; set; } = "";
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        if (await _users.FindByNameAsync(Input.Username) != null)
        {
            ModelState.AddModelError(nameof(Input.Username), "Username sudah digunakan.");
            return Page();
        }

        if (await _users.FindByEmailAsync(Input.Email) != null)
        {
            ModelState.AddModelError(nameof(Input.Email), "Email sudah digunakan.");
            return Page();
        }

        var user = new ApplicationUser
        {
            FullName = Input.FullName.Trim(),
            UserName = Input.Username.Trim(),
            Email = Input.Email.Trim(),
            IsActive = true,
            CreatedBy = "SYSTEM",
            CreatedAt = DateTime.UtcNow
        };

        try
        {
            var result = await _users.CreateAsync(user, Input.Password);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    var field = error.Code.StartsWith("Password", StringComparison.OrdinalIgnoreCase)
                        ? nameof(Input.Password)
                        : string.Empty;
                    ModelState.AddModelError(field, error.Description);
                }

                return Page();
            }

            TempData["SuccessMessage"] = "Registrasi berhasil. Silakan login menggunakan akun Anda.";
            _log.LogInformation("New user registered: {Username}", user.UserName);
            return RedirectToPage("/Account/Login");
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "User registration failed for {Username}", user.UserName);
            ModelState.AddModelError(string.Empty, "Registrasi gagal. Silakan coba lagi.");
            return Page();
        }
    }
}

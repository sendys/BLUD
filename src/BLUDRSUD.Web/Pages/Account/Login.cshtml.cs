using System.ComponentModel.DataAnnotations;
using BLUDRSUD.Infrastructure.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BLUDRSUD.Web.Pages.Account;

public class LoginModel : PageModel
{
    private readonly SignInManager<ApplicationUser> _signIn;
    private readonly UserManager<ApplicationUser> _users;
    private readonly ILogger<LoginModel> _log;

    public LoginModel(SignInManager<ApplicationUser> signIn, UserManager<ApplicationUser> users, ILogger<LoginModel> log)
    {
        _signIn = signIn; _users = users; _log = log;
    }

    [BindProperty] public InputModel Input { get; set; } = new();
    public string? ReturnUrl { get; set; }
    public string? ErrorMessage { get; set; }
    [TempData] public string? SuccessMessage { get; set; }

    public class InputModel
    {
        [Required(ErrorMessage = "Username wajib diisi")]
        public string Username { get; set; } = "";
        [Required(ErrorMessage = "Password wajib diisi")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = "";
    }

    public async Task OnGetAsync(string? returnUrl = null)
    {
        ReturnUrl = returnUrl ?? Url.Content("~/Dashboard");
        await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        ReturnUrl = returnUrl ?? Url.Content("~/Dashboard");
        if (!ModelState.IsValid) return Page();

        var user = await _users.FindByNameAsync(Input.Username);
        if (user == null || !user.IsActive)
        {
            _log.LogWarning("Login failed for {Username}", Input.Username);
            ModelState.AddModelError(string.Empty, "Username atau password salah.");
            return Page();
        }

        var result = await _signIn.PasswordSignInAsync(user, Input.Password, isPersistent: false, lockoutOnFailure: true);
        if (result.Succeeded)
        {
            user.LastLoginAt = DateTime.UtcNow;
            user.LastLoginIpAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            await _users.UpdateAsync(user);
            _log.LogInformation("User {Username} logged in.", Input.Username);
            return LocalRedirect(ReturnUrl);
        }
        if (result.IsLockedOut)
        {
            _log.LogWarning("User {Username} locked out.", Input.Username);
            ModelState.AddModelError(string.Empty, "Akun terkunci. Coba beberapa saat lagi.");
            return Page();
        }

        ModelState.AddModelError(string.Empty, "Username atau password salah.");
        return Page();
    }
}

public class LogoutModel : PageModel
{
    private readonly SignInManager<ApplicationUser> _signIn;
    private readonly ILogger<LogoutModel> _log;
    public LogoutModel(SignInManager<ApplicationUser> signIn, ILogger<LogoutModel> log) { _signIn = signIn; _log = log; }

    public async Task<IActionResult> OnPostAsync()
    {
        var name = User.Identity?.Name;
        await _signIn.SignOutAsync();
        _log.LogInformation("User {Username} logged out.", name);
        return RedirectToPage("/Account/Login");
    }
}

public class AccessDeniedModel : PageModel { public void OnGet() { } }

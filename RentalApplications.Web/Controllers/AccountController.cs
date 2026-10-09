using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RentalApplications.Data;
using RentalApplications.Domain;
using RentalApplications.Models.Account;

namespace RentalApplications.Controllers;

/// <summary>
/// Sign-in, registration, and sign-out for ASP.NET Core Identity users.
/// </summary>
public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ILogger<AccountController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="AccountController"/> class.
    /// </summary>
    public AccountController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ILogger<AccountController> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _logger = logger;
    }

    /// <summary>
    /// Displays the login form.
    /// </summary>
    /// <param name="returnUrl">Optional local URL to redirect to after a successful login.</param>
    /// <returns>The login view.</returns>
    [HttpGet]
    public IActionResult Login(string? returnUrl = null) =>
        View(new LoginViewModel { ReturnUrl = returnUrl });

    /// <summary>
    /// Authenticates a user with email and password.
    /// </summary>
    /// <param name="model">Credentials and optional return URL.</param>
    /// <returns>Redirects on success; otherwise redisplays the login view with validation errors.</returns>
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, false, lockoutOnFailure: false);
        if (!result.Succeeded)
        {
            _logger.LogWarning("Failed login attempt for {Email}", model.Email);
            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return View(model);
        }

        _logger.LogInformation("User {Email} signed in", model.Email);

        if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
        {
            return Redirect(model.ReturnUrl);
        }

        return RedirectToAction("Index", "Home");
    }

    /// <summary>
    /// Displays the registration form.
    /// </summary>
    /// <returns>The registration view.</returns>
    [HttpGet]
    public IActionResult Register() => View(new RegisterViewModel());

    /// <summary>
    /// Creates a new user account and signs the user in.
    /// </summary>
    /// <param name="model">Profile, credentials, and selected role (Applicant or Property Manager).</param>
    /// <returns>Redirects to home on success; otherwise redisplays the registration view.</returns>
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (model.Role is not (AppRoles.Applicant or AppRoles.PropertyManager))
        {
            ModelState.AddModelError(nameof(model.Role), "Select a valid role.");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            DisplayName = model.DisplayName
        };

        var createResult = await _userManager.CreateAsync(user, model.Password);
        if (!createResult.Succeeded)
        {
            _logger.LogWarning("Registration failed for {Email}", model.Email);
            foreach (var error in createResult.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        await _userManager.AddToRoleAsync(user, model.Role);
        await _signInManager.SignInAsync(user, isPersistent: false);
        _logger.LogInformation("Registered user {UserId} with role {Role}", user.Id, model.Role);
        return RedirectToAction("Index", "Home");
    }

    /// <summary>
    /// Signs out the current user.
    /// </summary>
    /// <returns>Redirects to the home page.</returns>
    [Authorize, HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        _logger.LogInformation("User {UserId} signed out", _userManager.GetUserId(User));
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    /// <summary>
    /// Displays the access-denied page when authorization fails.
    /// </summary>
    /// <returns>The access-denied view.</returns>
    [HttpGet]
    public IActionResult AccessDenied() => View();
}

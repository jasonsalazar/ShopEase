using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ShopEase.Application.Interfaces;
using ShopEase.Application.Services;
using ShopEase.Infrastructure.Identity;
using ShopEase.Web.Models.Account;
using ShopEase.Web.Services;

namespace ShopEase.Web.Controllers;

public class AccountController(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    ShoppingCartService shoppingCartService,
    IShopEaseEmailSender emailSender) : Controller
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly SignInManager<ApplicationUser> _signInManager = signInManager;
    private readonly ShoppingCartService _shoppingCartService = shoppingCartService;
    private readonly IShopEaseEmailSender _emailSender = emailSender;

    private const string CartCookieName = "ShopEase.CartKey";

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Register()
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Home");

        return View(new RegisterViewModel());
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(
        RegisterViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var user = new ApplicationUser
        {
            UserName = model.Email.Trim(),
            Email = model.Email.Trim(),
            FullName = model.FullName.Trim()
        };

        var result = await _userManager.CreateAsync(
            user, model.Password);

        if (result.Succeeded)
        {
            var guestCartKey = ReadGuestCartKey();
            var customerCartKey = EnsureCustomerCartKey(user);

            var updateResult = await _userManager.UpdateAsync(user);

            if (!updateResult.Succeeded)
            {
                foreach (var error in updateResult.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);

                return View(model);
            }

            var isSmtpConfigured = false; // configure smtp first

            if (isSmtpConfigured)
            {
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

                var encodedToken = Microsoft.AspNetCore.WebUtilities.WebEncoders
                    .Base64UrlEncode(System.Text.Encoding.UTF8.GetBytes(token));

                var confirmationUrl = Url.Action(
                    nameof(ConfirmEmail),
                    "Account",
                    new { userId = user.Id, token = encodedToken },
                    Request.Scheme);

                if (confirmationUrl is null)
                {
                    ModelState.AddModelError(
                        string.Empty, "Unable to generate the confirmation link.");
                    return View(model);
                }

                await _emailSender.SendAsync(
                    user.Email!,
                    "Confirm your ShopEase account",
                    $"<p>Welcome to ShopEase.</p>" +
                    $"<p><a href=\"{System.Net.WebUtility.HtmlEncode(confirmationUrl)}\">" +
                    "Confirm your email address</a></p>");

                return RedirectToAction(nameof(RegistrationConfirmation));
            }
            else
            {
                if (guestCartKey is not null)
                {
                    await _shoppingCartService.MergeAsync(
                        guestCartKey, customerCartKey);
                }

                SetCartCookie(customerCartKey);

                await _signInManager.SignInAsync(user, isPersistent: false);

                return RedirectToAction("Index", "Home");
            }
        }

        foreach (var error in result.Errors)
            ModelState.AddModelError(string.Empty, error.Description);

        return View(model);
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Home");

        return View(new LoginViewModel
        {
            ReturnUrl = returnUrl
        });
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var user = await _userManager.FindByEmailAsync(
            model.Email.Trim());

        if (user is null)
        {
            ModelState.AddModelError(
                string.Empty, "Invalid email or password.");

            return View(model);
        }

        var result = await _signInManager.PasswordSignInAsync(
            user.UserName!,
            model.Password,
            model.RememberMe,
            lockoutOnFailure: true);

        if (result.Succeeded)
        {
            var signedInUser = await _userManager.FindByIdAsync(user.Id);

            if (signedInUser is null)
            {
                ModelState.AddModelError(
                    string.Empty, "Unable to load your account.");
                return View(model);
            }

            var guestCartKey = ReadGuestCartKey();
            var customerCartKey = EnsureCustomerCartKey(signedInUser);

            var updateResult = await _userManager.UpdateAsync(signedInUser);

            if (!updateResult.Succeeded)
            {
                foreach (var error in updateResult.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);

                await _signInManager.SignOutAsync();
                return View(model);
            }

            if (guestCartKey is not null)
            {
                await _shoppingCartService.MergeAsync(
                    guestCartKey, customerCartKey);
            }

            SetCartCookie(customerCartKey);

            if (!string.IsNullOrWhiteSpace(model.ReturnUrl) &&
                Url.IsLocalUrl(model.ReturnUrl))
            {
                return LocalRedirect(model.ReturnUrl);
            }

            return RedirectToAction("Index", "Home");
        }

        ModelState.AddModelError(
            string.Empty,
            result.IsLockedOut
                ? "Your account is temporarily locked. Try again later."
                : "Invalid email or password.");

        return View(model);
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult RegistrationConfirmation()
    {
        return View();
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> ConfirmEmail(
        string? userId,
        string? token)
    {
        if (string.IsNullOrWhiteSpace(userId) ||
            string.IsNullOrWhiteSpace(token))
        {
            return BadRequest();
        }

        var user = await _userManager.FindByIdAsync(userId);

        if (user is null)
            return NotFound();

        try
        {
            var decodedToken = System.Text.Encoding.UTF8.GetString(
                Microsoft.AspNetCore.WebUtilities.WebEncoders
                    .Base64UrlDecode(token));

            var result = await _userManager.ConfirmEmailAsync(
                user, decodedToken);

            if (result.Succeeded)
                return View("EmailConfirmed");

            return View("EmailConfirmationFailed");
        }
        catch (FormatException)
        {
            return BadRequest();
        }
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult ForgotPassword()
    {
        return View(new ForgotPasswordViewModel());
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(
        ForgotPasswordViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var user = await _userManager.FindByEmailAsync(model.Email.Trim());

        // Always show the same response to avoid revealing whether
        // an email address has an account.
        if (user is not null && await _userManager.IsEmailConfirmedAsync(user))
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            var encodedToken =
                Microsoft.AspNetCore.WebUtilities.WebEncoders.Base64UrlEncode(
                    System.Text.Encoding.UTF8.GetBytes(token));

            var resetUrl = Url.Action(
                nameof(ResetPassword),
                "Account",
                new { userId = user.Id, token = encodedToken },
                Request.Scheme);

            if (resetUrl is not null)
            {
                await _emailSender.SendAsync(
                    user.Email!,
                    "Reset your ShopEase password",
                    $"<p>To reset your password, follow this link:</p>" +
                    $"<p><a href=\"{System.Net.WebUtility.HtmlEncode(resetUrl)}\">" +
                    "Reset password</a></p>" +
                    "<p>If you did not request this, you can ignore this email.</p>");
            }
        }

        return RedirectToAction(nameof(ForgotPasswordConfirmation));
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult ForgotPasswordConfirmation()
    {
        return View();
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult ResetPassword(
        string? userId,
        string? token)
    {
        if (string.IsNullOrWhiteSpace(userId) ||
            string.IsNullOrWhiteSpace(token))
        {
            return BadRequest();
        }

        return View(new ResetPasswordViewModel
        {
            UserId = userId,
            Token = token
        });
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(
        ResetPasswordViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var user = await _userManager.FindByIdAsync(model.UserId);

        if (user is null ||
            !string.Equals(
                user.Email, model.Email,
                StringComparison.OrdinalIgnoreCase))
        {
            ModelState.AddModelError(
                string.Empty, "The reset link is invalid or expired.");
            return View(model);
        }

        try
        {
            var decodedToken = System.Text.Encoding.UTF8.GetString(
                Microsoft.AspNetCore.WebUtilities.WebEncoders
                    .Base64UrlDecode(model.Token));

            var result = await _userManager.ResetPasswordAsync(
                user, decodedToken, model.Password);

            if (result.Succeeded)
                return RedirectToAction(nameof(ResetPasswordConfirmation));

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);
        }
        catch (FormatException)
        {
            ModelState.AddModelError(
                string.Empty, "The reset link is invalid or expired.");
        }

        return View(model);
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult ResetPasswordConfirmation()
    {
        return View();
    }

    private string? ReadGuestCartKey()
    {
        if (Request.Cookies.TryGetValue(CartCookieName, out var key) &&
            Guid.TryParseExact(key, "N", out _))
        {
            return key;
        }

        return null;
    }

    private static string EnsureCustomerCartKey(ApplicationUser user)
    {
        if (Guid.TryParseExact(user.CartKey, "N", out _))
            return user.CartKey!;

        user.CartKey = Guid.NewGuid().ToString("N");
        return user.CartKey;
    }

    private void SetCartCookie(string cartKey)
    {
        Response.Cookies.Append(CartCookieName, cartKey, new CookieOptions
        {
            HttpOnly = true,
            Secure = Request.IsHttps,
            SameSite = SameSiteMode.Lax,
            IsEssential = true,
            Expires = DateTimeOffset.UtcNow.AddDays(30)
        });
    }
}

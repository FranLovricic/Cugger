using System.Security.Claims;
using Cugger.Data;
using Cugger.Models;
using Cugger.Models.ViewModels;
using Cugger.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cugger.Controllers
{
    public class AccountController : Controller
    {
        private readonly CuggerDbContext _db;
        private readonly PasswordService _passwords;

        public AccountController(CuggerDbContext db, PasswordService passwords)
        {
            _db = db;
            _passwords = passwords;
        }

        // ========== REGISTER ==========

        [HttpGet]
        [AllowAnonymous]
        [Route("register")]
        public IActionResult Register()
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Home");

            ViewBag.Breadcrumbs = new[]
            {
                new BreadcrumbItem("Početna", "/", false),
                new BreadcrumbItem("Registracija", "/register", true)
            };
            return View(new RegisterViewModel());
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [Route("register")]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var usernameExists = await _db.Users.AnyAsync(u => u.Username == model.Username);
            if (usernameExists)
                ModelState.AddModelError(nameof(model.Username), "To korisničko ime je već zauzeto.");

            var emailExists = await _db.Users.AnyAsync(u => u.Email == model.Email);
            if (emailExists)
                ModelState.AddModelError(nameof(model.Email), "Email je već registriran.");

            if (!ModelState.IsValid)
                return View(model);

            var (hash, salt) = _passwords.HashPassword(model.Password);

            var user = new User
            {
                Username = model.Username.Trim(),
                Email = model.Email.Trim().ToLowerInvariant(),
                FirstName = model.FirstName.Trim(),
                LastName = model.LastName.Trim(),
                RegistrationDate = DateTime.UtcNow,
                Bio = string.Empty,
                AvatarUrl = $"https://ui-avatars.com/api/?name={Uri.EscapeDataString(model.FirstName + "+" + model.LastName)}&background=F59E0B&color=111",
                PasswordHash = hash,
                PasswordSalt = salt,
                IsEmailConfirmed = false,
                EmailConfirmationToken = _passwords.GenerateResetToken(),
                EmailConfirmationTokenExpiresAt = DateTime.UtcNow.AddDays(7)
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            // Demo mode: prikazujemo confirmation link na confirmation pending stranici
            // (u produkciji bi se ovaj link slao emailom).
            var confirmationLink = Url.Action(
                nameof(ConfirmEmail),
                "Account",
                new { token = user.EmailConfirmationToken, email = user.Email },
                Request.Scheme);

            TempData["ConfirmationLink"] = confirmationLink;
            TempData["ConfirmationEmail"] = user.Email;

            return RedirectToAction(nameof(RegisterPending));
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("register/pending")]
        public IActionResult RegisterPending()
        {
            ViewBag.ConfirmationLink = TempData["ConfirmationLink"] as string;
            ViewBag.ConfirmationEmail = TempData["ConfirmationEmail"] as string;

            ViewBag.Breadcrumbs = new[]
            {
                new BreadcrumbItem("Početna", "/", false),
                new BreadcrumbItem("Registracija", "/register", false),
                new BreadcrumbItem("Potvrda", "/register/pending", true)
            };
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("confirm-email")]
        public async Task<IActionResult> ConfirmEmail(string? token, string? email)
        {
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email))
            {
                TempData["Error"] = "Neispravan ili istekao link za potvrdu emaila.";
                return RedirectToAction(nameof(Login));
            }

            var normalized = email.Trim().ToLowerInvariant();
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == normalized);

            if (user == null
                || string.IsNullOrEmpty(user.EmailConfirmationToken)
                || user.EmailConfirmationToken != token
                || user.EmailConfirmationTokenExpiresAt == null
                || user.EmailConfirmationTokenExpiresAt < DateTime.UtcNow)
            {
                TempData["Error"] = "Link za potvrdu je istekao ili nije važeći.";
                return RedirectToAction(nameof(Login));
            }

            user.IsEmailConfirmed = true;
            user.EmailConfirmationToken = null;
            user.EmailConfirmationTokenExpiresAt = null;
            await _db.SaveChangesAsync();

            await SignInAsync(user, rememberMe: false);

            TempData["Success"] = $"Email potvrđen. Dobrodošao, {user.FirstName}! 🍻";
            return RedirectToAction("Index", "Home");
        }

        // ========== LOGIN ==========

        [HttpGet]
        [AllowAnonymous]
        [Route("login")]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Home");

            ViewBag.Breadcrumbs = new[]
            {
                new BreadcrumbItem("Početna", "/", false),
                new BreadcrumbItem("Prijava", "/login", true)
            };
            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [Route("login")]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var key = model.UsernameOrEmail.Trim().ToLowerInvariant();
            var user = await _db.Users
                .FirstOrDefaultAsync(u => u.Username.ToLower() == key || u.Email.ToLower() == key);

            if (user == null || !_passwords.VerifyPassword(model.Password, user.PasswordHash, user.PasswordSalt))
            {
                ModelState.AddModelError(string.Empty, "Neispravno korisničko ime/email ili lozinka.");
                return View(model);
            }

            if (!user.IsEmailConfirmed)
            {
                ModelState.AddModelError(string.Empty,
                    "Email nije potvrđen. Provjeri svoju e-poštu (ili klikni na 'Pošalji ponovno' niže).");
                ViewBag.UnconfirmedEmail = user.Email;
                return View(model);
            }

            await SignInAsync(user, model.RememberMe);

            if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                return Redirect(model.ReturnUrl);

            TempData["Success"] = $"Bok, {user.FirstName}! 🍺";
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [Route("resend-confirmation")]
        public async Task<IActionResult> ResendConfirmation(string email)
        {
            if (string.IsNullOrEmpty(email))
                return RedirectToAction(nameof(Login));

            var normalized = email.Trim().ToLowerInvariant();
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == normalized);

            if (user != null && !user.IsEmailConfirmed)
            {
                user.EmailConfirmationToken = _passwords.GenerateResetToken();
                user.EmailConfirmationTokenExpiresAt = DateTime.UtcNow.AddDays(7);
                await _db.SaveChangesAsync();

                var link = Url.Action(
                    nameof(ConfirmEmail),
                    "Account",
                    new { token = user.EmailConfirmationToken, email = user.Email },
                    Request.Scheme);

                TempData["ConfirmationLink"] = link;
                TempData["ConfirmationEmail"] = user.Email;
            }

            return RedirectToAction(nameof(RegisterPending));
        }

        // ========== LOGOUT ==========

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        [Route("logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            TempData["Success"] = "Vidimo se uskoro. 👋";
            return RedirectToAction("Index", "Home");
        }

        // ========== FORGOT PASSWORD ==========

        [HttpGet]
        [AllowAnonymous]
        [Route("forgot-password")]
        public IActionResult ForgotPassword()
        {
            ViewBag.Breadcrumbs = new[]
            {
                new BreadcrumbItem("Početna", "/", false),
                new BreadcrumbItem("Zaboravljena lozinka", "/forgot-password", true)
            };
            return View(new ForgotPasswordViewModel());
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [Route("forgot-password")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var email = model.Email.Trim().ToLowerInvariant();
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == email);

            if (user != null)
            {
                user.PasswordResetToken = _passwords.GenerateResetToken();
                user.PasswordResetTokenExpiresAt = DateTime.UtcNow.AddHours(2);
                await _db.SaveChangesAsync();

                var resetLink = Url.Action(
                    nameof(ResetPassword),
                    "Account",
                    new { token = user.PasswordResetToken, email = user.Email },
                    Request.Scheme);

                TempData["ResetLink"] = resetLink;
            }

            TempData["Success"] = "Ako račun s tim emailom postoji, link za reset je generiran.";
            return RedirectToAction(nameof(ForgotPasswordConfirmation));
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("forgot-password/confirmation")]
        public IActionResult ForgotPasswordConfirmation()
        {
            ViewBag.ResetLink = TempData["ResetLink"] as string;
            ViewBag.Breadcrumbs = new[]
            {
                new BreadcrumbItem("Početna", "/", false),
                new BreadcrumbItem("Zaboravljena lozinka", "/forgot-password", false),
                new BreadcrumbItem("Potvrda", "/forgot-password/confirmation", true)
            };
            return View();
        }

        // ========== RESET PASSWORD ==========

        [HttpGet]
        [AllowAnonymous]
        [Route("reset-password")]
        public IActionResult ResetPassword(string? token, string? email)
        {
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email))
            {
                TempData["Error"] = "Neispravan ili istekao link za reset.";
                return RedirectToAction(nameof(ForgotPassword));
            }

            ViewBag.Breadcrumbs = new[]
            {
                new BreadcrumbItem("Početna", "/", false),
                new BreadcrumbItem("Reset lozinke", "/reset-password", true)
            };

            return View(new ResetPasswordViewModel { Token = token, Email = email });
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [Route("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var email = model.Email.Trim().ToLowerInvariant();
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == email);

            if (user == null
                || string.IsNullOrEmpty(user.PasswordResetToken)
                || user.PasswordResetToken != model.Token
                || user.PasswordResetTokenExpiresAt == null
                || user.PasswordResetTokenExpiresAt < DateTime.UtcNow)
            {
                ModelState.AddModelError(string.Empty, "Token je istekao ili nije važeći. Zatraži novi reset link.");
                return View(model);
            }

            var (hash, salt) = _passwords.HashPassword(model.Password);
            user.PasswordHash = hash;
            user.PasswordSalt = salt;
            user.PasswordResetToken = null;
            user.PasswordResetTokenExpiresAt = null;
            await _db.SaveChangesAsync();

            TempData["Success"] = "Lozinka je promijenjena. Možeš se prijaviti.";
            return RedirectToAction(nameof(Login));
        }

        // ========== ACCESS DENIED ==========

        [HttpGet]
        [AllowAnonymous]
        [Route("access-denied")]
        public IActionResult AccessDenied() => View();

        // ========== Helpers ==========

        private async Task SignInAsync(User user, bool rememberMe)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name, user.Username),
                new(ClaimTypes.Email, user.Email),
                new(ClaimTypes.GivenName, user.FirstName),
                new(ClaimTypes.Surname, user.LastName),
                new("AvatarUrl", user.AvatarUrl ?? string.Empty)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            var props = new AuthenticationProperties
            {
                IsPersistent = rememberMe,
                ExpiresUtc = rememberMe ? DateTimeOffset.UtcNow.AddDays(30) : null
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                props);
        }
    }
}

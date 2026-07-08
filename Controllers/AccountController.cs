using System.Security.Claims;
using Cugger.Models;
using Cugger.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Cugger.Controllers
{
    /// <summary>
    /// Lab-5: autentikacija prebačena na ASP.NET Core Identity
    /// (UserManager / SignInManager) + Google external login.
    /// Rute i view-ovi ostali su isti kao u prethodnim labovima.
    /// </summary>
    public class AccountController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
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

            if (await _userManager.FindByNameAsync(model.Username.Trim()) != null)
                ModelState.AddModelError(nameof(model.Username), "To korisničko ime je već zauzeto.");

            if (await _userManager.FindByEmailAsync(model.Email.Trim()) != null)
                ModelState.AddModelError(nameof(model.Email), "Email je već registriran.");

            if (!ModelState.IsValid)
                return View(model);

            var user = new AppUser
            {
                UserName = model.Username.Trim(),
                Email = model.Email.Trim().ToLowerInvariant(),
                FirstName = model.FirstName.Trim(),
                LastName = model.LastName.Trim(),
                RegistrationDate = DateTime.UtcNow,
                Bio = string.Empty,
                AvatarUrl = $"https://ui-avatars.com/api/?name={Uri.EscapeDataString(model.FirstName + "+" + model.LastName)}&background=F59E0B&color=111"
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
                return View(model);
            }

            await _userManager.AddToRoleAsync(user, "Member");

            // Demo mode: prikazujemo confirmation link na confirmation pending stranici
            // (u produkciji bi se ovaj link slao emailom).
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var confirmationLink = Url.Action(
                nameof(ConfirmEmail),
                "Account",
                new { token, email = user.Email },
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

            var user = await _userManager.FindByEmailAsync(email.Trim());
            if (user == null)
            {
                TempData["Error"] = "Link za potvrdu je istekao ili nije važeći.";
                return RedirectToAction(nameof(Login));
            }

            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (!result.Succeeded)
            {
                TempData["Error"] = "Link za potvrdu je istekao ili nije važeći.";
                return RedirectToAction(nameof(Login));
            }

            await _signInManager.SignInAsync(user, isPersistent: false);

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

            var key = model.UsernameOrEmail.Trim();
            var user = await _userManager.FindByNameAsync(key)
                       ?? await _userManager.FindByEmailAsync(key);

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Neispravno korisničko ime/email ili lozinka.");
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(
                user, model.Password, model.RememberMe, lockoutOnFailure: true);

            if (result.IsNotAllowed)
            {
                ModelState.AddModelError(string.Empty,
                    "Email nije potvrđen. Provjeri svoju e-poštu (ili klikni na 'Pošalji ponovno' niže).");
                ViewBag.UnconfirmedEmail = user.Email;
                return View(model);
            }

            if (result.IsLockedOut)
            {
                _logger.LogWarning("Login: račun {Username} zaključan zbog previše neuspjelih pokušaja", user.UserName);
                ModelState.AddModelError(string.Empty, "Račun je privremeno zaključan zbog previše neuspjelih pokušaja.");
                return View(model);
            }

            if (!result.Succeeded)
            {
                _logger.LogWarning("Login: neuspješan pokušaj prijave za {Username}", user.UserName);
                ModelState.AddModelError(string.Empty, "Neispravno korisničko ime/email ili lozinka.");
                return View(model);
            }

            _logger.LogInformation("Login: korisnik {Username} se prijavio", user.UserName);

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

            var user = await _userManager.FindByEmailAsync(email.Trim());
            if (user != null && !user.EmailConfirmed)
            {
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var link = Url.Action(
                    nameof(ConfirmEmail),
                    "Account",
                    new { token, email = user.Email },
                    Request.Scheme);

                TempData["ConfirmationLink"] = link;
                TempData["ConfirmationEmail"] = user.Email;
            }

            return RedirectToAction(nameof(RegisterPending));
        }

        // ========== EXTERNAL LOGIN (Google) — lab-5 ==========

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [Route("external-login")]
        public IActionResult ExternalLogin(string provider, string? returnUrl = null)
        {
            var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Account", new { returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return Challenge(properties, provider);
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("external-login/callback")]
        public async Task<IActionResult> ExternalLoginCallback(string? returnUrl = null, string? remoteError = null)
        {
            if (remoteError != null)
            {
                TempData["Error"] = $"Vanjski pružatelj prijave je vratio grešku: {remoteError}";
                return RedirectToAction(nameof(Login));
            }

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                TempData["Error"] = "Greška pri dohvaćanju podataka o vanjskoj prijavi.";
                return RedirectToAction(nameof(Login));
            }

            // 1) Korisnik već ima povezan vanjski login → prijavi ga
            var signInResult = await _signInManager.ExternalLoginSignInAsync(
                info.LoginProvider, info.ProviderKey, isPersistent: true, bypassTwoFactor: true);

            if (signInResult.Succeeded)
            {
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);
                return RedirectToAction("Index", "Home");
            }

            // 2) Nema povezanog logina → pronađi po emailu ili kreiraj novi račun
            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(email))
            {
                TempData["Error"] = $"{info.ProviderDisplayName} nije vratio email adresu.";
                return RedirectToAction(nameof(Login));
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                var firstName = info.Principal.FindFirstValue(ClaimTypes.GivenName) ?? "Cugger";
                var lastName = info.Principal.FindFirstValue(ClaimTypes.Surname) ?? "Korisnik";

                user = new AppUser
                {
                    UserName = await GenerateUniqueUsernameAsync(email),
                    Email = email.ToLowerInvariant(),
                    EmailConfirmed = true, // email dolazi verificiran od Googlea
                    FirstName = firstName,
                    LastName = lastName,
                    RegistrationDate = DateTime.UtcNow,
                    Bio = string.Empty,
                    AvatarUrl = $"https://ui-avatars.com/api/?name={Uri.EscapeDataString(firstName + "+" + lastName)}&background=F59E0B&color=111"
                };

                var createResult = await _userManager.CreateAsync(user);
                if (!createResult.Succeeded)
                {
                    TempData["Error"] = "Neuspjelo kreiranje računa iz vanjske prijave.";
                    return RedirectToAction(nameof(Login));
                }

                await _userManager.AddToRoleAsync(user, "Member");
            }

            await _userManager.AddLoginAsync(user, info);
            await _signInManager.SignInAsync(user, isPersistent: true);

            TempData["Success"] = $"Bok, {user.FirstName}! 🍺 (prijava putem {info.ProviderDisplayName})";
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            return RedirectToAction("Index", "Home");
        }

        // ========== LOGOUT ==========

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        [Route("logout")]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
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

            var user = await _userManager.FindByEmailAsync(model.Email.Trim());
            if (user != null)
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var resetLink = Url.Action(
                    nameof(ResetPassword),
                    "Account",
                    new { token, email = user.Email },
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

            var user = await _userManager.FindByEmailAsync(model.Email.Trim());
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Token je istekao ili nije važeći. Zatraži novi reset link.");
                return View(model);
            }

            var result = await _userManager.ResetPasswordAsync(user, model.Token, model.Password);
            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, "Token je istekao ili nije važeći. Zatraži novi reset link.");
                return View(model);
            }

            TempData["Success"] = "Lozinka je promijenjena. Možeš se prijaviti.";
            return RedirectToAction(nameof(Login));
        }

        // ========== ACCESS DENIED ==========

        [HttpGet]
        [AllowAnonymous]
        [Route("access-denied")]
        public IActionResult AccessDenied() => View();

        // ========== Helpers ==========

        private async Task<string> GenerateUniqueUsernameAsync(string email)
        {
            var baseName = new string(email.Split('@')[0]
                .Where(c => char.IsLetterOrDigit(c) || c is '_' or '.' or '-')
                .ToArray());
            if (string.IsNullOrEmpty(baseName)) baseName = "cugger_user";

            var candidate = baseName;
            var i = 1;
            while (await _userManager.FindByNameAsync(candidate) != null)
            {
                candidate = $"{baseName}{i++}";
            }
            return candidate;
        }
    }
}

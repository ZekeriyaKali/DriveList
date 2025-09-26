using DriveList.Application.DTOs;
using DriveList.Domain.Entities;
using DriveList.Infrastructure.Persistence;
using DriveListApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using System.Net.Http;
using System.Security.Claims;

namespace DriveListApi.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly AppDbContext _context;

        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            AppDbContext context,
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory
            )
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
        }

        // GET: Register
        [HttpGet]
        public IActionResult Register() => View();

        // POST: Register
        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            var recaptchaResponse = Request.Form["g-recaptcha-response"].ToString();
            if (!await ValidateRecaptchaAsync(recaptchaResponse))
            {
                ModelState.AddModelError("", "reCAPTCHA doğrulaması başarısız oldu.");
                return View(model);
            }

            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = model.Username, Email = model.Email };
                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    var confirmLink = Url.Action("ConfirmEmail", "Account",
                        new { userId = user.Id, token }, Request.Scheme);

                    // Burada gerçek e-posta servisine gönder (SMTP/SendGrid). Geliştirici modu için:
                    Console.WriteLine($"[DEV] Email confirm link: {confirmLink}");

                    // Linki ekranda göstermek istersen:
                    TempData["ConfirmEmailLink"] = confirmLink;

                    return RedirectToAction("RegisterConfirmation");
                }

                foreach (var error in result.Errors)
                    ModelState.AddModelError("", error.Description);
            }
            ModelState.Clear();
            return View(model);
        }

        private async Task<bool> ValidateRecaptchaAsync(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return false;

            // Recaptcha secret'ı konfigürasyondan al
            var secret = _configuration["Recaptcha:Secret"];
            if (string.IsNullOrWhiteSpace(secret))
            {
                // Geliştirme ortamında isen konsola yazdırabilirsin; production'da secret olmadan doğrulama başarısız olmalı
                // _logger?.LogWarning("Recaptcha secret not configured.");
                return false;
            }

            var client = _httpClientFactory.CreateClient();

            // Google reCAPTCHA verify endpoint expects form-urlencoded (POST)
            var form = new Dictionary<string, string>
            {
                ["secret"] = secret,
                ["response"] = token
            };

            using var content = new FormUrlEncodedContent(form);
            using var response = await client.PostAsync("https://www.google.com/recaptcha/api/siteverify", content);

            if (!response.IsSuccessStatusCode)
                return false;

            var payload = await response.Content.ReadFromJsonAsync<RecaptchaVerifyResponse>();
            if (payload == null)
                return false;

            // Başarılıysa success==true; v3 kullanıyorsan score kontrolü de yap
            if (!payload.Success) return false;

            // Eğer v3 kullanıyorsan score eşiği uygulayabilirsin; v2 için Score null olacaktır
            if (payload.Score.HasValue && payload.Score < 0.5m)
                return false;

            return true;
        }

        [HttpGet]
        public IActionResult RegisterConfirmation()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
                return RedirectToAction("Error", "Home");

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return RedirectToAction("Error", "Home");

            var result = await _userManager.ConfirmEmailAsync(user, token);
            return View(result.Succeeded); // View'a bool model gönderiyoruz
        }

        // GET: Login
        [HttpGet]
        public IActionResult Login() => View();

        // POST: Login
        [EnableRateLimiting("loginPolicy")]
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // get client info
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            var userAgent = Request.Headers["User-Agent"].ToString();

            var user = await _userManager.FindByNameAsync(model.Username);
            var usernameForLog = model.Username;

            var result = await _signInManager.PasswordSignInAsync(
                model.Username, model.Password, model.RememberMe, lockoutOnFailure: true);

            // create audit record
            var audit = new LoginAudit
            {
                UserId = user?.Id,
                Username = usernameForLog,
                Success = result.Succeeded,
                IpAddress = ip,
                UserAgent = userAgent,
                FailureReason = result.Succeeded ? null :
                    result.IsLockedOut ? "LockedOut" :
                    result.IsNotAllowed ? "NotAllowed" :
                    "InvalidCredentials"
            };

            _context.LoginAudits.Add(audit);
            await _context.SaveChangesAsync();

            if (result.Succeeded)
            {
                // update last login
                if (user != null)
                {
                    user.LastLoginTime = DateTime.UtcNow;
                    await _userManager.UpdateAsync(user);
                }
                return RedirectToAction("Index", "Home");
            }

            if (result.IsLockedOut)
                return View("Lockout");

            if (result.IsNotAllowed)
            {
                ModelState.AddModelError("", "Please confirm your email before signing in.");
                return View(model);
            }

            ModelState.AddModelError("", "Invalid login attempt.");
            ModelState.Clear();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult ForgotPassword() => View();

        // POST: ForgotPassword
        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    // Güvenlik için: email kayıtlı olmasa da aynı ekran
                    return RedirectToAction("ForgotPasswordConfirmation");
                }

                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var resetLink = Url.Action("ResetPassword", "Account",
                    new { token, email = user.Email }, Request.Scheme);

                // 📧 burada kendi mail servisinden gönder (SMTP, SendGrid vs.)
                Console.WriteLine($"Reset link: {resetLink}");

                return RedirectToAction("ForgotPasswordConfirmation");
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        [HttpGet]
        public IActionResult ResetPassword(string token, string email)
        {
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email))
            {
                return RedirectToAction("Error", "Home");
            }

            var model = new ResetPasswordViewModel
            {
                Token = token,
                Email = email
            };

            return View(model);
        }

        // POST: ResetPassword
        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null) return RedirectToAction("ResetPasswordConfirmation");

            var result = await _userManager.ResetPasswordAsync(user, model.Token, model.Password);
            if (result.Succeeded)
                return RedirectToAction("ResetPasswordConfirmation");

            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);

            return View(model);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> EnableAuthenticator()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login");

            // ensure key exists
            var unformattedKey = await _userManager.GetAuthenticatorKeyAsync(user);
            if (string.IsNullOrEmpty(unformattedKey))
            {
                await _userManager.ResetAuthenticatorKeyAsync(user);
                unformattedKey = await _userManager.GetAuthenticatorKeyAsync(user);
            }

            var authenticatorUri = GenerateQrCodeUri(user.Email ?? user.UserName, unformattedKey);
            // generate QR code image bytes with QRCoder
            using var qrGenerator = new QRCoder.QRCodeGenerator();
            using var qrData = qrGenerator.CreateQrCode(authenticatorUri, QRCoder.QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new QRCoder.PngByteQRCode(qrData);
            var qrBytes = qrCode.GetGraphic(20);

            var vm = new EnableAuthenticatorViewModel
            {
                SharedKey = FormatKey(unformattedKey),
                AuthenticatorUri = authenticatorUri,
                QrCodeImageBase64 = "data:image/png;base64," + Convert.ToBase64String(qrBytes)
            };

            return View(vm);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> EnableAuthenticator(EnableAuthenticatorViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login");

            var verificationCode = model.Code.Replace(" ", string.Empty).Replace("-", string.Empty);

            var isValid = await _userManager.VerifyTwoFactorTokenAsync(
                 user, _userManager.Options.Tokens.AuthenticatorTokenProvider, verificationCode);

            if (!isValid)
            {
                ModelState.AddModelError("Code", "Verification code is invalid.");
                return View(model);
            }

            await _userManager.SetTwoFactorEnabledAsync(user, true);
            return RedirectToAction("Manage");
        }

        private string FormatKey(string unformattedKey)
        {
            return string.Join(" ", unformattedKey.ToUpper().Chunk(4).Select(c => new string(c)));
        }

        private string GenerateQrCodeUri(string email, string unformattedKey)
        {
            var issuer = Uri.EscapeDataString("DriveListApp"); // kendi uygulama adın
            return $"otpauth://totp/{issuer}:{email}?secret={unformattedKey}&issuer={issuer}&digits=6";
        }

        [HttpPost]
        public IActionResult ExternalLogin(string provider, string returnUrl = null)
        {
            var redirectUrl = Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return Challenge(properties, provider);
        }

        public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null, string remoteError = null)
        {
            if (remoteError != null)
            {
                ModelState.AddModelError("", $"External provider error: {remoteError}");
                return RedirectToAction(nameof(Login));
            }

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null) return RedirectToAction(nameof(Login));

            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, false);

            if (result.Succeeded)
                return RedirectToLocal(returnUrl);

            // Kullanıcı yoksa yeni kaydet
            var email = info.Principal.FindFirstValue(System.Security.Claims.ClaimTypes.Email);
            var user = new ApplicationUser { UserName = email, Email = email };
            var identityResult = await _userManager.CreateAsync(user);
            if (identityResult.Succeeded)
            {
                await _userManager.AddLoginAsync(user, info);
                await _signInManager.SignInAsync(user, false);
                return RedirectToLocal(returnUrl);
            }

            return RedirectToAction(nameof(Login));
        }

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        [Authorize]
        public async Task<IActionResult> Manage()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction("Login");
            }

            // Veritabanı context'i (DI ile inject edebilirsin, ctor’a eklemelisin)
            var totalPredictions = await _context.Predictions
                .CountAsync(x => x.UserId == user.Id);

            var recentLogins = await _context.LoginAudits
       .Where(l => l.UserId == user.Id)
       .OrderByDescending(l => l.Timestamp)
       .Take(10)
       .ToListAsync();


            var totalDiagnoses = 0;

            var model = new ProfileViewModel
            {
                UserName = user.UserName,
                Email = user.Email,
                TotalPredictions = totalPredictions,
                TotalDiagnoses = totalDiagnoses,
                LastLoginTime = user.LastLoginTime,
                Credits = user.Credits

            };

            return View(model);
        }

    }
}

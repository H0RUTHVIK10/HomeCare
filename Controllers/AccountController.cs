using System.Security.Claims;
using HomeCare.Data;
using HomeCare.Models;
using HomeCare.Services;
using HomeCare.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HomeCare.Controllers
{
    public class AccountController : Controller
    {
        private readonly HomeCareDbContext _context;
        private readonly IPasswordService _passwordService;
        private readonly IUserContext _userContext;

        public AccountController(
            HomeCareDbContext context,
            IPasswordService passwordService,
            IUserContext userContext)
        {
            _context = context;
            _passwordService = passwordService;
            _userContext = userContext;
        }

        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Dashboard");
            }
            return View(new RegisterViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var normalizedEmail = model.Email.Trim().ToLowerInvariant();
            if (await _context.Users.AnyAsync(u => u.Email.ToLower() == normalizedEmail))
            {
                ModelState.AddModelError(nameof(model.Email), "An account with this email address already exists.");
                return View(model);
            }

            var user = new User
            {
                Name = model.Name.Trim(),
                Email = normalizedEmail,
                PasswordHash = _passwordService.HashPassword(model.Password),
                CreatedAt = DateTime.UtcNow,
                Role = "User"
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Create default Home for new user
            var defaultHomeName = string.IsNullOrWhiteSpace(model.PrimaryHomeName) ? "My Residence" : model.PrimaryHomeName.Trim();
            var home = new Home
            {
                Name = defaultHomeName,
                UserId = user.Id,
                CreatedAt = DateTime.UtcNow,
                HomeType = "Primary Residence"
            };
            _context.Homes.Add(home);
            await _context.SaveChangesAsync();

            // Set active home in user context
            _userContext.ActiveHomeId = home.Id;

            // Automatically sign in the user
            await SignInUserAsync(user, isPersistent: true);

            TempData["SuccessMessage"] = $"Welcome to HomeCare, {user.Name}! Your account and primary home have been created.";
            return RedirectToAction("Index", "Dashboard");
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Dashboard");
            }

            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var normalizedEmail = model.Email.Trim().ToLowerInvariant();
            var user = await _context.Users
                .Include(u => u.Homes)
                .FirstOrDefaultAsync(u => u.Email.ToLower() == normalizedEmail);

            if (user == null || !_passwordService.VerifyPassword(user.PasswordHash, model.Password))
            {
                ModelState.AddModelError(string.Empty, "Invalid email address or password.");
                return View(model);
            }

            // Set active home if none selected
            if (_userContext.ActiveHomeId == null && user.Homes.Any())
            {
                _userContext.ActiveHomeId = user.Homes.First().Id;
            }

            await SignInUserAsync(user, model.RememberMe);

            TempData["SuccessMessage"] = $"Welcome back, {user.Name}!";

            if (!string.IsNullOrWhiteSpace(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            {
                return Redirect(model.ReturnUrl);
            }

            return RedirectToAction("Index", "Dashboard");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear();
            _userContext.ActiveHomeId = null;

            TempData["SuccessMessage"] = "You have been logged out successfully.";
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var userId = _userContext.CurrentUserId;
            if (!userId.HasValue) return RedirectToAction("Login");

            var user = await _context.Users
                .Include(u => u.Homes)
                .ThenInclude(h => h.Appliances)
                .FirstOrDefaultAsync(u => u.Id == userId.Value);

            if (user == null) return NotFound();

            var viewModel = new ProfileViewModel
            {
                Name = user.Name,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                MemberSince = user.CreatedAt,
                TotalHomes = user.Homes.Count,
                TotalAppliances = user.Homes.SelectMany(h => h.Appliances).Count()
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Profile(ProfileViewModel model)
        {
            var userId = _userContext.CurrentUserId;
            if (!userId.HasValue) return RedirectToAction("Login");

            var user = await _context.Users.FindAsync(userId.Value);
            if (user == null) return NotFound();

            if (!ModelState.IsValid)
            {
                model.Email = user.Email;
                model.MemberSince = user.CreatedAt;
                return View(model);
            }

            user.Name = model.Name.Trim();
            user.PhoneNumber = model.PhoneNumber?.Trim();

            await _context.SaveChangesAsync();

            // Refresh authentication claim for updated name
            await SignInUserAsync(user, isPersistent: true);

            TempData["SuccessMessage"] = "Profile updated successfully.";
            return RedirectToAction(nameof(Profile));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            var userId = _userContext.CurrentUserId;
            if (!userId.HasValue) return RedirectToAction("Login");

            var user = await _context.Users.FindAsync(userId.Value);
            if (user == null) return NotFound();

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Password validation failed. Please check inputs.";
                return RedirectToAction(nameof(Profile));
            }

            if (!_passwordService.VerifyPassword(user.PasswordHash, model.CurrentPassword))
            {
                TempData["ErrorMessage"] = "Current password is incorrect.";
                return RedirectToAction(nameof(Profile));
            }

            user.PasswordHash = _passwordService.HashPassword(model.NewPassword);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Your password has been changed successfully.";
            return RedirectToAction(nameof(Profile));
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        private async Task SignInUserAsync(User user, bool isPersistent)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name, user.Name),
                new(ClaimTypes.Email, user.Email),
                new(ClaimTypes.Role, string.IsNullOrWhiteSpace(user.Role) ? "User" : user.Role)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = isPersistent,
                ExpiresUtc = isPersistent ? DateTimeOffset.UtcNow.AddDays(14) : null
            };

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, authProperties);
        }
    }
}

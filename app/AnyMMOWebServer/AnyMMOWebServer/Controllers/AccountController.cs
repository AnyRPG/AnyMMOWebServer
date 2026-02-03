using AnyMMOWebServer.Database;
using AnyMMOWebServer.Models;
using AnyMMOWebServer.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AnyMMOWebServer.Controllers
{
    
    public class AccountController : Controller
    {
        private readonly GameDbContext dbContext;
        private readonly UserAccountService userAccountService;
        private readonly PlayerCharacterService playerCharacterService;
        private readonly ILogger<AccountController> logger;
        private readonly AnyMMOWebServer.Services.IAuthenticationService authenticationService;
        private readonly AnyMMOWebServerSettings anyMMOWebServerSettings;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AccountController(
            ILogger<AccountController> logger,
            GameDbContext dbContext,
            AnyMMOWebServerSettings anyMMOWebServerSettings,
            AnyMMOWebServer.Services.IAuthenticationService authenticationService,
            IHttpContextAccessor httpContextAccessor)
        {
            this.dbContext = dbContext;
            this.anyMMOWebServerSettings = anyMMOWebServerSettings;
            this._httpContextAccessor = httpContextAccessor;
            userAccountService = new UserAccountService(dbContext, logger, httpContextAccessor);
            playerCharacterService = new PlayerCharacterService(dbContext, logger, _httpContextAccessor);
            this.authenticationService = authenticationService;
            //authenticationService = new AnyMMOWebServer.Services.AuthenticationService(anyMMOWebServerSettings, dbContext, logger);
            this.logger = logger;
        }

        // GET: Account
        public ActionResult Index()
        {
            return RedirectToAction("Index", "Home");
        }

        // GET: Account/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: Account/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Account/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                //logger.LogInformation("Adding new user");

                var (success, content) = authenticationService.AddUserFromForm(collection);
                if (!success)
                {
                    return BadRequest(content);
                }
                return RedirectToAction(nameof(RegisterSuccess));
            }
            catch
            {
                return View();
            }
        }

        // GET: Account/RegisterSuccess
        public ActionResult RegisterSuccess() {
            return View();
        }

        // GET: Account/Login
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(IFormCollection collection)
        {
            try
            {
                //logger.LogInformation("Logging in user");
                AuthenticationRequest authenticationRequest = new AuthenticationRequest(collection);
                var (success, content) = authenticationService.Login(authenticationRequest);
                if (!success)
                {
                    TempData["ErrorMessage"] = content;
                    return View();
                }
                return Redirect(HttpContext.Request.Query["page"].ToString() == string.Empty ? "/Account/Dashboard" : collection["ReturnUrl"].ToString());
            } catch
            {
                return View();
            }
        }

        // GET: Account/logout
        public ActionResult Logout()
        {
            authenticationService.Logout();
            return RedirectToAction("Index", "Home");
        }

        // GET: Account/Dashboard
        [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
        public ActionResult Dashboard()
        {
            ViewData["UserName"] = HttpContext.User.Claims.First(c => c.Type == ClaimTypes.Name).Value;
            ViewData["UserId"] = HttpContext.User.Claims.First(c => c.Type == "Id").Value;
            int userId = int.Parse(HttpContext.User.Claims.First(c => c.Type == "Id").Value);
            PlayerCharacterListResponse playerCharacterListResponse = playerCharacterService.GetPlayerCharacters(userId);
            ViewBag.PlayerCharacters = playerCharacterListResponse.PlayerCharacters;
            return View();
        }

        // GET: AccountController/Edit
        [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
        public ActionResult Edit() {
            var idClaim = HttpContext.User.Claims.First(c => c.Type == "Id").Value;

            if (!int.TryParse(idClaim, out int userId)) {
                return RedirectToAction("Login", "Account");
            }

            // 2. Fetch the user from the database
            User user = userAccountService.GetUser(userId);

            if (user == null) {
                return NotFound();
            }

            // 3. Return the Edit view with the user model pre-filled
            return View(user);
        }

        // POST: AccountController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
        public ActionResult Edit(User model)
        {
            if (!ModelState.IsValid) {
                // If invalid, return the same view so the user can see error messages
                return View(model);
            }

            try {
                if (userAccountService.SaveUserDetails(model) != true) {
                    return NotFound();
                }

                TempData["SuccessMessage"] = "Profile updated successfully!";
                return RedirectToAction(nameof(Dashboard));
            } catch (DbUpdateException ex) {
                // Log the error (as discussed in your earlier logging question!)
                logger.LogError(ex, "Error updating details for User {UserId}", model.Id);

                ModelState.AddModelError("", "Unable to save changes. Try again later.");
                return View(model);
            }
        }

        // GET: AccountController/Delete/5
        [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
        public ActionResult Delete()
        {
            return View();
        }

        // POST: AccountController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction("Index", "Home");
            }
            catch
            {
                return View();
            }
        }

        // GET: AccountController/ChangePassword
        [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
        public ActionResult ChangePassword() {
            // Just initialize a clean ViewModel
            var viewModel = new ChangePasswordViewModel();

            // Return the specific ViewModel type to the View
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
        public IActionResult ChangePassword(ChangePasswordViewModel model) {
            if (!ModelState.IsValid) {
                return View(model);
            }

            // Extract User ID from claims
            var idClaim = User.FindFirst("Id")?.Value;
            if (!int.TryParse(idClaim, out int userId)) {
                return RedirectToAction("Login");
            }

            // Call your service
            var result = authenticationService.ChangePassword(userId, model.OldPassword, model.NewPassword);

            if (result.success) {
                TempData["SuccessMessage"] = "Password changed successfully!";
                return RedirectToAction("Dashboard");
            }

            // If service failed (e.g. wrong old password), add error to the specific field
            ModelState.AddModelError("OldPassword", result.errorMessage);
            return View(model);
        }

    }
}

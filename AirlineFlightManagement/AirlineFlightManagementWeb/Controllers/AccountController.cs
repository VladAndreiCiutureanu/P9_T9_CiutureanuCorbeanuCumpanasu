using AirlineFlightManagement.Models.ViewModels;
using AirlineFlightManagement.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Policy;
using System.Threading.Tasks;

namespace AirlineFlightManagementWeb.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAuthService _authService;

        public AccountController(IAuthService authService)
        {
            _authService = authService;
        }

        // GET: /Account/Register
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            return View(new RegisterViewModel());
        }

        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Apelăm serviciul de autentificare care gestionează tranzacția atomică
            var result = await _authService.RegisterAsync(
                model.Email,
                model.Password,
                model.FirstName,
                model.LastName,
                model.PhoneNumber,
                model.Address);

            if (result.Success)
            {
                // REQ-5: După înregistrarea cu succes, redirecționăm utilizatorul către pagina de login
                TempData["SuccessMessage"] = "Contul a fost creat cu succes! Vă rugăm să vă autentificați.";
                return RedirectToAction(nameof(Login));
            }

            // Dacă înregistrarea a eșuat (ex. email duplicat, parolă slabă), afișăm erorile primite
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error);
            }

            return View(model);
        }

        // GET: /Account/Login
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View(new LoginViewModel());
        }

        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _authService.LoginAsync(model.Email, model.Password, model.RememberMe);

            if (result.Success)
            {
                // REQ-3: Redirecționare sigură după autentificare
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }

                return RedirectToAction("Index", "Home");
            }

            // Mesaj generic de eroare pentru a proteja securitatea contului
            ModelState.AddModelError(string.Empty, "Adresă de email sau parolă incorectă, sau contul este dezactivat.");
            return View(model);
        }

        // POST: /Account/Logout
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _authService.LogoutAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}
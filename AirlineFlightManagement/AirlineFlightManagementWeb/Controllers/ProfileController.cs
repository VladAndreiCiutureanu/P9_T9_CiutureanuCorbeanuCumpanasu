using AirlineFlightManagement.Models.ViewModels;
using AirlineFlightManagement.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AirlineFlightManagementWeb.Controllers
{
    [Authorize] // Toate acțiunile necesită autentificare implicită
    public class ProfileController : Controller
    {
        private readonly IPassengerService _passengerService;
        private readonly IAuthService _authService;

        public ProfileController(IPassengerService passengerService, IAuthService authService)
        {
            _passengerService = passengerService;
            _authService = authService;
        }

        // GET: /Profile/Index
        // REQ-15: Vizualizarea propriilor date de profil
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var profile = await _passengerService.GetByUserIdAsync(userId);
            if (profile == null)
            {
                TempData["ErrorMessage"] = "Profilul de pasager nu a fost găsit.";
                return RedirectToAction("Index", "Home");
            }

            return View(profile);
        }

        // GET: /Profile/Edit
        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var profile = await _passengerService.GetByUserIdAsync(userId);
            if (profile == null)
            {
                return RedirectToAction(nameof(Index));
            }

            var model = new ProfileEditViewModel
            {
                FirstName = profile.FirstName,
                LastName = profile.LastName,
                PhoneNumber = profile.PhoneNumber,
                Address = profile.Address
            };

            return View(model);
        }

        // POST: /Profile/Edit
        // REQ-15: Pasagerul își poate actualiza datele de contact
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProfileEditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                await _passengerService.UpdateProfileAsync(
                    userId,
                    model.FirstName,
                    model.LastName,
                    model.PhoneNumber,
                    model.Address);

                TempData["SuccessMessage"] = "Datele profilului au fost actualizate cu succes.";
                return RedirectToAction(nameof(Index));
            }
            catch (System.Collections.Generic.KeyNotFoundException)
            {
                ModelState.AddModelError(string.Empty, "Eroare: Profilul nu a putut fi identificat pentru actualizare.");
                return View(model);
            }
        }

        // GET: /Profile/AdminUserList
        // REQ-17: Afișarea listei de clienți pentru personalul administrativ
        [HttpGet]
        [Authorize(Roles = "Administrator, Staff")]
        public async Task<IActionResult> AdminUserList()
        {
            var passengers = await _passengerService.GetAllAsync();
            return View(passengers);
        }

        // POST: /Profile/DeactivateUser
        // REQ-18: Doar Administratorul poate dezactiva un cont de utilizator
        [HttpPost]
        [Authorize(Roles = "Administrator")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeactivateUser(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                TempData["ErrorMessage"] = "Identificator de utilizator invalid.";
                return RedirectToAction(nameof(AdminUserList));
            }

            var result = await _authService.DeactivateUserAsync(userId);
            if (result.Success)
            {
                TempData["SuccessMessage"] = "Contul utilizatorului a fost dezactivat cu succes.";
            }
            else
            {
                TempData["ErrorMessage"] = "Nu s-a putut dezactiva contul. Verificați permisiunile sau starea utilizatorului.";
            }

            return RedirectToAction(nameof(AdminUserList));
        }

        // GET: /Profile/AssignRole
        // REQ-7: Formular de atribuire a drepturilor
        [HttpGet]
        [Authorize(Roles = "Administrator")]
        public IActionResult AssignRole(string userId, string email)
        {
            var model = new UserRoleAssignViewModel
            {
                UserId = userId,
                Email = email
            };

            return View(model);
        }

        // POST: /Profile/AssignRole
        [HttpPost]
        [Authorize(Roles = "Administrator")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignRole(UserRoleAssignViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _authService.AssignRoleAsync(model.UserId, model.SelectedRole);
            if (result.Success)
            {
                TempData["SuccessMessage"] = $"Rolul '{model.SelectedRole}' a fost atribuit cu succes utilizatorului {model.Email}.";
                return RedirectToAction(nameof(AdminUserList));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error);
            }

            return View(model);
        }
    }
}
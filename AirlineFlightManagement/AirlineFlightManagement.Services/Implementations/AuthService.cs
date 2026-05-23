using AirlineFlightManagement.DataAccess.Repositories.Interfaces;
using AirlineFlightManagement.Models.Models;
using AirlineFlightManagement.Services.Contracts;
using AirlineFlightManagement.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AirlineFlightManagement.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IUnitOfWork _uow;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            IUnitOfWork uow)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _uow = uow;
        }

        public async Task<AuthResult> RegisterAsync(
            string email,
            string password,
            string firstName,
            string lastName,
            string phoneNumber,
            string address)
        {
            // 1. Verificăm dacă adresa de email există deja în sistem
            var existingUser = await _userManager.FindByEmailAsync(email);
            if (existingUser != null)
            {
                return AuthResult.Fail("Există deja un cont înregistrat cu această adresă de email.");
            }

            // 2. Creăm instanța utilizatorului Identity
            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                IsActive = true
            };

            var createResult = await _userManager.CreateAsync(user, password);
            if (!createResult.Succeeded)
            {
                var errors = createResult.Errors.Select(e => e.Description).ToArray();
                return AuthResult.Fail(errors);
            }

            try
            {
                // 3. Verificăm existența rolului 'Customer' și îl atribuim utilizatorului nou creat
                const string defaultRole = "Customer";
                if (!await _roleManager.RoleExistsAsync(defaultRole))
                {
                    await _roleManager.CreateAsync(new IdentityRole(defaultRole));
                }

                await _userManager.AddToRoleAsync(user, defaultRole);

                // 4. Creăm profilul de pasager asociat contului Identity
                var passengerProfile = new PassengerProfile
                {
                    UserId = user.Id,
                    FirstName = firstName,
                    LastName = lastName,
                    PhoneNumber = phoneNumber,
                    Address = address
                };

                await _uow.PassengerProfileRepository.AddAsync(passengerProfile);
                await _uow.SaveAsync();

                return AuthResult.Ok(user.Id);
            }
            catch (Exception)
            {
                // 5. Fallback în caz de eroare la crearea profilului pentru a menține consistența
                await _userManager.DeleteAsync(user);
                return AuthResult.Fail("A apărut o eroare la salvarea profilului de pasager. Vă rugăm să încercați din nou.");
            }
        }

        public async Task<AuthResult> LoginAsync(string email, string password, bool rememberMe)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return AuthResult.Fail("Adresă de email sau parolă incorectă.");
            }

            // Verificăm dacă contul a fost dezactivat de un administrator
            if (!user.IsActive)
            {
                // Defensiv: dacă userul are un cookie vechi de când era activ,
                // îl forțăm să iasă imediat (REQ-18 + REQ 5.3 securitate).
                await _signInManager.SignOutAsync();
                return AuthResult.Fail("Acest cont este dezactivat. Vă rugăm să contactați suportul tehnic.");
            }

            var signInResult = await _signInManager.PasswordSignInAsync(user, password, rememberMe, lockoutOnFailure: false);
            if (signInResult.Succeeded)
            {
                return AuthResult.Ok(user.Id);
            }

            return AuthResult.Fail("Adresă de email sau parolă incorectă.");
        }

        public async Task LogoutAsync()
        {
            await _signInManager.SignOutAsync();
        }

        public async Task<AuthResult> AssignRoleAsync(string userId, string roleName)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return AuthResult.Fail("Utilizatorul specificat nu a fost găsit.");
            }

            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                await _roleManager.CreateAsync(new IdentityRole(roleName));
            }

            var result = await _userManager.AddToRoleAsync(user, roleName);
            if (result.Succeeded)
            {
                return AuthResult.Ok(userId);
            }

            var errors = result.Errors.Select(e => e.Description).ToArray();
            return AuthResult.Fail(errors);
        }

        public async Task<AuthResult> DeactivateUserAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return AuthResult.Fail("Utilizatorul specificat nu a fost găsit.");
            }

            user.IsActive = false;
            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                return AuthResult.Ok(userId);
            }

            var errors = result.Errors.Select(e => e.Description).ToArray();
            return AuthResult.Fail(errors);
        }
    }
}
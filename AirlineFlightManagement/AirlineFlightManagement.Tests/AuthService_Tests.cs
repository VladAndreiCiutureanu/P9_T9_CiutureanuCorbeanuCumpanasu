using AirlineFlightManagement.DataAccess.Repositories.Interfaces;
using AirlineFlightManagement.Models.Models;
using AirlineFlightManagement.Services.Implementations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Moq;
using System.Threading.Tasks;
using Xunit;

namespace AirlineFlightManagement.Tests
{
    public class AuthService_Tests
    {
        // TEST 4 : REQ-5/REQ-6 - Nu putem inregistra un cont daca adresa de email exista deja
        [Fact]
        public async Task RegisterAsync_CandEmailDejaExista_ReturneazaFail()
        {
            // Arrange
            var mockUserManager = MockUserManager();
            var mockSignInManager = MockSignInManager(mockUserManager.Object);
            var mockRoleManager = MockRoleManager();
            var mockUow = new Mock<IUnitOfWork>();

            // Simulam ca adresa de email a fost gasita in baza de date
            mockUserManager.Setup(u => u.FindByEmailAsync("test@email.com")).ReturnsAsync(new ApplicationUser());

            var service = new AuthService(mockUserManager.Object, mockSignInManager.Object, mockRoleManager.Object, mockUow.Object);

            // Act
            var result = await service.RegisterAsync("test@email.com", "Parola123!", "Ion", "Pop", "077", "Adresa");

            // Assert
            Assert.False(result.Success);
        }

        // TEST 5 : REQ-18 - Dezactivarea contului schimba IsActive in false (NU sterge din baza de date)
        [Fact]
        public async Task DeactivateUserAsync_CandUserGasit_SeteazaIsActiveFalseSiActualizeaza()
        {
            // Arrange
            var mockUserManager = MockUserManager();
            var mockSignInManager = MockSignInManager(mockUserManager.Object);
            var mockRoleManager = MockRoleManager();
            var mockUow = new Mock<IUnitOfWork>();

            var user = new ApplicationUser { Id = "user1", IsActive = true };
            mockUserManager.Setup(u => u.FindByIdAsync("user1")).ReturnsAsync(user);

            // UpdateAsync trebuie sa returneze IdentityResult.Success — altfel
            // 'result.Succeeded' din AuthService produce NullReferenceException.
            mockUserManager.Setup(u => u.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);

            var service = new AuthService(mockUserManager.Object, mockSignInManager.Object, mockRoleManager.Object, mockUow.Object);

            // Act
            await service.DeactivateUserAsync("user1");

            // Assert
            Assert.False(user.IsActive); // Verificam regula REQ-18
            mockUserManager.Verify(u => u.UpdateAsync(user), Times.Once); // Verificam ca s-a trimis actualizarea catre DB
        }

        // TEST 6 : Login-ul trebuie sa esueze imediat daca utilizatorul a fost dezactivat de admin
        [Fact]
        public async Task LoginAsync_CandUserEsteInactiv_ReturneazaFail()
        {
            // Arrange
            var mockUserManager = MockUserManager();
            var mockSignInManager = MockSignInManager(mockUserManager.Object);
            var mockRoleManager = MockRoleManager();
            var mockUow = new Mock<IUnitOfWork>();

            var inactiveUser = new ApplicationUser { UserName = "test@test.com", IsActive = false };

            // Simulam login cu parola corecta
            mockSignInManager.Setup(s => s.PasswordSignInAsync("test@test.com", "parola", false, false))
                             .ReturnsAsync(SignInResult.Success);

            mockUserManager.Setup(u => u.FindByEmailAsync("test@test.com")).ReturnsAsync(inactiveUser);

            var service = new AuthService(mockUserManager.Object, mockSignInManager.Object, mockRoleManager.Object, mockUow.Object);

            // Act
            var result = await service.LoginAsync("test@test.com", "parola", false);

            // Assert
            Assert.False(result.Success);
            mockSignInManager.Verify(s => s.SignOutAsync(), Times.Once); // Daca era inactiv, trebuie fortat delogarea (fallback)
        }

        // TEST 7 : BR-3/REQ-7 - Atribuirea de roluri (creeaza rolul daca acesta nu exista inca)
        [Fact]
        public async Task AssignRoleAsync_CandRolulNuExista_IlCreeazaSiAtribuie()
        {
            // Arrange
            var mockUserManager = MockUserManager();
            var mockSignInManager = MockSignInManager(mockUserManager.Object);
            var mockRoleManager = MockRoleManager();
            var mockUow = new Mock<IUnitOfWork>();

            var user = new ApplicationUser { Id = "user1" };
            mockUserManager.Setup(u => u.FindByIdAsync("user1")).ReturnsAsync(user);

            // Simulam ca rolul "Staff" nu exista in DB
            mockRoleManager.Setup(r => r.RoleExistsAsync("Staff")).ReturnsAsync(false);

            // CreateAsync si AddToRoleAsync TREBUIE setate sa returneze
            // IdentityResult.Success, altfel 'result.Succeeded' din AuthService
            // produce NullReferenceException (Moq returneaza null implicit).
            mockRoleManager
                .Setup(r => r.CreateAsync(It.Is<IdentityRole>(ir => ir.Name == "Staff")))
                .ReturnsAsync(IdentityResult.Success);
            mockUserManager
                .Setup(u => u.AddToRoleAsync(user, "Staff"))
                .ReturnsAsync(IdentityResult.Success);

            var service = new AuthService(mockUserManager.Object, mockSignInManager.Object, mockRoleManager.Object, mockUow.Object);

            // Act
            var result = await service.AssignRoleAsync("user1", "Staff");

            // Assert
            Assert.True(result.Success);
            mockRoleManager.Verify(r => r.CreateAsync(It.Is<IdentityRole>(role => role.Name == "Staff")), Times.Once);
            mockUserManager.Verify(u => u.AddToRoleAsync(user, "Staff"), Times.Once);
        }

        #region Helper Methods pentru Mock-uirea ASP.NET Core Identity
        private Mock<UserManager<ApplicationUser>> MockUserManager()
        {
            var store = new Mock<IUserStore<ApplicationUser>>();
            return new Mock<UserManager<ApplicationUser>>(store.Object, null, null, null, null, null, null, null, null);
        }

        private Mock<SignInManager<ApplicationUser>> MockSignInManager(UserManager<ApplicationUser> userManager)
        {
            var contextAccessor = new Mock<IHttpContextAccessor>();
            var claimsFactory = new Mock<IUserClaimsPrincipalFactory<ApplicationUser>>();
            return new Mock<SignInManager<ApplicationUser>>(userManager, contextAccessor.Object, claimsFactory.Object, null, null, null, null);
        }

        private Mock<RoleManager<IdentityRole>> MockRoleManager()
        {
            var store = new Mock<IRoleStore<IdentityRole>>();
            return new Mock<RoleManager<IdentityRole>>(store.Object, null, null, null, null);
        }
        #endregion
    }
}
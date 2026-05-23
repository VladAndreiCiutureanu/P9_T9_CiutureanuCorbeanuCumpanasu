using AirlineFlightManagement.DataAccess.Repositories.Interfaces;
using AirlineFlightManagement.Models.Models;
using AirlineFlightManagement.Services.Implementations;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace AirlineFlightManagement.Tests
{
    public class PassengerService_Tests
    {
        // TEST 1 : Verificăm dacă serviciul returnează corect profilul unui pasager
        [Fact]
        public async Task GetByUserIdAsync_ReturneazaProfilulCorect()
        {
            // Arrange
            var mockUow = new Mock<IUnitOfWork>();
            var mockRepo = new Mock<IPassengerProfileRepository>();
            mockUow.Setup(u => u.PassengerProfileRepository).Returns(mockRepo.Object);

            var expectedProfile = new PassengerProfile { PassengerId = 1, UserId = "user123", FirstName = "Ion", LastName = "Popescu" };
            mockRepo.Setup(r => r.GetByUserIdAsync("user123")).ReturnsAsync(expectedProfile);

            var service = new PassengerService(mockUow.Object);

            // Act
            var result = await service.GetByUserIdAsync("user123");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Ion", result.FirstName);
        }

        // TEST 2 : REQ-15 - Când se încearcă actualizarea unui profil inexistent, trebuie să arunce eroare
        [Fact]
        public async Task UpdateProfileAsync_CandNuExistaProfil_AruncaKeyNotFoundException()
        {
            // Arrange
            var mockUow = new Mock<IUnitOfWork>();
            var mockRepo = new Mock<IPassengerProfileRepository>();
            mockUow.Setup(u => u.PassengerProfileRepository).Returns(mockRepo.Object);

            // Simulam ca nu s-a gasit profilul in baza de date
            mockRepo.Setup(r => r.GetByUserIdAsync(It.IsAny<string>())).ReturnsAsync((PassengerProfile?)null);

            var service = new PassengerService(mockUow.Object);

            // Act & Assert
            await Assert.ThrowsAsync<System.Collections.Generic.KeyNotFoundException>(
                () => service.UpdateProfileAsync("invalid_user", "Ion", "Pop", "0700000000", "Bucuresti"));
        }

        // TEST 3 : REQ-15 - Profilul este actualizat cu succes și salvat în baza de date
        [Fact]
        public async Task UpdateProfileAsync_Succes_ActualizeazaSiSalveaza()
        {
            // Arrange
            var mockUow = new Mock<IUnitOfWork>();
            var mockRepo = new Mock<IPassengerProfileRepository>();
            mockUow.Setup(u => u.PassengerProfileRepository).Returns(mockRepo.Object);

            var existingProfile = new PassengerProfile { UserId = "user123", FirstName = "Vechi", LastName = "Vechi" };
            mockRepo.Setup(r => r.GetByUserIdAsync("user123")).ReturnsAsync(existingProfile);

            var service = new PassengerService(mockUow.Object);

            // Act
            await service.UpdateProfileAsync("user123", "Nou", "Nou", "0722222222", "Cluj");

            // Assert
            Assert.Equal("Nou", existingProfile.FirstName);
            Assert.Equal("0722222222", existingProfile.PhoneNumber);
            mockRepo.Verify(r => r.Update(existingProfile), Times.Once); // Verifica ca s-a apelat Update
            mockUow.Verify(u => u.SaveAsync(), Times.Once); // Verifica ca s-a salvat in DB
        }
    }
}
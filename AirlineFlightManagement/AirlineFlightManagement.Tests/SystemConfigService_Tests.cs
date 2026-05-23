using AirlineFlightManagement.DataAccess.Repositories.Interfaces;
using AirlineFlightManagement.Models.Models;
using AirlineFlightManagement.Services.Implementations;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace AirlineFlightManagement.Tests
{
    public class SystemConfigService_Tests
    {
        private readonly Mock<IUnitOfWork> _mockUow;
        private readonly Mock<ISystemConfigurationRepository> _mockConfigRepo;
        private readonly SystemConfigService _configService;

        public SystemConfigService_Tests()
        {
            _mockUow = new Mock<IUnitOfWork>();
            _mockConfigRepo = new Mock<ISystemConfigurationRepository>();
            _mockUow.Setup(u => u.SystemConfigurationRepository).Returns(_mockConfigRepo.Object);

            _configService = new SystemConfigService(_mockUow.Object);
        }

        [Fact]
        public async Task SystemConfigService_SetValue_CreeazaSiActualizeaza()
        {
            // Arrange
            var key = "TestKey";
            var value = "TestValue";

            _mockConfigRepo.Setup(r => r.GetByKeyAsync(key)).ReturnsAsync((SystemConfiguration?)null);

            // Act
            await _configService.SetValueAsync(key, value);

            // Assert
            _mockConfigRepo.Verify(r => r.AddAsync(It.Is<SystemConfiguration>(c => c.SettingKey == key && c.SettingValue == value)), Times.Once);
            _mockUow.Verify(u => u.SaveAsync(), Times.Once);
        }
    }
}

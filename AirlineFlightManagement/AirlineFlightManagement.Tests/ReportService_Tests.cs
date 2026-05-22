using AirlineFlightManagement.DataAccess.Repositories.Interfaces;
using AirlineFlightManagement.Models.Models;
using AirlineFlightManagement.Services.Implementations;
using Moq;

namespace AirlineFlightManagement.Tests;

/// <summary>
/// Teste pentru ReportService — slice-ul Coleg C.
/// Acopera: REQ-46 (raport ocupare zbor — procent calculat corect).
/// </summary>
public class ReportService_Tests
{
    //  TEST 7 — REQ-46: GetFlightOccupancyAsync calculeaza procentul
    //  de ocupare corect din capacitate vs locuri vandute.
    [Fact]
    public async Task GetFlightOccupancyAsync_CalculeazaProcentCorect()
    {
        var mockUow = new Mock<IUnitOfWork>();
        var mockFlightRepo = new Mock<IFlightRepository>();
        mockUow.Setup(u => u.FlightRepository).Returns(mockFlightRepo.Object);

        // Construim 100 locuri total: 30 vandute (IsAvailable=false)
        // si 70 disponibile (IsAvailable=true). Ocupare = 30%.
        var seats = new List<FlightSeat>();
        for (int i = 0; i < 30; i++)
            seats.Add(new FlightSeat { SeatNumber = $"S{i:D3}", IsAvailable = false });
        for (int i = 30; i < 100; i++)
            seats.Add(new FlightSeat { SeatNumber = $"S{i:D3}", IsAvailable = true });

        var flight = new Flight
        {
            FlightId = 1,
            AirlineName = "Tarom",
            Source = "OTP",
            Destination = "MAD",
            DepartureTime = DateTime.UtcNow.AddDays(7),
            Aircraft = new Aircraft
            {
                AircraftId = 1,
                ModelName = "A320",
                MaxCapacity = 100
            },
            FlightSeats = seats
        };

        mockFlightRepo.Setup(f => f.GetWithDetailsAsync(1)).ReturnsAsync(flight);

        var service = new ReportService(mockUow.Object);

        var report = await service.GetFlightOccupancyAsync(1);

        Assert.Equal(100, report.TotalSeats);
        Assert.Equal(30, report.SoldSeats);
        Assert.Equal(30m, report.OccupancyPercentage);  // 30 / 100 × 100 = 30%

        // Datele zborului transferate corect in DTO
        Assert.Equal(1, report.FlightId);
        Assert.Equal("Tarom", report.AirlineName);
        Assert.Equal("OTP", report.Source);
        Assert.Equal("MAD", report.Destination);
    }
}

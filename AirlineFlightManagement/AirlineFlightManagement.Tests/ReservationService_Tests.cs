using System.Linq.Expressions;
using AirlineFlightManagement.DataAccess.Repositories.Interfaces;
using AirlineFlightManagement.Models.Enums;
using AirlineFlightManagement.Models.Models;
using AirlineFlightManagement.Services.Implementations;
using AirlineFlightManagement.Services.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;
using Moq;

namespace AirlineFlightManagement.Tests;

/// <summary>
/// Teste pentru ReservationService — slice-ul Coleg C.
/// Acopera: BR-1 (overbooking), BR-2 (24h cancel), BR-4 (rezervare duplicata),
///          REQ-32, REQ-33, REQ-38, REQ-41.
/// </summary>
public class ReservationService_Tests
{
    //  TEST 1 — BR-1: overbooking prevention
    //  CreateAsync trebuie sa refuze rezervarea cand numarul de locuri
    //  confirmate >= MaxCapacity al aeronavei.
    [Fact]
    public async Task CreateAsync_CandZborulEFull_Arunca()
    {
        // Mock-uri pentru cele 3 dependinte ale ReservationService
        var mockUow = new Mock<IUnitOfWork>();
        var mockSerpApi = new Mock<ISerpApiClient>();
        var mockMarkup = new Mock<IMarkupService>();

        // Repository-urile pe care le acceseaza serviciul
        var mockReservationRepo = new Mock<IReservationRepository>();
        var mockFlightRepo = new Mock<IFlightRepository>();
        var mockFlightClassRepo = new Mock<IFlightClassRepository>();
        var mockSeatRepo = new Mock<IFlightSeatRepository>();

        mockUow.Setup(u => u.ReservationRepository).Returns(mockReservationRepo.Object);
        mockUow.Setup(u => u.FlightRepository).Returns(mockFlightRepo.Object);
        mockUow.Setup(u => u.FlightClassRepository).Returns(mockFlightClassRepo.Object);
        mockUow.Setup(u => u.FlightSeatRepository).Returns(mockSeatRepo.Object);

        // BR-4 nu trebuie sa fie triggered (pasagerul NU are deja
        // rezervare activa) — vrem sa ajungem la BR-1.
        mockReservationRepo
            .Setup(r => r.HasActiveReservationAsync(
                It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(false);

        // Construim un zbor cu capacitate 100 locuri si status Scheduled.
        // Cheia pentru BR-1: MaxCapacity = 100.
        var aircraft = new Aircraft
        {
            AircraftId = 1,
            ModelName = "Boeing 737-800",
            MaxCapacity = 100
        };
        var flightClass = new FlightClass
        {
            FlightClassId = 1,
            FlightId = 1,
            ClassName = "Economy",
            Price = 100m
        };
        var flight = new Flight
        {
            FlightId = 1,
            ExternalApiId = "test-flight-otp-mad",
            AircraftId = 1,
            Aircraft = aircraft,
            Source = "OTP",
            Destination = "MAD",
            AirlineName = "Tarom",
            DepartureTime = DateTime.UtcNow.AddDays(7),
            ArrivalTime = DateTime.UtcNow.AddDays(7).AddHours(4),
            Status = FlightStatus.Scheduled,
            FlightClasses = new List<FlightClass> { flightClass },
            FlightSeats = new List<FlightSeat>()
        };

        mockFlightRepo
            .Setup(f => f.GetWithDetailsAsync(1))
            .ReturnsAsync(flight);

        // REQ-25: API-ul extern confirma disponibilitatea zborului.
        mockSerpApi
            .Setup(s => s.VerifyAvailabilityAsync(It.IsAny<string>()))
            .ReturnsAsync(true);

        // Clasa zborului exista in DB si apartine zborului indicat.
        mockFlightClassRepo
            .Setup(fc => fc.GetByIdAsync(1))
            .ReturnsAsync(flightClass);

        // Markup-ul platformei (valoarea concreta nu conteaza pentru
        //       acest test — verificam BR-1, nu pretul).
        mockMarkup
            .Setup(m => m.GetPlatformMarkupAsync())
            .ReturnsAsync(25m);

        // Tranzactia — un mock simplu (nu apelam metode pe el daca
        // cade la BR-1; Dispose-ul automat pe `using var tx = ...`
        // primeste o implementare no-op de la Mock.Of).
        mockUow
            .Setup(u => u.BeginTransactionAsync())
            .ReturnsAsync(Mock.Of<IDbContextTransaction>());

        // TRIGGERUL BR-1: deja exista 100 rezervari confirmate,
        //       egal cu MaxCapacity ⇒ orice rezervare noua trebuie refuzata.
        mockReservationRepo
            .Setup(r => r.CountConfirmedForFlightAsync(1))
            .ReturnsAsync(100);

        // Instantiem serviciul cu mock-urile pregatite.
        var service = new ReservationService(
            mockUow.Object,
            mockSerpApi.Object,
            mockMarkup.Object);

        // Asteptam ca metoda sa arunce InvalidOperationException.
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.CreateAsync(
                passengerId: 1,
                flightId: 1,
                flightClassId: 1));

        // Mesajul de eroare trebuie sa contina "full" (case-insensitive)
        // — confirma ca a fost respinsa din motivul corect (BR-1).
        Assert.Contains("full", ex.Message, StringComparison.OrdinalIgnoreCase);

        // Verificare suplimentara: rezervarea NU a fost salvata (SaveAsync
        // nu trebuie sa fie apelat daca booking-ul a fost refuzat).
        mockUow.Verify(u => u.SaveAsync(), Times.Never);
    }

    //  TEST 2 — BR-4: un pasager NU poate avea mai mult de o rezervare
    //  activa pe acelasi zbor.
    [Fact]
    public async Task CreateAsync_CandPasagerulAreRezervareActiva_Arunca()
    {
        var mockUow = new Mock<IUnitOfWork>();
        var mockSerpApi = new Mock<ISerpApiClient>();
        var mockMarkup = new Mock<IMarkupService>();

        var mockReservationRepo = new Mock<IReservationRepository>();
        mockUow
            .Setup(u => u.ReservationRepository)
            .Returns(mockReservationRepo.Object);

        // TRIGGERUL BR-4: pasagerul cu ID=1 are deja rezervare activa
        // (status != Cancelled) pe zborul cu ID=1 → trebuie refuzata orice
        // rezervare noua pe acelasi zbor.
        mockReservationRepo
            .Setup(r => r.HasActiveReservationAsync(1, 1))
            .ReturnsAsync(true);

        var service = new ReservationService(
            mockUow.Object,
            mockSerpApi.Object,
            mockMarkup.Object);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.CreateAsync(
                passengerId: 1,
                flightId: 1,
                flightClassId: 1));

        // Mesajul de eroare confirma motivul corect (BR-4).
        Assert.Contains("rezervare", ex.Message, StringComparison.OrdinalIgnoreCase);

        // Verificam ca short-circuit-ul a functionat: codul a iesit la BR-4,
        // fara sa atinga celelalte servicii / repos.
        mockUow.Verify(u => u.FlightRepository, Times.Never);
        mockSerpApi.Verify(s => s.VerifyAvailabilityAsync(It.IsAny<string>()), Times.Never);
        mockUow.Verify(u => u.BeginTransactionAsync(), Times.Never);
        mockUow.Verify(u => u.SaveAsync(), Times.Never);
    }

    //  TEST 3 — REQ-32 + REQ-33: la o rezervare reusita, locul devine
    //  IsAvailable=false, iar rezervarea are Status=Pending si timestamp setat.
    [Fact]
    public async Task CreateAsync_LaSuccess_MarcheazaSeatOcupatSiStatusPending()
    {
        // Setup-ul de baza (acelasi schelet ca Test 1)
        var mockUow = new Mock<IUnitOfWork>();
        var mockSerpApi = new Mock<ISerpApiClient>();
        var mockMarkup = new Mock<IMarkupService>();

        var mockReservationRepo = new Mock<IReservationRepository>();
        var mockFlightRepo = new Mock<IFlightRepository>();
        var mockFlightClassRepo = new Mock<IFlightClassRepository>();
        var mockSeatRepo = new Mock<IFlightSeatRepository>();

        mockUow.Setup(u => u.ReservationRepository).Returns(mockReservationRepo.Object);
        mockUow.Setup(u => u.FlightRepository).Returns(mockFlightRepo.Object);
        mockUow.Setup(u => u.FlightClassRepository).Returns(mockFlightClassRepo.Object);
        mockUow.Setup(u => u.FlightSeatRepository).Returns(mockSeatRepo.Object);

        mockReservationRepo
            .Setup(r => r.HasActiveReservationAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(false);

        var aircraft = new Aircraft { AircraftId = 1, ModelName = "A320", MaxCapacity = 100 };
        var flightClass = new FlightClass { FlightClassId = 1, FlightId = 1, ClassName = "Economy", Price = 100m };
        var seat = new FlightSeat
        {
            FlightSeatId = 10,
            FlightId = 1,
            FlightClassId = 1,
            SeatNumber = "E01",
            IsAvailable = true  // initial liber — va deveni ocupat
        };
        var flight = new Flight
        {
            FlightId = 1,
            ExternalApiId = "test-flight",
            Aircraft = aircraft,
            AircraftId = 1,
            Source = "OTP", Destination = "MAD",
            AirlineName = "Tarom",
            DepartureTime = DateTime.UtcNow.AddDays(7),
            ArrivalTime = DateTime.UtcNow.AddDays(7).AddHours(4),
            Status = FlightStatus.Scheduled,
            FlightClasses = new List<FlightClass> { flightClass },
            FlightSeats = new List<FlightSeat> { seat }
        };

        mockFlightRepo.Setup(f => f.GetWithDetailsAsync(1)).ReturnsAsync(flight);
        mockSerpApi.Setup(s => s.VerifyAvailabilityAsync(It.IsAny<string>())).ReturnsAsync(true);
        mockFlightClassRepo.Setup(fc => fc.GetByIdAsync(1)).ReturnsAsync(flightClass);
        mockMarkup.Setup(m => m.GetPlatformMarkupAsync()).ReturnsAsync(25m);

        // BR-1: NU full (0 rezervari din 100)
        mockReservationRepo.Setup(r => r.CountConfirmedForFlightAsync(1)).ReturnsAsync(0);

        // Locuri disponibile pentru zbor+clasa
        mockSeatRepo
            .Setup(s => s.GetAvailableAsync(1, 1, true))
            .ReturnsAsync(new[] { seat });

        // Captam Reservation-ul cand se adauga, ca sa-i verificam proprietatile
        Reservation? capturedReservation = null;
        mockReservationRepo
            .Setup(r => r.AddAsync(It.IsAny<Reservation>()))
            .Callback<Reservation>(r => capturedReservation = r)
            .Returns(Task.CompletedTask);

        // Tranzactia trebuie sa returneze Task valid pentru CommitAsync
        // (altfel `await tx.CommitAsync()` crapa cu NullReferenceException)
        var mockTx = new Mock<IDbContextTransaction>();
        mockTx.Setup(t => t.CommitAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        mockUow.Setup(u => u.BeginTransactionAsync()).ReturnsAsync(mockTx.Object);
        mockUow.Setup(u => u.SaveAsync()).Returns(Task.CompletedTask);

        var service = new ReservationService(mockUow.Object, mockSerpApi.Object, mockMarkup.Object);

        // Act
        var result = await service.CreateAsync(passengerId: 1, flightId: 1, flightClassId: 1);

        // Assert
        // REQ-32: locul a fost marcat ca ocupat
        Assert.False(seat.IsAvailable);

        // REQ-33 + Status: rezervarea are Pending si timestamp recent
        Assert.NotNull(capturedReservation);
        Assert.Equal(ReservationStatus.Pending, capturedReservation!.Status);
        Assert.True((DateTime.UtcNow - capturedReservation.ReservationTimeStamp).TotalSeconds < 10);

        // Persistenta a fost finalizata
        mockUow.Verify(u => u.SaveAsync(), Times.Once);
        mockTx.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    //  TEST 4 — REQ-38: TotalPrice = pretul clasei + markup-ul platformei.
    //  Pretul vazut in Search trebuie sa fie cel platit la final.
    [Fact]
    public async Task CreateAsync_CalculeazaTotalPriceCuMarkup()
    {
        // Setup identic cu Test 3, dar verificam DOAR TotalPrice
        var mockUow = new Mock<IUnitOfWork>();
        var mockSerpApi = new Mock<ISerpApiClient>();
        var mockMarkup = new Mock<IMarkupService>();

        var mockReservationRepo = new Mock<IReservationRepository>();
        var mockFlightRepo = new Mock<IFlightRepository>();
        var mockFlightClassRepo = new Mock<IFlightClassRepository>();
        var mockSeatRepo = new Mock<IFlightSeatRepository>();

        mockUow.Setup(u => u.ReservationRepository).Returns(mockReservationRepo.Object);
        mockUow.Setup(u => u.FlightRepository).Returns(mockFlightRepo.Object);
        mockUow.Setup(u => u.FlightClassRepository).Returns(mockFlightClassRepo.Object);
        mockUow.Setup(u => u.FlightSeatRepository).Returns(mockSeatRepo.Object);

        mockReservationRepo
            .Setup(r => r.HasActiveReservationAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(false);

        var aircraft = new Aircraft { AircraftId = 1, MaxCapacity = 100, ModelName = "A320" };
        // Pret de baza: 200 RON
        var flightClass = new FlightClass { FlightClassId = 1, FlightId = 1, ClassName = "Business", Price = 200m };
        var seat = new FlightSeat { FlightSeatId = 5, FlightId = 1, FlightClassId = 1, SeatNumber = "B01", IsAvailable = true };
        var flight = new Flight
        {
            FlightId = 1,
            ExternalApiId = "test-markup",
            Aircraft = aircraft, AircraftId = 1,
            Source = "OTP", Destination = "MAD", AirlineName = "Tarom",
            DepartureTime = DateTime.UtcNow.AddDays(7),
            ArrivalTime = DateTime.UtcNow.AddDays(7).AddHours(4),
            Status = FlightStatus.Scheduled,
            FlightClasses = new List<FlightClass> { flightClass },
            FlightSeats = new List<FlightSeat> { seat }
        };

        mockFlightRepo.Setup(f => f.GetWithDetailsAsync(1)).ReturnsAsync(flight);
        mockSerpApi.Setup(s => s.VerifyAvailabilityAsync(It.IsAny<string>())).ReturnsAsync(true);
        mockFlightClassRepo.Setup(fc => fc.GetByIdAsync(1)).ReturnsAsync(flightClass);

        // Markup: 50 RON
        mockMarkup.Setup(m => m.GetPlatformMarkupAsync()).ReturnsAsync(50m);

        mockReservationRepo.Setup(r => r.CountConfirmedForFlightAsync(1)).ReturnsAsync(0);
        mockSeatRepo.Setup(s => s.GetAvailableAsync(1, 1, true)).ReturnsAsync(new[] { seat });

        Reservation? capturedReservation = null;
        mockReservationRepo
            .Setup(r => r.AddAsync(It.IsAny<Reservation>()))
            .Callback<Reservation>(r => capturedReservation = r)
            .Returns(Task.CompletedTask);

        var mockTx = new Mock<IDbContextTransaction>();
        mockTx.Setup(t => t.CommitAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        mockUow.Setup(u => u.BeginTransactionAsync()).ReturnsAsync(mockTx.Object);
        mockUow.Setup(u => u.SaveAsync()).Returns(Task.CompletedTask);

        var service = new ReservationService(mockUow.Object, mockSerpApi.Object, mockMarkup.Object);

        // Act
        await service.CreateAsync(passengerId: 1, flightId: 1, flightClassId: 1);

        // Assert: TotalPrice = 200 (clasa) + 50 (markup) = 250
        Assert.NotNull(capturedReservation);
        Assert.Equal(250m, capturedReservation!.TotalPrice);
    }

    //  TEST 5 — BR-2: anularea independenta e permisa doar cu minim 24h
    //  inainte de plecare. Sub 24h ⇒ exceptie.
    [Fact]
    public async Task CancelAsync_SubFereastraDe24h_Arunca()
    {
        var mockUow = new Mock<IUnitOfWork>();
        var mockSerpApi = new Mock<ISerpApiClient>();
        var mockMarkup = new Mock<IMarkupService>();

        var mockReservationRepo = new Mock<IReservationRepository>();
        var mockSeatRepo = new Mock<IFlightSeatRepository>();
        mockUow.Setup(u => u.ReservationRepository).Returns(mockReservationRepo.Object);
        mockUow.Setup(u => u.FlightSeatRepository).Returns(mockSeatRepo.Object);

        // Rezervare cu zborul peste DOAR 5 ore (sub fereastra de 24h)
        var reservation = new Reservation
        {
            ReservationId = 1,
            PassengerId = 1,
            FlightSeatId = 10,
            Status = ReservationStatus.Pending,
            Flight = new Flight
            {
                FlightId = 1,
                DepartureTime = DateTime.UtcNow.AddHours(5),  // ← sub 24h
                Source = "OTP", Destination = "MAD"
            },
            Seat = new FlightSeat { FlightSeatId = 10, IsAvailable = false }
        };

        // Mock GetAsync (varianta tracked cu Include-uri) — folosita de CancelAsync
        mockReservationRepo
            .Setup(r => r.GetAsync(
                It.IsAny<Expression<Func<Reservation, bool>>>(),
                true,
                It.IsAny<Expression<Func<Reservation, object>>[]>()))
            .ReturnsAsync(reservation);

        var service = new ReservationService(mockUow.Object, mockSerpApi.Object, mockMarkup.Object);

        // Act + Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.CancelAsync(reservationId: 1, passengerId: 1));

        // Mesajul contine "24" (referinta la fereastra orara)
        Assert.Contains("24", ex.Message);

        // Anularea NU s-a finalizat
        mockUow.Verify(u => u.SaveAsync(), Times.Never);
    }
}

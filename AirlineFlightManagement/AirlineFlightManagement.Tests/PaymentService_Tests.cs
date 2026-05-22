using System.Linq.Expressions;
using AirlineFlightManagement.DataAccess.Repositories.Interfaces;
using AirlineFlightManagement.Models.Enums;
using AirlineFlightManagement.Models.Models;
using AirlineFlightManagement.Services.Implementations;
using Microsoft.EntityFrameworkCore.Storage;
using Moq;

namespace AirlineFlightManagement.Tests;

/// <summary>
/// Teste pentru PaymentService — slice-ul Coleg C.
/// Acopera: REQ-45 (Reservation devine ConfirmedAndPaid),
///          REQ-47 (TransactionId unic generat).
/// </summary>
public class PaymentService_Tests
{
    //  TEST 6 — REQ-45: la plata reusita, status-ul rezervarii devine
    //  ConfirmedAndPaid si payment-ul are Status=Completed.
    [Fact]
    public async Task ProcessAsync_LaPlataReusita_ActualizeazaStatusConfirmedAndPaid()
    {
        var mockUow = new Mock<IUnitOfWork>();
        var mockReservationRepo = new Mock<IReservationRepository>();
        var mockPaymentRepo = new Mock<IPaymentRepository>();

        mockUow.Setup(u => u.ReservationRepository).Returns(mockReservationRepo.Object);
        mockUow.Setup(u => u.PaymentRepository).Returns(mockPaymentRepo.Object);

        // Rezervare pregatita pentru plata: Status=Pending, pret=125 RON.
        // Serviciul o va modifica in-place la ConfirmedAndPaid.
        var reservation = new Reservation
        {
            ReservationId = 1,
            Status = ReservationStatus.Pending,
            TotalPrice = 125m,
            Flight = new Flight { FlightId = 1 }
        };

        // ReservationService o incarca via GetAsync (tracked + Include Flight)
        mockReservationRepo
            .Setup(r => r.GetAsync(
                It.IsAny<Expression<Func<Reservation, bool>>>(),
                true,
                It.IsAny<Expression<Func<Reservation, object>>[]>()))
            .ReturnsAsync(reservation);

        // Captam payment-ul cand se adauga
        Payment? capturedPayment = null;
        mockPaymentRepo
            .Setup(p => p.AddAsync(It.IsAny<Payment>()))
            .Callback<Payment>(p => capturedPayment = p)
            .Returns(Task.CompletedTask);

        // Tranzactia trebuie sa returneze Task.CompletedTask pentru CommitAsync
        var mockTx = new Mock<IDbContextTransaction>();
        mockTx.Setup(t => t.CommitAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        mockUow.Setup(u => u.BeginTransactionAsync()).ReturnsAsync(mockTx.Object);
        mockUow.Setup(u => u.SaveAsync()).Returns(Task.CompletedTask);

        var service = new PaymentService(mockUow.Object);

        var result = await service.ProcessAsync(
            reservationId: 1,
            method: PaymentMethod.CreditCard,
            amount: 125m);

        // Rezultatul indica success cu TransactionId non-null
        Assert.True(result.Success);
        Assert.NotNull(result.TransactionId);
        Assert.NotEmpty(result.TransactionId!);

        // REQ-45: statusul rezervarii s-a schimbat
        Assert.Equal(ReservationStatus.ConfirmedAndPaid, reservation.Status);

        // Payment-ul a fost creat cu valorile asteptate
        Assert.NotNull(capturedPayment);
        Assert.Equal(PaymentStatus.Completed, capturedPayment!.Status);
        Assert.Equal(125m, capturedPayment.Amount);
        Assert.Equal(PaymentMethod.CreditCard, capturedPayment.PaymentMethod);
        Assert.Equal(result.TransactionId, capturedPayment.TransactionId);

        // SaveAsync + CommitAsync au fost apelate exact o data
        mockUow.Verify(u => u.SaveAsync(), Times.Once);
        mockTx.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}

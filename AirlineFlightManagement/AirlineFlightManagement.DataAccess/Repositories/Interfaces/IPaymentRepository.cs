using AirlineFlightManagement.Models.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace AirlineFlightManagement.DataAccess.Repositories.Interfaces
{
    public interface IPaymentRepository : IRepository<Payment>
    {
        Task<Payment?> GetByTransactionIdAsync(string transactionId);
        Task<IEnumerable<Payment>> GetByPassengerIdAsync(int passengerId);
        Task<IEnumerable<Payment>> GetByDateRangeAsync(DateTime from, DateTime to);
        Task<IEnumerable<Payment>> GetByReservationAsync(int reservationId);
    }
}

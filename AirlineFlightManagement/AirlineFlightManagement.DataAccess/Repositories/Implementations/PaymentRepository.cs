using AirlineFlightManagement.DataAccess.Data;
using AirlineFlightManagement.DataAccess.Repositories.Interfaces;
using AirlineFlightManagement.Models.Models;
using Microsoft.EntityFrameworkCore;

namespace AirlineFlightManagement.DataAccess.Repositories.Implementations
{
    public class PaymentRepository : Repository<Payment>, IPaymentRepository
    {
        public PaymentRepository(ApplicationDbContext db) : base(db)
        {
        }

        public async Task<IEnumerable<Payment>> GetByDateRangeAsync(DateTime from, DateTime to)
        {
            return await _dbSet.AsNoTracking()
                .Where(p => p.TransactionDate >= from && p.TransactionDate <= to)
                .OrderByDescending(p => p.TransactionDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Payment>> GetByPassengerIdAsync(int passengerId)
        {
            // Includem Reservation -> Flight pentru ca pagina de istoric plati
            // afiseaza informatii despre zbor (sursa/destinatie/data) langa
            // fiecare tranzactie. Fara Include am avea N+1 queries.
            return await _dbSet.AsNoTracking()
                .Include(p => p.Reservation)
                    .ThenInclude(r => r!.Flight)
                .Where(p => p.Reservation!.PassengerId == passengerId)
                .OrderByDescending(p => p.TransactionDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Payment>> GetByReservationAsync(int reservationId)
        {
            return await _dbSet.AsNoTracking()
                .Where(p => p.ReservationId == reservationId)
                .ToListAsync();
        }

        public Task<Payment?> GetByTransactionIdAsync(string transactionId)
        {
            return _dbSet.AsNoTracking()
                .FirstOrDefaultAsync(p => p.TransactionId == transactionId);
        }
    }
}

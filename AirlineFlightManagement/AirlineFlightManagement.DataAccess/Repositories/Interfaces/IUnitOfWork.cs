using Microsoft.EntityFrameworkCore.Storage;

namespace AirlineFlightManagement.DataAccess.Repositories.Interfaces
{
    public interface IUnitOfWork
    {
        IAircraftRepository AircraftRepository { get; }
        IFlightRepository FlightRepository { get; }
        IFlightClassRepository FlightClassRepository { get; }
        IFlightSeatRepository FlightSeatRepository { get; }
        IPassengerProfileRepository PassengerProfileRepository { get; }
        IReservationRepository ReservationRepository { get; }
        IPaymentRepository PaymentRepository { get; }
        IApiLogRepository ApiLogRepository { get; }
        ISystemConfigurationRepository SystemConfigurationRepository { get; }

        // Persista in DB toate modificarile facute pe entitatile tracked.
        // EF Core invelește implicit acest call intr-o tranzactie atomica.
        Task SaveAsync();

        // Porneste o tranzactie explicita pentru cazurile cand avem
        // nevoie sa facem mai multe SaveAsync sau verificari intermediare
        // sub aceeasi izolare (ex: ReservationService.CreateAsync pentru BR-1).
        // Apelantul e responsabil sa cheme CommitAsync / RollbackAsync.
        Task<IDbContextTransaction> BeginTransactionAsync();
    }
}

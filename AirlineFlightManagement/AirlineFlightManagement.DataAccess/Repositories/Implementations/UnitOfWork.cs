using AirlineFlightManagement.DataAccess.Data;
using AirlineFlightManagement.DataAccess.Repositories.Interfaces;

namespace AirlineFlightManagement.DataAccess.Repositories.Implementations
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _db;

        public IAircraftRepository AircraftRepository { get; }
        public IFlightRepository FlightRepository { get; }
        public IFlightClassRepository FlightClassRepository { get; }
        public IFlightSeatRepository FlightSeatRepository { get; }
        public IPassengerProfileRepository PassengerProfileRepository { get; }
        public IReservationRepository ReservationRepository { get; }
        public IPaymentRepository PaymentRepository { get; }
        public IApiLogRepository ApiLogRepository { get; }
        public ISystemConfigurationRepository SystemConfigurationRepository { get; }

        public UnitOfWork(ApplicationDbContext db)
        {
            _db = db;
            AircraftRepository = new AircraftRepository(_db);
            FlightRepository = new FlightRepository(_db);
            FlightClassRepository = new FlightClassRepository(_db);
            FlightSeatRepository = new FlightSeatRepository(_db);
            PassengerProfileRepository = new PassengerProfileRepository(_db);
            ReservationRepository = new ReservationRepository(_db);
            PaymentRepository = new PaymentRepository(_db);
            ApiLogRepository = new ApiLogRepository(_db);
            SystemConfigurationRepository = new SystemConfigurationRepository(_db);
        }

        public async Task SaveAsync()
        {
            await _db.SaveChangesAsync();
        }
    }
}

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

        Task SaveAsync();
    }
}

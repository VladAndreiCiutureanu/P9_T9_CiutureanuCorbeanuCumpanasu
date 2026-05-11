using AirlineFlightManagement.DataAccess.Repositories.Interfaces;
using AirlineFlightManagement.Models.Enums;
using AirlineFlightManagement.Models.Models;
using AirlineFlightManagement.Services.Contracts;
using AirlineFlightManagement.Services.Interfaces;

namespace AirlineFlightManagement.Services.Implementations
{
    /// <summary>
    /// Genereaza rapoartele agregate pentru Admin / Staff:
    ///   - REQ-46 (raport ocupare zbor)
    ///   - REQ-44 (raport financiar pe interval)
    ///
    /// NOTA: pentru proiecte la scara mare, agregari de tipul GROUP BY
    /// ar fi mai rapide pushate direct in SQL prin DbContext. Pentru
    /// dimensiunea acestui proiect (sute de zboruri / mii de plati pe an),
    /// loop-uri in memorie sunt suficient de rapide si pastreaza codul
    /// curat la nivel de repository.
    /// </summary>
    public class ReportService : IReportService
    {
        private readonly IUnitOfWork _uow;

        public ReportService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        // ─────────────────────────────────────────────────────────────────────
        //  GetFlightOccupancyAsync — REQ-46 pentru un singur zbor
        // ─────────────────────────────────────────────────────────────────────
        public async Task<OccupancyReport> GetFlightOccupancyAsync(int flightId)
        {
            if (flightId <= 0)
                throw new ArgumentException("Flight ID invalid.", nameof(flightId));

            // GetWithDetailsAsync include Aircraft (pentru MaxCapacity)
            // si FlightSeats (pentru numaratoare locuri vandute).
            var flight = await _uow.FlightRepository.GetWithDetailsAsync(flightId)
                ?? throw new KeyNotFoundException($"Zbor inexistent: ID {flightId}.");

            return BuildOccupancyReport(flight);
        }

        // ─────────────────────────────────────────────────────────────────────
        //  GetOccupancyForRangeAsync — REQ-46 extins pe interval
        // ─────────────────────────────────────────────────────────────────────
        public async Task<IEnumerable<OccupancyReport>> GetOccupancyForRangeAsync(
            DateTime from,
            DateTime to)
        {
            if (from > to)
                throw new ArgumentException("Data de inceput este dupa cea de final.");

            // Incarcam zborurile din interval. tracked: false (read-only raport).
            // Includem Aircraft + FlightSeats pentru ca BuildOccupancyReport
            // are nevoie de ele si nu vrem N+1 queries.
            var flights = await _uow.FlightRepository.GetAllAsync(
                filter: f => f.DepartureTime >= from && f.DepartureTime <= to,
                tracked: false,
                f => f.Aircraft!,
                f => f.FlightSeats);

            // Materializam si construim rapoartele pe fiecare zbor.
            // OrderBy garanteaza o afisare predictibila in UI.
            return flights
                .Select(BuildOccupancyReport)
                .OrderBy(r => r.DepartureTime)
                .ToList();
        }

        // ─────────────────────────────────────────────────────────────────────
        //  GetRevenueAsync — REQ-44 raport financiar
        // ─────────────────────────────────────────────────────────────────────
        public async Task<IEnumerable<FinancialReportRow>> GetRevenueAsync(
            DateTime from,
            DateTime to)
        {
            if (from > to)
                throw new ArgumentException("Data de inceput este dupa cea de final.");

            // Luam platile din interval (repo-ul deja le ordoneaza descending
            // dupa data, dar reordering oricum la final dupa GroupBy).
            var payments = await _uow.PaymentRepository.GetByDateRangeAsync(from, to);

            // Filtram doar platile finalizate (nu incluse: Pending, Failed, Refunded).
            // GroupBy pe TransactionDate.Date — ignoram ora, vrem agregare zilnica.
            // Sum(Amount) cu decimal pastreaza precizia exacta.
            return payments
                .Where(p => p.Status == PaymentStatus.Completed)
                .GroupBy(p => p.TransactionDate.Date)
                .Select(g => new FinancialReportRow
                {
                    Date = g.Key,
                    TransactionsCount = g.Count(),
                    TotalRevenue = g.Sum(p => p.Amount)
                })
                .OrderBy(row => row.Date)
                .ToList();
        }

        // ─────────────────────────────────────────────────────────────────────
        //  Helper privat: construieste un OccupancyReport dintr-un Flight
        //  cu navigation properties incarcate.
        // ─────────────────────────────────────────────────────────────────────
        private static OccupancyReport BuildOccupancyReport(Flight flight)
        {
            // TotalSeats = capacitatea aeronavei (BR-1 foloseste aceeasi sursa).
            // SoldSeats = locurile cu IsAvailable = false.
            // OccupancyPercentage e calculata in clasa OccupancyReport.
            int totalSeats = flight.Aircraft?.MaxCapacity ?? 0;
            int soldSeats = flight.FlightSeats.Count(s => !s.IsAvailable);

            return new OccupancyReport
            {
                FlightId = flight.FlightId,
                AirlineName = flight.AirlineName,
                Source = flight.Source,
                Destination = flight.Destination,
                DepartureTime = flight.DepartureTime,
                TotalSeats = totalSeats,
                SoldSeats = soldSeats
            };
        }
    }
}

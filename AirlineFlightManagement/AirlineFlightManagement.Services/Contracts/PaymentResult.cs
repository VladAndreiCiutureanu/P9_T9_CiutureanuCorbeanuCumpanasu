namespace AirlineFlightManagement.Services.Contracts
{
    public class PaymentResult
    {
        public bool Success { get; set; }
        public string? TransactionId { get; set; }
        public string? ErrorMessage { get; set; }

        public static PaymentResult Ok(string transactionId) =>
            new() { Success = true, TransactionId = transactionId };

        public static PaymentResult Fail(string errorMessage) =>
            new() { Success = false, ErrorMessage = errorMessage };
    }
}

namespace AirlineFlightManagement.Services.Contracts
{
    public class AuthResult
    {
        public bool Success { get; set; }
        public string? UserId { get; set; }
        public IEnumerable<string> Errors { get; set; } = Enumerable.Empty<string>();

        public static AuthResult Ok(string userId) =>
            new() { Success = true, UserId = userId };

        public static AuthResult Fail(params string[] errors) =>
            new() { Success = false, Errors = errors };
    }
}

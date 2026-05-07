using System;
using System.ComponentModel.DataAnnotations;

namespace AirlineFlightManagement.Models.Models
{
    public class ApiLog
    {
        [Key]
        public int LogId { get; set; }

        public DateTime RequestTimestamp { get; set; }

        [MaxLength(50)]
        public string? ErrorCode { get; set; }

        [MaxLength(2000)]
        public string? ErrorMessage { get; set; }

        [MaxLength(2000)]
        public string? RequestParams { get; set; }
    }
}

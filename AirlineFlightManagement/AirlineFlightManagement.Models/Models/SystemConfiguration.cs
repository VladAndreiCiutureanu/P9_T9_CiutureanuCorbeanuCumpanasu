using System;
using System.ComponentModel.DataAnnotations;

namespace AirlineFlightManagement.Models.Models
{
    public class SystemConfiguration
    {
        [Key]
        public int ConfigurationId { get; set; }

        [Required]
        [MaxLength(100)]
        public string SettingKey { get; set; } = string.Empty;

        [Required]
        [MaxLength(2000)]
        public string SettingValue { get; set; } = string.Empty;

        public DateTime LastUpdated { get; set; }
    }
}

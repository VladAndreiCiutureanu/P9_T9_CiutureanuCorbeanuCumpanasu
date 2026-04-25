using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace AirlineFlightManagement.Models.Models
{
    public class SystemConfiguration
    {
        [Key]
        public int ConfigurationId { get; set; }

        public string SettingKey { get; set; }
        public string SettingValue { get; set; }
        public DateTime LastUpdated { get; set; }

        
    }
}

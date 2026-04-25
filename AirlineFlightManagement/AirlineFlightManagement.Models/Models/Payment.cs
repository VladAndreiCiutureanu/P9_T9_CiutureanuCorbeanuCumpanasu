using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace AirlineFlightManagement.Models.Models
{
    public class Payment
    {
        public int PaymentId { get; set; }

        // Foreign Key to Reservation
        public int ReservationId { get; set; }
        [ForeignKey("ReservationId")]
        public virtual Reservation Reservation { get; set; }

        public string TransactionId { get; set; }
        public string PaymentMethod {  get; set; }
        public DateTime TransactionDate { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }
        public string Status { get; set; }
    }
}

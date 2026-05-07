using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AirlineFlightManagement.Models.Enums;

namespace AirlineFlightManagement.Models.Models
{
    public class Payment
    {
        [Key]
        public int PaymentId { get; set; }

        public int ReservationId { get; set; }

        [ForeignKey(nameof(ReservationId))]
        public virtual Reservation? Reservation { get; set; }

        [Required]
        [MaxLength(64)]
        public string TransactionId { get; set; } = string.Empty;

        public PaymentMethod PaymentMethod { get; set; }

        public DateTime TransactionDate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Range(typeof(decimal), "0", "1000000")]
        public decimal Amount { get; set; }

        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    }
}

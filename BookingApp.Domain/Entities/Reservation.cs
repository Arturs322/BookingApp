using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookingApp.Domain.Entities
{
    public class Reservation
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string UserId { get; set; }
        [ForeignKey("UserId")]
        [ValidateNever]
        public ApplicationUser User { get; set; }
        [Required]
        public int VillaId { get; set; }
        [ForeignKey("VillaId")]
        [ValidateNever]
        public Villa Villa { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Email { get; set; }
        public string? Phone { get; set; }
        [Required]
        public double TotalCost { get; set; }
        public int Nights { get; set; }
        public string? Status { get; set; }
        [Required]
        public DateTime BookingDate { get; set; }
        [Required]
        public DateOnly CheckInDate { get; set; }
        [Required]
        public DateOnly CheckOutDate { get; set; }
        public bool IsPaymentSuccessful { get; set; } = false;
        public DateTime PaymentDate { get; set; }
        public string? StripeSessionId { get; set; }
        public string? StripePaymentIntentId { get; set; }
        public DateTime ActualCheckInDate { get; set; }
        public DateTime ActualCheckOutDate { get; set; }
        public int VillaNumber { get; set; }
    }
}

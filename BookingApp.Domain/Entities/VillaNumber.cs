using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookingApp.Domain.Entities
{
    public class VillaNumber
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        [DisplayName("Villa Number")]
        public int Villa_Number { get; set; }
        [DisplayName("Villa ID")]
        public int VillaId { get; set; }
        [ForeignKey("VillaId")]
        [ValidateNever]
        public Villa Villa { get; set; }
        [DisplayName("Special Details")]
        public string? SpecialDetails { get; set; }
    }
}

using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusBookingDemo.Entity
{
    public class Trip
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string? From { get; set; }
        [Required]
        public string? To { get; set; }
        [Required]
        public string? Depart { get; set; }
        [Required]
        public string? Return { get; set; }
        [Required]
        public string? Time { get; set; }
        [Required]
        public string? Guest { get; set; }
        
        public int BusId { get; set; }
        
        [ValidateNever]
        [ForeignKey("BusId")]
        public Bus Bus { get; set; }
        [Required]
        public double Price { get; set; }
    }
}

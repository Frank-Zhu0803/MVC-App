using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace AwesomeTickets.Models
{
    public class Category
    {
        [Key]
        public int CategoryId { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Title { get; set; }

        // Navigation property for relationship with Events
        [ValidateNever]
        public List<Event> Events { get; set; }
    }
} 
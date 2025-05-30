using System.ComponentModel.DataAnnotations;

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
        public List<Event> Events { get; set; }
    }
} 
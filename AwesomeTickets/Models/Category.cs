using System.ComponentModel.DataAnnotations;

namespace AwesomeTickets.Models
{
    public class Category
    {
        [Key]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "The category title is required.")]
        [StringLength(100, ErrorMessage = "The category title cannot exceed 100 characters.")]
        public string Title { get; set; }

        // Navigation Property
        public List<Event> Events { get; set; }
    }

}

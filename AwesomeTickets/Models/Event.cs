using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace AwesomeTickets.Models
{
    public class Event
    {
        [Key]
        public int EventId { get; set; }

        [Required(ErrorMessage = "The title is required.")]
        [StringLength(200, ErrorMessage = "The title cannot exceed 200 characters.")]
        public string Title { get; set; }

        [Required(ErrorMessage = "The description is required.")]
        [StringLength(1000, ErrorMessage = "The description cannot exceed 1000 characters.")]
        public string Description { get; set; }

        [ValidateNever]
        public string ImageFilename { get; set; } // Store only the filename

        [Required(ErrorMessage = "The event date and time are required.")]
        [DataType(DataType.DateTime)]
        public DateTime EventDateTime { get; set; }

        [Required(ErrorMessage = "The location is required.")]
        [StringLength(200, ErrorMessage = "The location cannot exceed 200 characters.")]
        public string Location { get; set; }

        [Required(ErrorMessage = "The price is required.")]
        [Range(0, double.MaxValue, ErrorMessage = "The price must be a non-negative number.")]
        [DataType(DataType.Currency)]
        public decimal Price { get; set; }

        [ValidateNever]
        public DateTime CreateDate { get; set; } = DateTime.Now; // Set default value

        // Foreign Key
        [Required(ErrorMessage = "The category is required.")]
        public int CategoryId { get; set; }

        // Navigation Property
        [ForeignKey("CategoryId")]
        [ValidateNever]
        public Category Category { get; set; }
    }
}

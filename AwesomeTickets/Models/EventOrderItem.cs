using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AwesomeTickets.Models
{
    public class EventOrderItem
    {
        [Key]
        public int OrderItemId { get; set; }

        [Required]
        public int OrderId { get; set; }

        [Required]
        public int EventId { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }

        [ForeignKey("OrderId")]
        public EventOrder Order { get; set; }

        [ForeignKey("EventId")]
        public Event Event { get; set; }

        [NotMapped]
        public decimal TotalPrice => Quantity * UnitPrice;
    }
} 
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AwesomeTickets.Models
{
    public class CartItem
    {
        [Key]
        public int CartItemId { get; set; }
        
        public int CartId { get; set; }
        
        [Required]
        public int EventId { get; set; }
        
        [Required]
        public int Quantity { get; set; }
        
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }
        
        [ForeignKey("EventId")]
        public Event Event { get; set; }
        
        [ForeignKey("CartId")]
        public Cart Cart { get; set; }
        
        [NotMapped]
        public decimal TotalPrice => Quantity * UnitPrice;
    }
} 
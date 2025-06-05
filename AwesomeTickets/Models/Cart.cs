using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AwesomeTickets.Models
{
    public class Cart
    {
        [Key]
        public int CartId { get; set; }
        
        [Required]
        public int UserId { get; set; }
        
        [ForeignKey("UserId")]
        public User User { get; set; }
        
        public ICollection<CartItem> Items { get; set; } = new List<CartItem>();
        
        [NotMapped]
        public decimal TotalPrice => Items.Sum(item => item.TotalPrice);
        
        [NotMapped]
        public int TotalQuantity => Items.Sum(item => item.Quantity);
    }
} 
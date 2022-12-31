using System.ComponentModel.DataAnnotations;

namespace LOi.Models
{
    public class AdminOrderCreation
    {
        public string Size { get; set; } //Available sizes are 200ml, 500ml, 1l, 2l 
        public DateTime UpdatedAt { get; set; } //change sstring to datetime, set to auto
        public string Location { get; set; }
        public int Quantity { get; set; }
        public string OrderStatus { get; set; } //intransit, pending, delivered
        public string Email { get; set; }
    }
}
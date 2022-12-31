using System.ComponentModel.DataAnnotations;

namespace LOi.Models
{
    public class OrderCreation
    {
        public string Size { get; set; } //Available sizes are 200ml, 500ml, 1l, 2l 
        public string Location { get; set; }
        public int Quantity { get; set; } 
    }
}
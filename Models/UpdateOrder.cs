using System.ComponentModel.DataAnnotations;

namespace LOi.Models
{
    public class UpdateOrder
    {
        [Required]
        public string Size { get; set; } //Available sizes are 200ml, 500ml, 1l, 2l 
        [Required]
        public string Location { get; set; }
        [Required]
        public int Quantity { get; set; } 
    }
}
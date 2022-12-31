// using System.ComponentModel.DataAnnotations;

using System;
using System.ComponentModel.DataAnnotations;

namespace LOi.Models
{
    public class Order
    {
        [Key]
        public Guid OrderID { get; set; }

        public User User { get; set; }

        [Required]
        public string Size { get; set; } //Available sizes are 200ml, 500ml, 1l, 2l 
        [Required]
        public DateTime CreationTime { get; set; } 
        [Required]
        public DateTime UpdatedAt { get; set; } //change sstring to datetime, set to auto
        [Required]
        public string Location { get; set; }
        [Required]
        public int Quantity { get; set; } 
        [Required]
        public string OrderStatus { get; set; } //intransit, pending, delivered

        [Required]
        public int Cost { get; set; } //intransit, pending, delivered



    }
}
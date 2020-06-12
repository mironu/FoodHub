using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FoodHub.Models
{
    public class Order
    {
        public int Id { get; set; }
        public DateTime OrderPlaced { get; set; }
        public string State { get; set; }
        public string Type { get; set; }
        public string Payment { get; set; }
        public int UserId { get; set; }
        public int RestaurantId { get; set; }
        public User User { get; set; }
        public Restaurant Restaurant { get; set; }
        public ICollection<ProductOrder> ProductOrders { get; set; }
    }
}

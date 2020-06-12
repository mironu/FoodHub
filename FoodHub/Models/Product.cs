using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FoodHub.Models
{
    public class Product
    {
        public int Id { get; set; }
        public int RestaurantId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Price { get; set; }
        [Newtonsoft.Json.JsonIgnore]
        public byte[] Image { get; set; }
        public virtual Restaurant Restaurant { get; set; }
        public ICollection<ProductOrder> ProductOrders { get; set; }
    }
}

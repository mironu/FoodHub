using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FoodHub.Models
{
    public class ProductOrder
    {
        public int Id { get; set; }
        public int Quantity { get; set; }
        public int ProductId { get; set; }
        public int OrderId { get; set; }
        public int Price { get; set; }
        public Order Order { get; set; }
        public Product Product { get; set; }
    }
}

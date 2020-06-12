using FoodHub.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FoodHub.ViewModels
{
    public class ProductViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Price { get; set; }
        public string Image { get; set; }


        public static implicit operator Product(ProductViewModel vm)
        {
            return new Product
            {
                Name = vm.Name,
                Description = vm.Description,
                Id = vm.Id,
                Price = vm.Price,
            };
        }

        public static implicit operator ProductViewModel(Product product)
        {
            return new ProductViewModel
            {
                Id = product.Id,
                Description = product.Description,
                Name = product.Name,
                Price = product.Price,
            };
        }
    }
}

using FoodHub.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace FoodHub.ViewModels
{
    public class RestaurantViewModel
    {
        public int Id { get; set; }
        [Required] 
        public string Name { get; set; }

        [Required]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.EmailAddress)]
        [Compare("Email", ErrorMessage = "Emails do not match.")]
        [DisplayName("Re-enter Email")]
        public string ReEmail { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        [DisplayName("Re-enter password")]
        public string RePassword { get; set; }

        [Required]
        [DisplayName("Phone Number")]
        public string PhoneNumber { get; set; }

        [Required] 
        public string Address { get; set; }

        public string InfoMessage { get; set; }

        public static implicit operator Restaurant(RestaurantViewModel vm)
        {
            return new Restaurant
            {
                Name = vm.Name,
                Address = vm.Address,
                Email = vm.Email,
                Id = vm.Id,
                Password = vm.Password,
                PhoneNumber = vm.PhoneNumber,
            };
        }

        public static implicit operator RestaurantViewModel(Restaurant restaurant)
        {
            return new RestaurantViewModel
            {
                Name = restaurant.Name,
                Address = restaurant.Address,
                Email = restaurant.Email,
                Id = restaurant.Id,
                Password = restaurant.Password,
                PhoneNumber = restaurant.PhoneNumber,
            };
        }

    }
}

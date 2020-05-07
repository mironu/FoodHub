using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FoodHub.Data;
using FoodHub.Models;
using FoodHub.ViewModels;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;

namespace FoodHub.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class RestaurantsController : ControllerBase
    {
        private readonly MyDbContext _context;

        public RestaurantsController(MyDbContext context)
        {
            _context = context;
        }

        // GET: api/Restaurants
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RestaurantApiModel>>> GetRestaurant()
        {
            var restaurant = await _context.Restaurant.ToListAsync();
            List<RestaurantApiModel> lst = new List<RestaurantApiModel>();
            foreach (var r in restaurant)
            {
                lst.Add(new RestaurantApiModel()
                {
                    Address = r.Address,
                    Email = r.Email,
                    Id = r.Id,
                    Name = r.Name,
                    PhoneNumber = r.PhoneNumber
                });
            }

            return lst;
        }

        // GET: api/Restaurants/5
        [HttpGet("{id}")]
        public async Task<ActionResult<RestaurantApiModel>> GetRestaurant(int id)
        {
            var restaurant = await _context.Restaurant.FindAsync(id);

            if (restaurant == null)
            {
                return NotFound();
            }

            return new RestaurantApiModel()
            {
                Address = restaurant.Address,
                Email = restaurant.Email,
                Id = restaurant.Id,
                Name = restaurant.Name,
                PhoneNumber = restaurant.PhoneNumber
            };
        }

    }
}

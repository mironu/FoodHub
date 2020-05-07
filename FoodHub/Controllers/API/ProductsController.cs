using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FoodHub.Data;
using FoodHub.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;

namespace FoodHub.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class ProductsController : ControllerBase
    {
        private readonly MyDbContext _context;

        public ProductsController(MyDbContext context)
        {
            _context = context;
        }

        // GET: api/Products/5
        [HttpGet("{id}")]
        public async Task<ActionResult<IEnumerable<Product>>> GetProduct(int id)
        {
            var product = await _context.Product.Where(p => p.RestaurantId == id).ToListAsync();

            if (product == null)
            {
                return NotFound();
            }

            return product;
        }

    }
}

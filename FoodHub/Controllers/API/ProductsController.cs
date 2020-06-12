using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using FoodHub.Data;
using Microsoft.Net.Http.Headers;

namespace FoodHub.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
   
    public class ProductsController : ControllerBase
    {
        private readonly MyDbContext _context;

        public ProductsController(MyDbContext context)
        {
            _context = context;
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetProductImage(int id)
        {
            var product = await _context.Product.FindAsync(id);
            if (product != null && product.Image != null)
                return new FileContentResult(product.Image, new MediaTypeHeaderValue("image/jpeg"));
            return NotFound();
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FoodHub.Data;
using FoodHub.Models;
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json.Linq;

namespace FoodHub.Controllers
{
    [Authorize]
    public class OrdersController : Controller
    {
        private readonly MyDbContext _context;

        public OrdersController(MyDbContext context)
        {
            _context = context;
        }

        // GET: Orders
        public async Task<IActionResult> Index()
        {
            string id = HttpContext.User.Claims
                .FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;

            var orders = _context.Order.Include(o => o.User)
                .Where(o => o.RestaurantId == Int32.Parse(id) && (o.State.Equals("Finished") || o.State.Equals("Declined")));
            return View(await orders.ToListAsync());
        }

        // GET: Orders/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            string restaurantId = HttpContext.User.Claims
                .FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
            if (id == null)
            {
                return NotFound();
            }
            var order = await _context.Order
                .Include(o => o.User)
                .Include(o => o.ProductOrders)
                .ThenInclude(o => o.Product)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (order == null)
            {
                return NotFound();
            }
            else if (order.RestaurantId == Int32.Parse(restaurantId))
            {
                return View(order);
            }

            return Unauthorized();
        }

        public async Task<IActionResult> ActiveOrders()
        {
            string id = HttpContext.User.Claims
                .FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;

            var orders = _context.Order.Include(o => o.User)
                .Include(o => o.ProductOrders)
                .ThenInclude(o => o.Product)
                .Where(o => o.RestaurantId == Int32.Parse(id) &&
                            !(o.State.Equals("Finished") || o.State.Equals("Declined")));

            return View(await orders.ToListAsync());
        }



        [HttpPost]
        public IActionResult UpdateOrder([FromBody] JObject data)
        {
            Order o = new Order
            {
                Id = Int32.Parse(data.GetValue("orderId").ToString()),
                State = data.GetValue("state").ToString()
            };
            _context.Order.Attach(o).Property(x => x.State).IsModified = true;
            _context.SaveChanges();

            return new EmptyResult();
        }


        private bool OrderExists(int id)
        {
            return _context.Order.Any(e => e.Id == id);
        }
    }
}

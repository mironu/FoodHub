using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FoodHub.Data;
using FoodHub.Models;
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.SignalR;
using FoodHub.Hubs;

namespace FoodHub.Controllers
{
    [Authorize]
    public class OrdersController : Controller
    {
        private readonly MyDbContext _context;
        private readonly IHubContext<AndroidHub> _hub;
        public OrdersController(MyDbContext context, IHubContext<AndroidHub> hub)
        {
            _context = context;
            _hub = hub;
        }

        // GET: All finished orders
        public async Task<IActionResult> Index()
        {
            string id = HttpContext.User.Claims
                .FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
            int restId = 0;
            try
            {
                restId = Int32.Parse(id);
            }
            catch(Exception e)
            {
                return NotFound(e.Message);
            }
            var orders = await _context.Order.Include(o => o.User)
                .Where(o => o.RestaurantId == restId && (o.State.Equals("Finished") || o.State.Equals("Declined")))
                .ToListAsync();

            return View(orders);
        }

        // GET: Orders/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            string restaurantId = HttpContext.User.Claims
                .FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
            int restId = 0;

            try
            {
                restId = Int32.Parse(restaurantId);
            }
            catch (Exception e)
            {
                return NotFound(e.Message);
            }
            var order = await _context.Order
                .Include(o => o.User)
                .Include(o => o.ProductOrders)
                .ThenInclude(o => o.Product)
                .Where(o => o.Id == id && o.RestaurantId == restId)
                .FirstOrDefaultAsync();
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }
        //GET all active orders
        public async Task<IActionResult> ActiveOrders()
        {
            string id = HttpContext.User.Claims
                .FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
            int restId = 0;
            try
            {
                restId = Int32.Parse(id);
            }
            catch (Exception e)
            {
                return NotFound(e.Message);
            }
            var orders = await _context.Order.Include(o => o.User)
                .Include(o => o.ProductOrders)
                .ThenInclude(o => o.Product)
                .Where(o => o.RestaurantId == restId &&
                            !(o.State.Equals("Finished") || o.State.Equals("Declined")))
                .ToListAsync();

            return View(orders);
        }

        // Update state of the order and send it to the client
        [HttpPost]
        public  IActionResult UpdateOrder([FromBody] JObject data)
        {
            if(data["orderId"] == null || data["state"] == null)
            {
                return Problem("orderId or state not set");
            }

            Order o = new Order
            {
                Id = Int32.Parse(data.GetValue("orderId").ToString()),
                State = data.GetValue("state").ToString()
            };
            _context.Order.Attach(o).Property(x => x.State).IsModified = true;
            _context.SaveChanges();


           _hub.Clients.User(data.GetValue("email").ToString())
                .SendAsync("UpdateOrder", data.GetValue("state").ToString(), data.GetValue("orderId").ToString());
            
         
            return new EmptyResult();
        }

    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using FoodHub.Data;
using FoodHub.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using FoodHub.Hubs;

namespace FoodHub.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class OrdersController : ControllerBase
    {
        private readonly MyDbContext _context;
        private readonly IHubContext<OrderHub> _hub;
        public OrdersController(MyDbContext context, IHubContext<OrderHub> hub)
        {
            _context = context;
            _hub = hub;
        }

        [HttpPost]
        public IActionResult PostOrder([FromBody] JObject data)
        {
            string email = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = _context.User.FirstOrDefault(u => u.Email.Equals(email));

            Order receivedOrder = new Order()
            {
                OrderPlaced = DateTime.Now,
                RestaurantId = Int32.Parse(data["RestaurantId"].ToString()),
                Payment = data["Payment"].ToString(),
                State = "Initiated",
                Type = data["Type"].ToString(),
                UserId = user.Id
            };
            _context.Order.Add(receivedOrder);
            _context.SaveChanges();

            var products = JsonConvert.DeserializeObject<List<ProductOrder>>(data["ProductOrders"].ToString());
            products.ForEach(p => p.OrderId = receivedOrder.Id);
            _context.ProductOrder.AddRange(products);
            _context.SaveChanges();

            var order = _context.Order
               .Include(u => u.User)
               .Include(o => o.ProductOrders)
               .ThenInclude(o => o.Product)
               .Where(u => u.Id == receivedOrder.Id)
               .Select(u => new
               {
                   u.User.Name,
                   u.User.Email,
                   u.User.PhoneNumber,
                   ProductName = u.ProductOrders.Select(p => p.Product.Name),
                   ProductPrice = u.ProductOrders.Select(p => p.Price),
                   ProductQuantity = u.ProductOrders.Select(p => p.Quantity),
                   u.OrderPlaced,
                   u.State,
                   u.Type,
                   u.Payment,
                   u.Id,
               })
               .FirstOrDefault();

           
             _hub.Clients.User(receivedOrder.RestaurantId.ToString())
                .SendAsync("ReceiveMessage",
                JsonConvert.SerializeObject(order, Formatting.None,
                        new JsonSerializerSettings()
                        {
                            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                        }));
                
     
            return Ok("success");
        }


        [HttpGet]
        public async Task<ActionResult<IEnumerable>> GetOrders()
        {

            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var email = identity.Claims.FirstOrDefault().Value;
            var user = await _context.User.AsNoTracking().Where(u => u.Email.Equals(email)).FirstOrDefaultAsync();
            var orders = await _context.Order
                .Include(o => o.ProductOrders)
                .Where(o => o.UserId == user.Id)
                .Select(o => new
                {
                    o.Id,
                    o.OrderPlaced,
                    o.State,
                    o.Type,
                    o.Payment,
                    o.Restaurant.Name,
                    ProductOrders = o.ProductOrders.Select(p => new
                    {
                        p.Price,
                        p.Quantity,
                        p.Product.Name
                    })
                })
                .ToListAsync();
            string response = JsonConvert.SerializeObject(orders, Formatting.None,
                        new JsonSerializerSettings()
                        {
                            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                        });
            return response;

        }

        [HttpGet("{id}")]
        public async Task<ActionResult<IEnumerable>> GetOrder(int id)
        {

            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var email = identity.Claims.FirstOrDefault().Value;
            var user = await _context.User.AsNoTracking().Where(u => u.Email.Equals(email)).FirstOrDefaultAsync();
            var orders = await _context.Order
                .Include(o => o.ProductOrders)
                .Where(o => o.Id == id)
                .Select(o => new
                {
                    o.Id,
                    o.OrderPlaced,
                    o.State,
                    o.Type,
                    o.Payment,
                    o.Restaurant.Name,
                    ProductOrders = o.ProductOrders.Select(p => new
                    {
                        p.Price,
                        p.Quantity,
                        p.Product.Name
                    })
                }).FirstOrDefaultAsync();

            string response = JsonConvert.SerializeObject(orders, Formatting.None,
                        new JsonSerializerSettings()
                        {
                            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                        });
            return response;

        }
    }
}

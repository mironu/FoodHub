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
                State = "Initiated",
                Type = data["Type"].ToString(),
                UserId = user.Id
            };
            _context.Order.Add(receivedOrder);
            _context.SaveChanges();

            var x = JsonConvert.DeserializeObject<List<ProductOrder>>(data["ProductOrders"].ToString());
            x.ForEach(p => p.OrderId = receivedOrder.Id);
            _context.ProductOrder.AddRange(x);
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
               })
               .FirstOrDefault();
            // .FirstOrDefaultAsync(m => m.Id == receivedOrder.Id);
            // _hub.Clients.User(receivedOrder.RestaurantId.ToString()).
            _hub.Clients.User(receivedOrder.RestaurantId.ToString())
                .SendAsync("ReceiveMessage", JsonConvert.SerializeObject(order, Formatting.None,
                        new JsonSerializerSettings()
                        {
                            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                        }));
            return Ok();
        }


        [HttpGet]
        public async Task<ActionResult<IEnumerable>> GetOrder(int id)
        {
            var orders = await _context.Order.Where(o => o.UserId == id).ToListAsync();

            return orders;

        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using FoodHub.Data;
using FoodHub.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Stripe;

namespace FoodHub.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class PaymentController : ControllerBase
    {
        private readonly MyDbContext _context;
        public PaymentController(MyDbContext context)
        {
            _context = context;
            StripeConfiguration.ApiKey = "sk_test_pYh0rfomNeq3KvqXyIaDCGWO00XMbINsnF";
        }
        [HttpPost]
        public IActionResult createEphemeralKey([FromBody] JObject data)
        {

            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var email = identity.Claims.FirstOrDefault().Value;
            var user = _context.User.FirstOrDefault(u => u.Email.Equals(email));

            var options = new EphemeralKeyCreateOptions
            {
                Customer = user.StripeId,
                StripeVersion = data["api_version"].ToString(),
            };
            var service = new EphemeralKeyService();
            var key = service.Create(options);
            
            return Ok(key.RawJson);
        }
        [HttpPost("createPaymentIntent")]
        public IActionResult createPaymentIntent([FromBody] JObject data)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var email = identity.Claims.FirstOrDefault().Value;
            var user = _context.User.FirstOrDefault(u => u.Email.Equals(email));

            var products = JsonConvert.DeserializeObject<List<ProductOrder>>(data["products"].ToString());
            int total = 0;
            foreach(ProductOrder p in products)
            {
                total += p.Quantity * p.Price;
            }
            var options = new PaymentIntentCreateOptions
            {
                Amount = total * 100,
                Currency = "ron",
                Customer = user.StripeId,
            };

            var service = new PaymentIntentService();
            var paymentIntent = service.Create(options);
            return Ok(paymentIntent.ClientSecret);
        }
    }
}
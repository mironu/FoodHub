﻿using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FoodHub.Data;
using FoodHub.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Stripe;
using FoodHub.Utils;

namespace FoodHub.Controllers.API
{

    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly MyDbContext _context;
        private readonly IConfiguration _configuration;

        public UsersController(MyDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        // GET: api/Users/5
        [HttpGet("getCurrentUser")]
        public async Task<ActionResult<User>> GetUser()
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var email = identity.Claims.FirstOrDefault().Value;
            var user = await _context.User.Where(u => u.Email.Equals(email)).FirstOrDefaultAsync();

            if (user == null)
            {
                return NotFound();
            }

            return user;
        }


        // POST: api/Users
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        [HttpPost]
        public async Task<ActionResult<User>> PostUser(User user)
        {
            if (ModelState.IsValid)
            {
                if (_context.User.Any(r => r.Email.Equals(user.Email)))
                {
                    return Ok("Email already in use");
                }
                else
                {
                    StripeConfiguration.ApiKey = "sk_test_pYh0rfomNeq3KvqXyIaDCGWO00XMbINsnF";
                    var options1 = new CustomerCreateOptions
                    {
                        Description = "My First Test Customer (created for API docs)",
                    };
                    var service = new CustomerService();
                    Customer c = service.Create(options1);

                    user.StripeId = c.Id;
                    string salt = Crypto.generateSalt();
                    string hasedPassword = Crypto.computeHash(user.Password, salt);
                    user.Salt = salt;
                    user.Password = hasedPassword;
                    _context.User.Add(user);
                    await _context.SaveChangesAsync();
                }
            }
            else
            {
                return BadRequest(ModelState);
            }

            return Ok("success");
        }

        [HttpPost("login")]
        public  IActionResult Login([FromBody]JObject data)
        {
            if (data["EmailAddress"] == null || data["Password"] == null)
            {
                return Ok("Email or password cannot be null");
            }
            if (_context.User.Any(r => r.Email.Equals(data["EmailAddress"].ToString())))
            {
                User user = _context.User.FirstOrDefault(u => u.Email.Equals(data["EmailAddress"].ToString()));
                string salt = user.Salt;
                string computedHash = Crypto.computeHash(data["Password"].ToString(), salt);

                if (computedHash.Equals(user.Password))
                {
                    var claims = new[]
                    {
                        new Claim(JwtRegisteredClaimNames.Sub, data["EmailAddress"].ToString()),
                    };

                    var token = new JwtSecurityToken
                    (
                        issuer: _configuration["Jwt:Issuer"],
                        audience: _configuration["Jwt:Audience"],
                        claims: claims,
                        expires: DateTime.UtcNow.AddDays(60),
                        notBefore: DateTime.UtcNow,
                        signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:SigningKey"])),
                            SecurityAlgorithms.HmacSha256)
                    );
                    return Ok(new JwtSecurityTokenHandler().WriteToken(token));
                }
                else
                {
                    return Ok("Email or password is wrong");
                }
            }
            return Ok("Email or password is wrong");
            
        }
 
    }
}

using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
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
            if (_context.User.Any(r => r.Email.Equals(data["EmailAddress"].ToString())
                                       && r.Password.Equals(data["Password"].ToString())))
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
                User user = _context.User.Where(u => u.Email.Equals(data["EmailAddress"].ToString())).FirstOrDefault();
                return Ok(new JwtSecurityTokenHandler().WriteToken(token));
                
                //, token = new JwtSecurityTokenHandler().WriteToken(token) });
            }
            return BadRequest("Email or password is wrong");
            
        }
 
   

        private bool UserExists(int id)
        {
            return _context.User.Any(e => e.Id == id);
        }
    }
}

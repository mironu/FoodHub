using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using FoodHub.Data;
using Microsoft.AspNetCore.Mvc;
using FoodHub.Models;
using FoodHub.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using FoodHub.Utils;

namespace FoodHub.Controllers
{
    public class RestaurantController : Controller
    {
        private readonly MyDbContext _context;

        public RestaurantController(MyDbContext context)
        {
            _context = context;
        }
        //GET
        public IActionResult Index()
        {
            return View();
        }
        //GET
        public IActionResult Privacy()
        {
            return View();
        }
        //GET
        public IActionResult Register()
        {
            return View();
        }
        //GET
        public IActionResult Login()
        {
            return View();
        }
        // GET
        public IActionResult Profile()
        {
            string restaurantId = HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
            var restaurant = _context.Restaurant.FirstOrDefault(r => r.Id == Int32.Parse(restaurantId));
            RestaurantViewModel restVm = restaurant;
            return View(restVm);
        }
        // GET
        public IActionResult EditProfile()
        {
            string restaurantId = HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
            var restaurant = _context.Restaurant.FirstOrDefault(r => r.Id == Int32.Parse(restaurantId));
            RestaurantViewModel restVm = restaurant;
            return View(restVm);
        }

        [HttpPost]
        public async Task<IActionResult> Register(RestaurantViewModel model)
        {
 
            if (ModelState.IsValid)
            {
                if (_context.Restaurant.Any(r => r.Email.Equals(model.Email)))
                {
                    model.InfoMessage = "Email " + model.Email + " already in use.";
                }
                else
                {
                    Restaurant dbRestaurant = model;
                    string salt = Crypto.generateSalt();
                    string hasedPassword = Crypto.computeHash(model.Password, salt);
                    dbRestaurant.Salt = salt;
                    dbRestaurant.Password = hasedPassword;

                    _context.Add(dbRestaurant);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }

            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Login(RestaurantViewModel model)
        {
            //validate user credentials against db
            if(_context.Restaurant.Any(r => r.Email.Equals(model.Email))){
                Restaurant currentRestaurant = _context.Restaurant.FirstOrDefault(p => p.Email == model.Email);
                string salt = currentRestaurant.Salt;
                string computedHash = Crypto.computeHash(model.Password, salt);

                if (computedHash.Equals(currentRestaurant.Password))
                {
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, currentRestaurant.Id.ToString()),
                        new Claim(ClaimTypes.Name,"Restaurant"),
                        new Claim(ClaimTypes.Email, model.Email)
                    };
                    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var principal = new ClaimsPrincipal(identity);
                    await HttpContext.SignInAsync(principal);

                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    model.InfoMessage = "Wrong Password";
                }

            }
            else
            {
                model.InfoMessage = "Email does not exist";
            }

            return View(model);
        }
  
   
        [HttpPost]
        public async Task<IActionResult> EditProfile(RestaurantViewModel model)
        {
            var dbRestaurant = _context.Restaurant.FindAsync(model.Id);
            Restaurant restaurant = model;
            restaurant.Password = dbRestaurant.Result.Password;
            _context.Entry(dbRestaurant.Result).State = EntityState.Detached;
            _context.Restaurant.Update(restaurant);
            await _context.SaveChangesAsync();
                
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(Index));
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

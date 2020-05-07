using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using FoodHub.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using FoodHub.Models;
using FoodHub.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Server.HttpSys;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FoodHub.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly MyDbContext _context;

        public HomeController(ILogger<HomeController> logger, MyDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Register()
        {
            return View();
        }

        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Register(RestaurantViewModel model)
        {
            Restaurant dbRestaurant = RestaurantViewModelToRestaurant(model);
            if (ModelState.IsValid)
            {
                //email already used
                if (_context.Restaurant.Any(r => r.Email.Equals(model.Email)))
                {
                    model.InfoMessage = "Email " + model.Email + " already in use.";
                }
                else
                {
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
            if (_context.Restaurant.Any(r => r.Email.Equals(model.Email)
                                             && r.Password.Equals(model.Password)))
            {
                Restaurant CurrentRestaurant = _context.Restaurant.FirstOrDefault(p => p.Email == model.Email);

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, CurrentRestaurant.Id.ToString()),
                    new Claim(ClaimTypes.Name,"Restaurant"),
                    new Claim(ClaimTypes.Email, model.Email),

                };
                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);
                await HttpContext.SignInAsync(principal);

                return RedirectToAction(nameof(Index));
            }
            else
            {
                model.InfoMessage = "Email or password is wrong.";
            }

            return View(model);
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(Index));
        }



        public IActionResult Profile()
        {
            string restaurantId = HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
            var restaurant = _context.Restaurant.FirstOrDefault(r => r.Id == Int32.Parse(restaurantId));
            return View(RestaurantToResaurantViewModel(restaurant));
        }
        public IActionResult EditProfile()
        {
            string restaurantId = HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
            var restaurant = _context.Restaurant.FirstOrDefault(r => r.Id == Int32.Parse(restaurantId));
            return View(RestaurantToResaurantViewModel(restaurant));
        }
        [HttpPost]
        public async Task<IActionResult> EditProfile(RestaurantViewModel model)
        {
           var dbRestaurant = _context.Restaurant.FindAsync(model.Id);
                Restaurant restaurant = RestaurantViewModelToRestaurant(model);
                restaurant.Password = dbRestaurant.Result.Password;
                _context.Entry(dbRestaurant.Result).State = EntityState.Detached;
                _context.Restaurant.Update(restaurant);
                await _context.SaveChangesAsync();
                
                return RedirectToAction(nameof(Index));
            
            //return View(model);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public Restaurant RestaurantViewModelToRestaurant(RestaurantViewModel restaurant)
        {
            return new Restaurant()
            {
                Address = restaurant.Address,
                Email = restaurant.Email,
                Id = restaurant.Id,
                Name = restaurant.Name,
                Password = restaurant.Password,
                PhoneNumber = restaurant.PhoneNumber,
            };
        }

        public RestaurantViewModel RestaurantToResaurantViewModel(Restaurant restaurant)
        {
            return new RestaurantViewModel()
            {
                Address = restaurant.Address,
                Email = restaurant.Email,
                Id = restaurant.Id,
                Name = restaurant.Name,
                Password = restaurant.Password,
                PhoneNumber = restaurant.PhoneNumber,
            };
        }
    }
}

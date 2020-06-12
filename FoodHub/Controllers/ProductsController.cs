using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FoodHub.Data;
using FoodHub.Models;
using FoodHub.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Net.Http.Headers;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace FoodHub.Controllers
{
    [Authorize]
    
    public class ProductsController : Controller
    {
        private readonly MyDbContext _context;

        public ProductsController(MyDbContext context)
        {
            _context = context;
        }

        // GET:/Products - All products
        public async Task<IActionResult> Index()
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
            var products = await _context.Product.
                    Where(p => p.RestaurantId == restId).ToListAsync();
            List<ProductViewModel> productViewModels = new List<ProductViewModel>();

            foreach (var p in products)
            {
                productViewModels.Add(p);

            }

            return View(productViewModels);
        }

        // GET: Products/Details/5
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

            var product = await _context.Product
                .Where(p => p.Id == id && p.RestaurantId == restId)
                .FirstOrDefaultAsync();
            if (product == null)
            {
                return NotFound();
            }
 
            ProductViewModel productViewModel = product;
            return View(productViewModel);

        }

        // GET: Products/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Products/
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductViewModel productViewModel)
        {
            if (ModelState.IsValid)
            {
                Product product = productViewModel;
                product.Image = GetImageBytes().Result;
                string restaurantId = HttpContext.User.Claims
                               .FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
                product.RestaurantId = Int32.Parse(restaurantId);
                _context.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(productViewModel);
        }

        // GET: Products/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            string restaurantId = HttpContext.User.Claims
                               .FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
            var product = await _context.Product
                .Where(p => p.Id == id && p.RestaurantId == Int32.Parse(restaurantId))
                .FirstOrDefaultAsync();
            if (product == null)
            {
                return NotFound();
            }

            ProductViewModel productViewModel = product;
            return View(productViewModel);
        }

        // POST: Products/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProductViewModel productViewModel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    Product product = productViewModel;
                    string restaurantId = HttpContext.User.Claims
                                          .FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
                    product.Image = GetImageBytes().Result;
                    product.RestaurantId = Int32.Parse(restaurantId);
                    if (product.Image == null)
                    {
                        var p = await _context.Product.FindAsync(product.Id);
                        product.Image = p.Image;
                        _context.Entry(p).State = EntityState.Detached;
                    }
                    _context.Product.Update(product);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductViewModelExists(productViewModel.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(productViewModel);
        }

        // GET: ProductViewModels/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            string restaurantId = HttpContext.User.Claims
                   .FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
            var product = await _context.Product
                    .Where(p => p.Id == id && p.RestaurantId == Int32.Parse(restaurantId))
                    .FirstOrDefaultAsync();
            if (product == null)
            {
                return NotFound();
            }

            ProductViewModel productViewModel = product;
            return View(productViewModel);
        }

        // POST: ProductViewModels/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            string restaurantId = HttpContext.User.Claims
                   .FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
            var product = await _context.Product
                    .Where(p => p.Id == id && p.RestaurantId == Int32.Parse(restaurantId))
                    .FirstOrDefaultAsync();
            if (product == null)
            {
                return NotFound();
            };
            _context.Product.Remove(product);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> GetImage(int id)
        {
            var product = await _context.Product.FindAsync(id);
            if(product.Image != null)
                return new FileContentResult(product.Image, new MediaTypeHeaderValue("image/jpeg"));
            return NotFound();
        }

        public async Task<byte[]> GetImageBytes()
         {
            var files = HttpContext.Request.Form.Files;
            if (files.Count != 0)
            {
                await using var memoryStream = new MemoryStream();
                await files.FirstOrDefault().CopyToAsync(memoryStream);
                using Image<Rgba32> image = Image.Load(memoryStream.ToArray());
                image.Mutate(x => x.Resize(480, 240));
                await using var saveMemoryStream = new MemoryStream();
                image.Save(saveMemoryStream, new JpegEncoder());
                return saveMemoryStream.ToArray();
            }
            return null;
        }
        private bool ProductViewModelExists(int id)
        {
            return _context.Product.Any(e => e.Id == id);
        }

    }

}

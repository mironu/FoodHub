using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FoodHub.Data;
using FoodHub.Models;
using FoodHub.ViewModels;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Net.Http.Headers;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
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

        // GET: ProductViewModels
        public IActionResult Index()
        {
            string id = HttpContext.User.Claims
                    .FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
            var products = _context.Product.
                    Where(p => p.RestaurantId == Int32.Parse(id));
            List<ProductViewModel> productViewModels = new List<ProductViewModel>();

            foreach (var p in products)
            {
                productViewModels.Add(ProductToProductViewModel(p));

            }

            return View(productViewModels);
        }
        // GET: ProductViewModels/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            string restaurantId = HttpContext.User.Claims
                .FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Product
                .FirstOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }
            else if (product.RestaurantId == Int32.Parse(restaurantId))
            {
                ProductViewModel productViewModel = ProductToProductViewModel(product);
                return View(productViewModel);
            }

            return Unauthorized();

        }

        // GET: ProductViewModels/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: ProductViewModels/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Description,Price, Image")] ProductViewModel productViewModel)
        {
            if (ModelState.IsValid)
            {
                Product product = ProductViewModelToProduct(productViewModel);
                product.Image = GetImageBytes().Result;

                _context.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(productViewModel);
        }

        // GET: ProductViewModels/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Product.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            ProductViewModel productViewModel = ProductToProductViewModel(product);
            return View(productViewModel);
        }

        // POST: ProductViewModels/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,Price,Image")] ProductViewModel productViewModel)
        {
            if (id != productViewModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    Product product = ProductViewModelToProduct(productViewModel);
                    product.Image = GetImageBytes().Result;
                    if (product.Image == null)
                    {
                        var p = await _context.Product.FindAsync(id);
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

            var product = await _context.Product
                .FirstOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            ProductViewModel productViewModel = ProductToProductViewModel(product);
            return View(productViewModel);
        }

        // POST: ProductViewModels/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Product.FindAsync(id);
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
                await using(var memoryStream = new MemoryStream())
                {
                    await files.FirstOrDefault().CopyToAsync(memoryStream);
                    using (Image<Rgba32> image = Image.Load(memoryStream.ToArray()))
                    {
                        image.Mutate(x => x
                            .Resize(480, 240));
                        await using (var saveMemoryStream = new MemoryStream())
                        {
                            image.Save(saveMemoryStream, new JpegEncoder());
                            return saveMemoryStream.ToArray();
                        }
                    }
                }
            }
            return null;
        }
        private bool ProductViewModelExists(int id)
        {
            return _context.Product.Any(e => e.Id == id);
        }

        Product ProductViewModelToProduct(ProductViewModel p)
        {
            string restaurantId = HttpContext.User.Claims
                                .FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
            return new Product()
            { 
                Name = p.Name,
                Description = p.Description,
                Id = p.Id,
                Price = p.Price,
                RestaurantId = Int32.Parse(restaurantId),
            };
        }

        ProductViewModel ProductToProductViewModel(Product p)
        {
            return new ProductViewModel()
            {
                Id = p.Id,
                Description = p.Description,
                Name = p.Name,
                Price = p.Price,
            };
        }
    }

   
}

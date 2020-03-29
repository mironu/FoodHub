using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using FoodHub.ViewModels;

namespace FoodHub.Data
{
    public class MyDbContext : DbContext
    {
        public MyDbContext(DbContextOptions<MyDbContext> options) :
            base(options)
        {

        }
        public DbSet<FoodHub.Models.Restaurant> Restaurant { get; set; }
        public DbSet<FoodHub.Models.Product> Product { get; set; }
    }
}
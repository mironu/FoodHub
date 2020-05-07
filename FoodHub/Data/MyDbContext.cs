using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using FoodHub.ViewModels;
using FoodHub.Models;

namespace FoodHub.Data
{
    public class MyDbContext : DbContext
    {
        public MyDbContext(DbContextOptions<MyDbContext> options) :
            base(options)
        {

        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //Write Fluent API configurations here
            modelBuilder.Entity<ProductOrder>()
                .HasOne(p => p.Order)
                .WithMany(p => p.ProductOrders )
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<ProductOrder>()
                .HasOne(p => p.Product)
                .WithMany(p => p.ProductOrders)
                .OnDelete(DeleteBehavior.NoAction);

            base.OnModelCreating(modelBuilder);
        }

        public DbSet<FoodHub.Models.Restaurant> Restaurant { get; set; }
        public DbSet<FoodHub.Models.Product> Product { get; set; }
        public DbSet<FoodHub.Models.User> User { get; set; }
        public DbSet<FoodHub.Models.Order> Order { get; set; }
        public DbSet<FoodHub.Models.ProductOrder> ProductOrder { get; set; }
    }
}
using Microsoft.EntityFrameworkCore;
using ELT3.Models;

namespace ELT3.Data;

public class AppDbContext : DbContext 
{
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<StockQuote> Quotes { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            
            modelBuilder.Entity<StockQuote>().ToTable("Quotes", schema: "public");
        }
}
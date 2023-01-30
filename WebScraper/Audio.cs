using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebScraper
{
    internal class AudioContext : DbContext
    {
        protected override void OnConfiguring(
            DbContextOptionsBuilder optionsBuilder)
        {
            string connection = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=Audio;Integrated Security=True";
            optionsBuilder.UseSqlServer(connection);
        }

        public DbSet<Company> Companies { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //configures one-to - many relationship
            modelBuilder.Entity<Product>( b =>
            {
                b.HasOne(e => e.Category)
                .WithMany(e => e.Products)
                .HasForeignKey(e => e.CategoryId);

                b.Property(b => b.Properties)
                .HasConversion(
                    v => JsonConvert.SerializeObject(v),
                    v => JsonConvert.DeserializeObject<Dictionary<string, string>>(v));
            });
        }
    }
}


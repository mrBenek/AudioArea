using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Packt.Shared;

namespace WebScraper
{
    internal class AudioContext : DbContext
    {
        protected override void OnConfiguring(
            DbContextOptionsBuilder optionsBuilder)
        {
            string connection = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=Audio;Integrated Security=True;TrustServerCertificate=True";
            optionsBuilder.UseSqlServer(connection);
        }

        public DbSet<Company> Companies { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Product>( b =>
            {
                //configures one-to-many relationship
                b.HasOne(e => e.Category)
                .WithMany(e => e.Products)
                .HasForeignKey(e => e.CategoryId);

                //configures dictionary as json
                b.Property(b => b.Properties)
                .HasConversion(
                    v => JsonConvert.SerializeObject(v),
                    v => JsonConvert.DeserializeObject<Dictionary<string, string>>(v));
            });
        }
    }
}


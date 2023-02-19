using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Packt.Shared;

public class AudioContext : DbContext
{
	public AudioContext()
	{
	}

	public AudioContext(DbContextOptions<AudioContext> options)
		: base(options)
	{
	}

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
		modelBuilder.Entity<Product>(b =>
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

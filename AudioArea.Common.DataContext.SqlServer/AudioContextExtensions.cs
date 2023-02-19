using Microsoft.EntityFrameworkCore; // UseSqlServer
using Microsoft.Extensions.DependencyInjection; // IServiceCollection

namespace Packt.Shared;

public static class AudioContextExtensions
{
	/// <summary>
	/// Adds AudioContext to the specified IServiceCollection. Uses the SqlServer database provider.
	/// </summary>
	/// <param name="services"></param>
	/// <param name="connectionString">Set to override the default.</param>
	/// <returns>An IServiceCollection that can be used to add more services.</returns>
	public static IServiceCollection AddAudioContext(
	  this IServiceCollection services,
	  string connectionString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=Audio;Integrated Security=True")
	{
		services.AddDbContext<AudioContext>(options =>
		{
			options.UseSqlServer(connectionString);

			options.LogTo(WriteLine, // Console
			  new[] { Microsoft.EntityFrameworkCore
		  .Diagnostics.RelationalEventId.CommandExecuting });
		});

		return services;
	}
}

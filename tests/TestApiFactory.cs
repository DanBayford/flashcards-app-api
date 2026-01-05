using Flashcards.Api;
using Flashcards.Api.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;


namespace tests;

/// <summary>
/// This class replaces the normal Web API database (SQLite, PostGres etc) with an in memory DB for the purposes of the test suite
/// It spins up an instance of your Web API but doesn't esxpose to network etc
/// I does mean all your middleware and service can run as normal, though
/// </summary>
public class TestApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove real API database from services
            var descriptor = services.Single(d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
            services.Remove(descriptor);
            
            // Add in memory database for duration of tests
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseInMemoryDatabase(Guid.NewGuid().ToString()));
        });
        
    }
}
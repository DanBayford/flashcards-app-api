using Data.Reset;
using Flashcards.Api.Common.Auth;
using Flashcards.Api.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

public class Program
{
    public static async Task Main(string[] args)
    {
        /*
         * Create an instance of the web app inc:
         *  - config
         *  - DI container + other services
         */
        var host = Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((context, config) =>
            {
                config.AddJsonFile("appsettings.json", optional: true);
                config.AddEnvironmentVariables();
            })
            .ConfigureServices((context, services) =>
            {
                // Add DBContext
                var basePath = AppContext.BaseDirectory;
                var dbPath = Path.Combine(basePath, "..", "..", "..", "..", "Flashcards.Api", "Persistence", "Flashcards.db");
                var connectionString = $"Data Source={dbPath}";

                services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseSqlite(connectionString));
                
                // Add PasswordService
                services.AddSingleton<IPasswordService, PasswordService>();
                
                // Add ResetTestUserService
                services.AddScoped<ResetTestUserService>();
                
            })
            .Build();

        // Create a scope (ie a context for the job to run in, similar to an HTTP request lifecycle)
        using var scope = host.Services.CreateScope();

        // Get required class (ResetTestUserService) from DI container
        var job = scope.ServiceProvider.GetRequiredService<ResetTestUserService>();
        
        // Confirm database connections 
        await job.TestDbConnection();
        
        // Execute job
        await job.ResetTestUserAsync();
    }
}
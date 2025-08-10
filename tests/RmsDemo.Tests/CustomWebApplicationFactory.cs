using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RmsDemo.Data;

namespace RmsDemo.Tests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove existing DbContext registration (Npgsql)
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<RmsDbContext>));
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Replace with InMemory provider for tests
            services.AddDbContext<RmsDbContext>(options =>
            {
                options.UseInMemoryDatabase("rmsdemo-tests");
            });

            // Build the provider and ensure DB is created
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<RmsDbContext>();
            db.Database.EnsureCreated();
        });
    }
}

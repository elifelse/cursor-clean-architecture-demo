using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CursorDemo.Tests.Integration;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove background service for tests
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(Microsoft.Extensions.Hosting.IHostedService) &&
                     d.ImplementationType?.Name == "BackgroundBookRefresherService");
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }
        });

        builder.UseEnvironment("Testing");
    }
}


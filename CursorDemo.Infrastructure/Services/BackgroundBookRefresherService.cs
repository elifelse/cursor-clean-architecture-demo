using System.Timers;
using CursorDemo.Domain.Entities;
using CursorDemo.Domain.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace CursorDemo.Infrastructure.Services;

/// <summary>
/// Background service that periodically refreshes book data
/// </summary>
public class BackgroundBookRefresherService : IHostedService, IDisposable
{
    private readonly ILogger<BackgroundBookRefresherService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private System.Timers.Timer? _timer;

    public BackgroundBookRefresherService(
        ILogger<BackgroundBookRefresherService> logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("BackgroundBookRefresherService is starting.");

        // Create a timer that fires every 30 seconds
        _timer = new System.Timers.Timer(TimeSpan.FromSeconds(30).TotalMilliseconds);
        _timer.Elapsed += OnTimerElapsed;
        _timer.AutoReset = true;
        _timer.Enabled = true;

        _logger.LogInformation("BackgroundBookRefresherService started. Refresh interval: 30 seconds");

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("BackgroundBookRefresherService is stopping.");

        _timer?.Stop();

        return Task.CompletedTask;
    }

    private async void OnTimerElapsed(object? sender, ElapsedEventArgs e)
    {
        try
        {
            _logger.LogInformation("Background refresh executed at {Time}", DateTime.UtcNow);

            // Use a scope to resolve scoped services (like IBookRepository)
            using var scope = _serviceProvider.CreateScope();
            var bookRepository = scope.ServiceProvider.GetRequiredService<IBookRepository>();

            // Optional: Add a "RefreshedAt" book entry
            // For demonstration, we'll just log the refresh
            // Uncomment the following lines to add a book entry on each refresh:
            /*
            var refreshBook = new Book
            {
                Id = Guid.NewGuid(),
                Title = $"Background Refresh - {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}",
                Author = "System",
                ISBN = $"REFRESH-{DateTime.UtcNow.Ticks}",
                PublishedDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };
            await bookRepository.AddAsync(refreshBook);
            _logger.LogInformation("Added refresh book entry: {BookId}", refreshBook.Id);
            */

            _logger.LogInformation("Background refresh completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during background refresh");
        }
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}


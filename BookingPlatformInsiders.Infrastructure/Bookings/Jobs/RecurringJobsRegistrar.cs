using Hangfire;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BookingPlatformInsiders.Infrastructure.Bookings.Jobs;

internal sealed class RecurringJobsRegistrar : IHostedService
{
    internal const string AutoCompleteJobId = "bookings-autocomplete";

    private readonly IRecurringJobManager _recurringJobManager;
    private readonly IConfiguration _configuration;
    private readonly ILogger<RecurringJobsRegistrar> _logger;

    public RecurringJobsRegistrar(
        IRecurringJobManager recurringJobManager,
        IConfiguration configuration,
        ILogger<RecurringJobsRegistrar> logger)
    {
        _recurringJobManager = recurringJobManager;
        _configuration = configuration;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        var cron = _configuration["BackgroundJobs:AutoComplete:Cron"] ?? "*/5 * * * *";

        _recurringJobManager.AddOrUpdate<CompleteExpiredBookingsJob>(
            AutoCompleteJobId,
            job => job.AutoComplete(JobCancellationToken.Null),
            cron,
            new RecurringJobOptions { TimeZone = TimeZoneInfo.Utc });

        _logger.LogInformation(
            "Recurring job {JobId} registered with cron '{Cron}' (UTC).",
            AutoCompleteJobId,
            cron);

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}

using BookingPlatformInsiders.Application.Bookings.Commands;
using Hangfire;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BookingPlatformInsiders.Infrastructure.Bookings.Jobs;

internal sealed class CompleteExpiredBookingsJob
{
    private readonly IMediator _mediator;
    private readonly IConfiguration _configuration;
    private readonly ILogger<CompleteExpiredBookingsJob> _logger;

    public CompleteExpiredBookingsJob(
        IMediator mediator,
        IConfiguration configuration,
        ILogger<CompleteExpiredBookingsJob> logger)
    {
        _mediator = mediator;
        _configuration = configuration;
        _logger = logger;
    }

    [DisableConcurrentExecution(timeoutInSeconds: 60)]
    public async Task AutoComplete(IJobCancellationToken cancellationToken)
    {
        var batchSize = _configuration.GetValue("BackgroundJobs:AutoComplete:BatchSize", 100);
        var maxBatches = _configuration.GetValue("BackgroundJobs:AutoComplete:MaxBatches", 10);

        var completed = await _mediator.Send(
            new CompleteExpiredBookingsCommand(batchSize, maxBatches),
            cancellationToken.ShutdownToken);

        _logger.LogInformation("Auto-complete finished. Completed bookings: {Completed}.", completed);
    }
}

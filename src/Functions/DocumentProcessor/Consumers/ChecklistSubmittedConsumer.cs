using DocumentProcessor.Jobs;
using Hangfire;
using MassTransit;
using Microsoft.Extensions.Logging;
using Services.Core.Events.ChecklistsEvents;

namespace DocumentProcessor.Consumers;

public class ChecklistSubmittedConsumer : IConsumer<ChecklistSubmitted>
{
    private readonly IBackgroundJobClient _backgroundJobClient;
    private readonly ILogger<ChecklistSubmittedConsumer> _logger;

    public ChecklistSubmittedConsumer(
        IBackgroundJobClient backgroundJobClient,
        ILogger<ChecklistSubmittedConsumer> logger)
    {
        _backgroundJobClient = backgroundJobClient;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ChecklistSubmitted> context)
    {
        var message = context.Message;
        
        _logger.LogInformation("Received ChecklistSubmitted event for checklist {ChecklistId}", 
            message.ChecklistId);

        // Enqueue Hangfire job for processing
        var jobId = _backgroundJobClient.Enqueue<ChecklistProcessorJob>(
            job => job.ProcessChecklistAsync(message));

        _logger.LogInformation("Enqueued Hangfire job {JobId} for checklist {ChecklistId}", 
            jobId, message.ChecklistId);

        await Task.CompletedTask;
    }
}
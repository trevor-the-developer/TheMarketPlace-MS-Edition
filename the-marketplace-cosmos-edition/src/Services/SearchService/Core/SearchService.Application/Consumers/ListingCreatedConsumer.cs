using MassTransit;
using Microsoft.Extensions.Logging;
using SearchService.Application.Contracts.Persistence;
using SearchService.Application.Models;
using Services.Core.Enums;
using Services.Core.Events.ListingEvents;

namespace SearchService.Application.Consumers;

public class ListingCreatedConsumer : IConsumer<ListingCreated>
{
    private readonly ISearchRepository _searchRepository;
    private readonly ILogger<ListingCreatedConsumer> _logger;

    public ListingCreatedConsumer(ISearchRepository searchRepository, ILogger<ListingCreatedConsumer> logger)
    {
        _searchRepository = searchRepository;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ListingCreated> context)
    {
        _logger.LogInformation("ListingCreated message received for listing: {ListingId}", context.Message.ListingId);

        var listing = context.Message;

        try
        {
            var item = new Item
            {
                Id = listing.ListingId.ToString(),
                Name = listing.Title,
                Description = listing.Description,
                Type = "Listing",
                AccountId = listing.SellerId.ToString(),
                ResourceUrl = listing.ResourceUrl,
                Status = listing.IsActive ? ServiceStatus.Active : ServiceStatus.Inactive,
                CreatedAt = listing.CreatedAt,
                UpdatedAt = listing.CreatedAt,
                Metadata = new Dictionary<string, string>
                {
                    { "CategoryId", listing.CategoryId.ToString() },
                    { "CategoryName", listing.CategoryName },
                    { "Price", listing.Price.ToString() },
                    { "Location", listing.Location }
                }
            };

            // Add tags to metadata
            if (listing.TagNames != null && listing.TagNames.Length > 0)
            {
                item.Metadata.Add("Tags", string.Join(",", listing.TagNames));
            }

            await _searchRepository.SaveAsync(item);
            _logger.LogInformation("Listing indexed successfully: {ListingId}", listing.ListingId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error indexing listing: {ListingId}", listing.ListingId);
            throw;
        }
    }
}
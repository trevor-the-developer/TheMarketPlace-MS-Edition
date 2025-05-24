namespace Services.Core.Events.ListingEvents;

public record ListingPublished(
    Guid ListingId,
    DateTime PublishedAt
);
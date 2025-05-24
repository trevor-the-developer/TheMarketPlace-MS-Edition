namespace Services.Core.Events.ListingEvents;

public record ListingCreated(
    Guid ListingId,
    string Title,
    string Description,
    decimal Price,
    string Location,
    Guid SellerId,
    Guid CategoryId,
    string CategoryName,
    string[] TagNames,
    string ResourceUrl,
    bool IsActive,
    DateTime CreatedAt
);
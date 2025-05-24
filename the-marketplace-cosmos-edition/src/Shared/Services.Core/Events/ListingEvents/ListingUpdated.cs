namespace Services.Core.Events.ListingEvents;

public record ListingUpdated(
    Guid ListingId,
    string Title,
    string Description,
    decimal Price,
    string Location,
    Guid CategoryId,
    string CategoryName,
    string[] TagNames,
    string ResourceUrl,
    bool IsActive,
    DateTime UpdatedAt
);
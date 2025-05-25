namespace Services.Core.ServiceBus;

public static class ServiceBusConstants
{
    public static class Topics
    {
        
        public static class Listing
        {
            public const string Created = "listing.events.created";
            public const string Updated = "listing.events.updated";
            public const string Published = "listing.events.published";
            public const string Deleted = "listing.events.deleted";
            
            public static class Subscriptions
            {
                public const string SearchServiceCreated = "search-service.listing-indexing.created";
                public const string SearchServiceUpdated = "search-service.listing-indexing.updated";
                public const string SearchServiceDeleted = "search-service.listing-indexing.deleted";
            }
        }
        
        public static class Checklist
        {
            public const string Created = "checklist.events.created";
            public const string Submitted = "checklist.events.submitted";
            
            public static class Subscriptions
            {
                public const string DocumentProcessorSubmitted = "document-processor.checklist-stats.submitted";
            }
        }
        
        public static class DeadLetter
        {
            public const string ChecklistCreated = "deadletter.checklist.events.created";
            public const string ChecklistSubmitted = "deadletter.checklist.events.submitted";
            public const string ListingCreated = "deadletter.listing.events.created";
            public const string ListingUpdated = "deadletter.listing.events.updated";
            public const string ListingPublished = "deadletter.listing.events.published";
            public const string ListingDeleted = "deadletter.listing.events.deleted";

            public static class Subscriptions
            {
                public const string ListingSearchServiceCreated = "deadletter.search-service.listing-indexing.created";
                public const string ListingSearchServiceUpdated = "deadletter.search-service.listing-indexing.updated";
                public const string ListingSearchServiceDeleted = "deadletter.search-service.listing-indexing.deleted";
            }
        }
    }
}
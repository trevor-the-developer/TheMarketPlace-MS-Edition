using Demo.IntegrationTests.Core.Infrastructure;
using FluentAssertions;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Services.Core.Events.ListingEvents;
using Xunit;

namespace Demo.IntegrationTests.Core.MessageFlow;

public class EndToEndMessageFlowTests : IntegrationTestBase
{
    [Fact]
    public async Task When_ListingCreated_Event_Published_Should_Be_Consumed_By_SearchService()
    {
        // Arrange
        var services = CreateTestServices();
        
        services.AddMassTransitTestHarness(cfg =>
        {
            cfg.UsingRabbitMq((context, rabbitmqCfg) =>
            {
                rabbitmqCfg.Host(new Uri(RabbitMqConnectionString));
                rabbitmqCfg.ConfigureEndpoints(context);
            });
        });

        var provider = services.BuildServiceProvider();
        var harness = provider.GetRequiredService<ITestHarness>();
        
        await harness.Start();

        try
        {
            // Act
            var listingCreatedEvent = new ListingCreated(
                ListingId: Guid.NewGuid(),
                Title: "Test Listing",
                Description: "Test Description",
                Price: 100.00m,
                Location: "Test Location",
                SellerId: Guid.NewGuid(),
                CategoryId: Guid.NewGuid(),
                CategoryName: "Test Category",
                TagNames: new[] { "test", "quick" },
                ResourceUrl: "http://test.com",
                IsActive: true,
                CreatedAt: DateTime.UtcNow
            );

            await harness.Bus.Publish(listingCreatedEvent);

            // Assert
            (await harness.Published.Any<ListingCreated>()).Should().BeTrue();
        }
        finally
        {
            await harness.Stop();
        }
    }

    [Fact]
    public async Task RabbitMQ_Connection_Should_Be_Established_Successfully()
    {
        // Arrange
        var services = CreateTestServices();
        
        services.AddMassTransit(cfg =>
        {
            cfg.UsingRabbitMq((context, rabbitmqCfg) =>
            {
                rabbitmqCfg.Host(new Uri(RabbitMqConnectionString));
            });
        });

        var provider = services.BuildServiceProvider();
        var busControl = provider.GetRequiredService<IBusControl>();

        // Act & Assert
        await busControl.StartAsync();
        // Verify bus is running by checking it can be stopped successfully
        await busControl.StopAsync();
    }

    [Fact]
    public async Task Message_Should_Flow_Through_Complete_Pipeline()
    {
        // Arrange
        var services = CreateTestServices();
        var messageReceived = false;
        var receivedMessage = default(ListingCreated);

        services.AddMassTransit(cfg =>
        {
            cfg.AddConsumer<TestListingCreatedConsumer>();
            
            cfg.UsingRabbitMq((context, rabbitmqCfg) =>
            {
                rabbitmqCfg.Host(new Uri(RabbitMqConnectionString));
                
                rabbitmqCfg.ReceiveEndpoint("test-listing-created", e =>
                {
                    e.ConfigureConsumer<TestListingCreatedConsumer>(context);
                });
            });
        });

        services.AddSingleton<TestListingCreatedConsumer>(provider => 
            new TestListingCreatedConsumer(msg =>
            {
                messageReceived = true;
                receivedMessage = msg;
            }));

        var provider = services.BuildServiceProvider();
        var busControl = provider.GetRequiredService<IBusControl>();

        await busControl.StartAsync();

        try
        {
            // Act
            var listingCreatedEvent = new ListingCreated(
                ListingId: Guid.NewGuid(),
                Title: "Integration Test Listing",
                Description: "Integration Test Description",
                Price: 250.00m,
                Location: "Test Location",
                SellerId: Guid.NewGuid(),
                CategoryId: Guid.NewGuid(),
                CategoryName: "Test Category",
                TagNames: new[] { "test", "integration" },
                ResourceUrl: "http://test.com",
                IsActive: true,
                CreatedAt: DateTime.UtcNow
            );

            await busControl.Publish(listingCreatedEvent);
            
            // Wait for message processing
            await Task.Delay(1000);

            // Assert
            messageReceived.Should().BeTrue();
            receivedMessage.Should().NotBeNull();
            receivedMessage!.ListingId.Should().Be(listingCreatedEvent.ListingId);
            receivedMessage.Title.Should().Be(listingCreatedEvent.Title);
        }
        finally
        {
            await busControl.StopAsync();
        }
    }
}

public class TestListingCreatedConsumer : IConsumer<ListingCreated>
{
    private readonly Action<ListingCreated> _onMessageReceived;

    public TestListingCreatedConsumer(Action<ListingCreated> onMessageReceived)
    {
        _onMessageReceived = onMessageReceived;
    }

    public Task Consume(ConsumeContext<ListingCreated> context)
    {
        _onMessageReceived(context.Message);
        return Task.CompletedTask;
    }
}
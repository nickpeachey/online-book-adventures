using FluentAssertions;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using OnlineBookAdventures.Application.Events;
using OnlineBookAdventures.Application.Common.Interfaces;
using OnlineBookAdventures.Infrastructure.Analytics;
using OnlineBookAdventures.Infrastructure.Messaging.Consumers;

namespace OnlineBookAdventures.Tests.Analytics;

/// <summary>
/// Tests for analytics event consumers using MassTransit in-memory test harness.
/// </summary>
public sealed class AnalyticsConsumerTests : IAsyncLifetime
{
    private ServiceProvider _provider = null!;
    private ITestHarness _harness = null!;
    private InMemoryAnalyticsStore _store = null!;

    public async Task InitializeAsync()
    {
        _store = new InMemoryAnalyticsStore();

        _provider = new ServiceCollection()
            .AddSingleton<IAnalyticsStore>(_store)
            .AddSingleton(typeof(Microsoft.Extensions.Logging.ILogger<>), typeof(NullLogger<>))
            .AddMassTransitTestHarness(x =>
            {
                x.AddConsumer<StoryStartedConsumer>();
                x.AddConsumer<ChoiceMadeConsumer>();
                x.AddConsumer<StoryCompletedConsumer>();
                x.AddConsumer<StoryRatedConsumer>();
            })
            .BuildServiceProvider(true);

        _harness = _provider.GetRequiredService<ITestHarness>();
        await _harness.Start();
    }

    public async Task DisposeAsync()
    {
        await _harness.Stop();
        await _provider.DisposeAsync();
    }

    [Fact]
    public async Task StoryStartedConsumer_IncrementsStartCount()
    {
        // Arrange
        var storyId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        // Act
        await _harness.Bus.Publish(new StoryStartedEvent(userId, storyId, DateTimeOffset.UtcNow));
        await _harness.Consumed.Any<StoryStartedEvent>();

        // Assert
        var analytics = _store.Get(storyId);
        analytics.Should().NotBeNull();
        analytics!.StartCount.Should().Be(1);
    }

    [Fact]
    public async Task ChoiceMadeConsumer_IncrementsTotalChoices()
    {
        // Arrange
        var storyId = Guid.NewGuid();
        var fromNodeId = Guid.NewGuid();
        var toNodeId = Guid.NewGuid();

        // Act
        await _harness.Bus.Publish(new ChoiceMadeEvent(Guid.NewGuid(), storyId, Guid.NewGuid(), fromNodeId, toNodeId, DateTimeOffset.UtcNow));
        await _harness.Consumed.Any<ChoiceMadeEvent>();

        // Assert
        var analytics = _store.Get(storyId);
        analytics!.TotalChoicesMade.Should().Be(1);
    }

    [Fact]
    public async Task StoryCompletedConsumer_IncrementsCompletionCount()
    {
        // Arrange
        var storyId = Guid.NewGuid();

        // Act
        await _harness.Bus.Publish(new StoryCompletedEvent(Guid.NewGuid(), storyId, Guid.NewGuid(), DateTimeOffset.UtcNow));
        await _harness.Consumed.Any<StoryCompletedEvent>();

        // Assert
        var analytics = _store.Get(storyId);
        analytics!.CompletionCount.Should().Be(1);
    }

    [Fact]
    public async Task StoryRatedConsumer_ConsumesWithoutError()
    {
        // Act
        await _harness.Bus.Publish(new StoryRatedEvent(Guid.NewGuid(), Guid.NewGuid(), 5, DateTimeOffset.UtcNow));

        // Assert
        (await _harness.Consumed.Any<StoryRatedEvent>()).Should().BeTrue();
    }
}

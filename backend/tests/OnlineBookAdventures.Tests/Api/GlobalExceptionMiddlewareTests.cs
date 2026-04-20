using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OnlineBookAdventures.Application.Common.Interfaces;
using OnlineBookAdventures.Infrastructure.Persistence;

namespace OnlineBookAdventures.Tests.Api;

/// <summary>
/// Integration tests verifying the global exception middleware returns structured ProblemDetails.
/// </summary>
public sealed class GlobalExceptionMiddlewareTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public GlobalExceptionMiddlewareTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var toRemove = services
                    .Where(d =>
                        d.ServiceType == typeof(AppDbContext) ||
                        d.ServiceType == typeof(DbContextOptions<AppDbContext>) ||
                        d.ServiceType == typeof(DbContextOptions) ||
                        d.ServiceType.Name.Contains("DbContextOptions"))
                    .ToList();
                foreach (var d in toRemove)
                    services.Remove(d);

                services.AddDbContext<AppDbContext>(options =>
                    options.UseInMemoryDatabase($"MiddlewareTests_{Guid.NewGuid()}"));

                services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<AppDbContext>());
            });
        }).CreateClient();
    }

    [Fact]
    public async Task GetNonExistentStory_Returns404ProblemDetails()
    {
        // Act
        var response = await _client.GetAsync($"/api/stories/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/problem+json");

        var body = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        body.Should().ContainKey("title");
        body!["title"].ToString().Should().Be("Not Found");
    }

    [Fact]
    public async Task PostInvalidLogin_Returns400WithValidationErrors()
    {
        // Arrange — email is invalid
        var request = new { Email = "not-an-email", Password = "" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}

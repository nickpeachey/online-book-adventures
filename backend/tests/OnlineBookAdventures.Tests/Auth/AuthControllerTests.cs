using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OnlineBookAdventures.Api.Controllers;
using OnlineBookAdventures.Application.Common.Interfaces;
using OnlineBookAdventures.Application.Features.Auth.Commands.Login;
using OnlineBookAdventures.Application.Features.Auth.Commands.Register;
using OnlineBookAdventures.Infrastructure.Persistence;

namespace OnlineBookAdventures.Tests.Auth;

public sealed class AuthControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public AuthControllerTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["JwtSettings:SecretKey"] = "test-secret-key-that-is-at-least-32-chars!!",
                    ["JwtSettings:Issuer"] = "OnlineBookAdventures",
                    ["JwtSettings:Audience"] = "OnlineBookAdventures",
                    ["JwtSettings:ExpiryMinutes"] = "60"
                });
            });

            builder.ConfigureServices(services =>
            {
                // Remove ALL DbContext-related registrations to prevent dual-provider conflict
                var toRemove = services
                    .Where(d =>
                        d.ServiceType == typeof(AppDbContext) ||
                        d.ServiceType == typeof(DbContextOptions<AppDbContext>) ||
                        d.ServiceType == typeof(DbContextOptions) ||
                        d.ServiceType.Name.Contains("DbContextOptions"))
                    .ToList();
                foreach (var d in toRemove)
                    services.Remove(d);

                // Generate the DB name once so all requests in this test share the same in-memory DB
                var dbName = $"TestDb_{Guid.NewGuid()}";
                services.AddDbContext<AppDbContext>(options =>
                    options.UseInMemoryDatabase(dbName));

                services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<AppDbContext>());
            });
        }).CreateClient();
    }

    [Fact]
    public async Task Register_WithValidData_Returns201AndToken()
    {
        var request = new RegisterRequest("testuser", "test@example.com", "Password123!");

        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = await response.Content.ReadFromJsonAsync<RegisterResult>();
        result.Should().NotBeNull();
        result!.Username.Should().Be("testuser");
        result.AccessToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ThrowsConflict()
    {
        var request = new RegisterRequest("user1", "dup@example.com", "Password123!");
        await _client.PostAsJsonAsync("/api/auth/register", request);

        var duplicate = new RegisterRequest("user2", "dup@example.com", "Password123!");

        var response = await _client.PostAsJsonAsync("/api/auth/register", duplicate);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Login_WithValidCredentials_Returns200AndToken()
    {
        var registerRequest = new RegisterRequest("loginuser", "login@example.com", "Password123!");
        await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

        var loginRequest = new LoginRequest("login@example.com", "Password123!");

        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<LoginResult>();
        result.Should().NotBeNull();
        result!.AccessToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Login_WithWrongPassword_Returns401()
    {
        var registerRequest = new RegisterRequest("wrongpassuser", "wrongpass@example.com", "Password123!");
        await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

        var loginRequest = new LoginRequest("wrongpass@example.com", "WrongPassword!");

        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Register_WithInvalidEmail_Returns400()
    {
        var request = new RegisterRequest("user", "not-an-email", "Password123!");

        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}

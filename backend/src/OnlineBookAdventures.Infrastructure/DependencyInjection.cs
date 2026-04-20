using System.Text;
using Amazon.S3;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using OnlineBookAdventures.Application.Common.Interfaces;
using OnlineBookAdventures.Infrastructure.Analytics;
using OnlineBookAdventures.Infrastructure.HealthChecks;
using OnlineBookAdventures.Infrastructure.Messaging;
using OnlineBookAdventures.Infrastructure.Messaging.Consumers;
using OnlineBookAdventures.Infrastructure.Persistence;
using OnlineBookAdventures.Infrastructure.Services;
using OnlineBookAdventures.Infrastructure.Storage;
using StackExchange.Redis;

namespace OnlineBookAdventures.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                npgsql => npgsql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)));

        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<AppDbContext>());
        services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();
        services.AddScoped<ITokenService, JwtTokenService>();

        var jwtSettings = configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"] ?? "dev-secret-key-min-32-chars-long!!";

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings["Issuer"] ?? "OnlineBookAdventures",
                    ValidAudience = jwtSettings["Audience"] ?? "OnlineBookAdventures",
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
                };
            });

        services.AddAuthorizationBuilder();

        // Redis
        var redisConnectionString = configuration.GetConnectionString("Redis") ?? "localhost:6379";
        services.AddSingleton<IConnectionMultiplexer>(_ =>
            ConnectionMultiplexer.Connect(redisConnectionString));
        services.AddScoped<IStoryGraphCache, RedisStoryGraphCache>();

        // MassTransit + RabbitMQ
        var rabbitMqHost = configuration["RabbitMQ:Host"] ?? "localhost";
        var rabbitMqUser = configuration["RabbitMQ:Username"] ?? "cyoa";
        var rabbitMqPass = configuration["RabbitMQ:Password"] ?? "cyoa_dev_password";

        services.AddSingleton<IAnalyticsStore, InMemoryAnalyticsStore>();

        services.AddMassTransit(x =>
        {
            x.AddConsumer<StoryStartedConsumer>();
            x.AddConsumer<ChoiceMadeConsumer>();
            x.AddConsumer<StoryCompletedConsumer>();
            x.AddConsumer<StoryRatedConsumer>();

            x.UsingRabbitMq((ctx, cfg) =>
            {
                cfg.Host(rabbitMqHost, "/", h =>
                {
                    h.Username(rabbitMqUser);
                    h.Password(rabbitMqPass);
                });

                cfg.ConfigureEndpoints(ctx);
            });
        });

        services.AddScoped<IEventPublisher, MassTransitEventPublisher>();

        // S3 / MinIO
        var minioUrl = configuration.GetConnectionString("MinIO") ?? "http://localhost:9000";
        var minioAccessKey = configuration["MinIO:AccessKey"] ?? "cyoa";
        var minioSecretKey = configuration["MinIO:SecretKey"] ?? "cyoa_dev_password";

        services.AddSingleton<IAmazonS3>(_ =>
        {
            var s3Config = new AmazonS3Config
            {
                ServiceURL = minioUrl,
                ForcePathStyle = true  // Required for MinIO
            };
            return new AmazonS3Client(minioAccessKey, minioSecretKey, s3Config);
        });

        services.AddScoped<IStorageService, S3StorageService>();

        // OpenAI
        services.AddHttpClient();
        services.AddScoped<IStoryGenerationService, OpenAIStoryGenerationService>();

        // Health Checks
        services.AddHealthChecks()
            .AddDbContextCheck<AppDbContext>("database")
            .AddCheck<RedisHealthCheck>("redis");

        return services;
    }
}

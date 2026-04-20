using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using OnlineBookAdventures.Application.Common.Behaviours;

namespace OnlineBookAdventures.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(typeof(DependencyInjection).Assembly);

        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));

        return services;
    }
}

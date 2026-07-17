using System.Reflection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace TicketReserve.Api.Infrastructure.Endpoints;

public static class EndpointExtensions
{
    public static IServiceCollection AddEndpoints(this IServiceCollection services, Assembly assembly)
    {
        var endpointDescriptors = assembly.GetTypes()
            .Where(type => type is { IsClass: true, IsAbstract: false } && typeof(IEndpoint).IsAssignableFrom(type))
            .Select(type => ServiceDescriptor.Transient(typeof(IEndpoint), type));

        services.TryAddEnumerable(endpointDescriptors);

        return services;
    }

    public static void MapEndpoints(this WebApplication app)
    {
        var apiGroup = app.MapGroup("/api");

        var endpoints = app.Services.GetServices<IEndpoint>();

        foreach (var endpoint in endpoints)
        {
            endpoint.MapEndpoint(apiGroup);
        }
    }
}

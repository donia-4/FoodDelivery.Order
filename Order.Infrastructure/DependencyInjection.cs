using CloudinaryDotNet;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Order.Application.Common.Interfaces.Messaging;
using Order.Application.Common.Interfaces.Services;
using Order.Infrastructure.BackgroundJobs;
using Order.Infrastructure.Data;
using Order.Infrastructure.Data.Interceptors;
using Order.Infrastructure.RabbitMQ;
using Order.Infrastructure.Services;
using Order.Infrastructure.Settings;
using RabbitMQ.Client;

namespace Order.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddDatabase(configuration)
            .AddCaching()
            .AddCloudinary(configuration)
            .AddRabbitMq(configuration)
            .AddRepositories();

        return services;
    }

    private static IServiceCollection AddCloudinary(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<CloudinarySettings>(
            configuration.GetSection("Cloudinary"));

        services.AddSingleton(sp =>
        {
            var settings = sp
                .GetRequiredService<IOptions<CloudinarySettings>>()
                .Value;

            var account = new Account(
                settings.CloudName,
                settings.ApiKey,
                settings.ApiSecret);

            return new Cloudinary(account);
        });

        return services;
    }

    private static IServiceCollection AddDatabase(
        this IServiceCollection services,
        IConfiguration configuration)
    {

        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddSingleton(TimeProvider.System);

        services.AddScoped<ISaveChangesInterceptor, AuditableEntityInterceptor>();

        services.AddDbContext<OrderDbContext>((sp, options) =>
        {
            options.UseSqlServer(connectionString);

            options.AddInterceptors(
                sp.GetServices<ISaveChangesInterceptor>());
        });

        return services;
    }
    private static IServiceCollection AddRabbitMq(
    this IServiceCollection services,
    IConfiguration configuration)
    {
        services.Configure<RabbitMqOptions>(
            configuration.GetSection(RabbitMqOptions.SectionName));

        services.AddSingleton<ConnectionFactory>(sp =>
        {
            var options = sp
                .GetRequiredService<IOptions<RabbitMqOptions>>()
                .Value;

            return new ConnectionFactory
            {
                HostName = options.HostName,
                Port = options.Port,
                UserName = options.UserName,
                Password = options.Password
            };
        });

        services.AddSingleton<IEventPublisher, RabbitMqPublisher>();
        services.AddScoped<IOutbox, EfOutbox>();

        // Background worker that polls the outbox table and publishes to RabbitMQ
        services.AddHostedService<OutboxProcessor>();

        return services;
    }
    private static IServiceCollection AddCaching(
        this IServiceCollection services)
    {
        services.AddHybridCache();

        services.AddScoped<ICacheService, HybridCacheService>();

        services.AddScoped<IFileService, CloudinaryFileService>();

        return services;
    }

    private static IServiceCollection AddRepositories(
        this IServiceCollection services)
    {

        return services;
    }
}
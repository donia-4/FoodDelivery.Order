using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Order.Application.Common.Interfaces.Messaging;
using Order.Application.Common.Interfaces.Repositories;
using Order.Application.Common.Interfaces.Services;
using Order.Infrastructure.BackgroundJobs;
using Order.Infrastructure.Data;
using Order.Infrastructure.Data.Interceptors;
using Order.Infrastructure.RabbitMQ;
using Order.Infrastructure.Repositories;
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
            .AddRabbitMq(configuration)
            .AddRepositories()
            .AddServices()
            .AddHttpClients(configuration)
            .AddJwtAuthentication(configuration)
            .AddJwtAuthorization();

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

        return services;
    }

    private static IServiceCollection AddRepositories(
        this IServiceCollection services)
    {
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IOrderStatusHistoryRepository, OrderStatusHistoryRepository>();
        services.AddScoped<ICartRepository, CartRepository>(); 

        return services;
    }
    private static IServiceCollection AddServices(
        this IServiceCollection services)
    {
        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<IRestaurantService, RestaurantService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();


        return services;
    }
    private static IServiceCollection AddHttpClients(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddHttpContextAccessor(); // ← we can access HttpContext through it

        // Identity Service
        services.AddHttpClient("IdentityService", client =>
        {
            client.BaseAddress = new Uri(
                configuration["Services:IdentityBaseUrl"]!);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        })
        .AddHttpMessageHandler<AuthHeaderHandler>(); // ← بيضيف التوكن أوتوماتيك

        // Restaurant Service
        services.AddHttpClient("RestaurantService", client =>
        {
            client.BaseAddress = new Uri(
                configuration["Services:RestaurantBaseUrl"]!);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        })
        .AddHttpMessageHandler<AuthHeaderHandler>();

        services.AddScoped<AuthHeaderHandler>();

        return services;
    }
    private static IServiceCollection AddJwtAuthentication(
    this IServiceCollection services,
    IConfiguration configuration)
    {
        var jwtSettings = configuration
            .GetSection(JwtSettings.SectionName)
            .Get<JwtSettings>()
            ?? throw new InvalidOperationException(
                $"Configuration section '{JwtSettings.SectionName}' is missing.");

        services
            .AddOptions<JwtSettings>()
            .Bind(configuration.GetSection(JwtSettings.SectionName))
            .ValidateOnStart();

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtSettings.Issuer,

                    ValidateAudience = true,
                    ValidAudiences = jwtSettings.Audience,

                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtSettings.Secret)),

                    ValidateLifetime = true,

                    ClockSkew = TimeSpan.Zero
                };
            });

        return services;
    }

    private static IServiceCollection AddJwtAuthorization(
        this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            options.AddPolicy("Authenticated",
                policy => policy.RequireAuthenticatedUser());

            options.AddPolicy("AdminOnly",
                policy => policy.RequireRole("Admin"));

            options.AddPolicy("RestaurantOwnerOnly",
                policy => policy.RequireRole("RestaurantOwner"));

            options.AddPolicy("CustomerOnly",
                policy => policy.RequireRole("Customer"));
        });

        return services;
    }
}
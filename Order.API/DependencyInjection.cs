using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.OpenApi.Models;
using Order.API.Middlewares;

namespace Order.API
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddPresentationServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services
                .AddApiDocumentation()
                .AddAppCors(configuration)
                .AddAppOutputCaching()
                .AddAppHealthChecks(configuration)
                .AddExceptionHandling()
                .AddCustomProblemDetails()
                .AddAppRateLimiting()
                .AddHttpClients(configuration);

            return services;
        }
        private static IServiceCollection AddHttpClients(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Identity Service 
            services.AddHttpClient("IdentityService", client =>
            {
                client.BaseAddress = new Uri(
                    configuration["Services:IdentityBaseUrl"]!); // https://identityservices.runasp.net
                client.DefaultRequestHeaders.Add("Accept", "application/json");
            });

            // Restaurant Service 
            services.AddHttpClient("RestaurantService", client =>
            {
                client.BaseAddress = new Uri(
                    configuration["Services:RestaurantBaseUrl"]!); // https://localhost:7126
                client.DefaultRequestHeaders.Add("Accept", "application/json");
            });

            return services;
        }
        private static IServiceCollection AddApiDocumentation(
            this IServiceCollection services)
        {
            services.AddEndpointsApiExplorer();

            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Order Service API"
                });

                options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme,
                    new OpenApiSecurityScheme
                    {
                        Name = "Authorization",
                        Type = SecuritySchemeType.Http,
                        Scheme = "bearer",
                        BearerFormat = "JWT",
                        In = ParameterLocation.Header,
                        Description = "Enter your JWT token."
                    });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = JwtBearerDefaults.AuthenticationScheme
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });

            return services;
        }

        private static IServiceCollection AddAppCors(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("DefaultCorsPolicy", builder =>
                {
                    var allowedOrigins =
                        configuration
                            .GetSection("Cors:AllowedOrigins")
                            .Get<string[]>() ?? [];

                    if (allowedOrigins.Length > 0)
                    {
                        builder.WithOrigins(allowedOrigins)
                            .AllowAnyHeader()
                            .AllowAnyMethod();
                    }
                    else
                    {
                        builder.AllowAnyOrigin()
                            .AllowAnyHeader()
                            .AllowAnyMethod();
                    }
                });
            });

            return services;
        }

        private static IServiceCollection AddAppOutputCaching(
            this IServiceCollection services)
        {
            services.AddOutputCache(options =>
            {
                options.AddPolicy(
                    "DefaultCache",
                    policy => policy.Expire(TimeSpan.FromMinutes(1)));
            });

            return services;
        }

        private static IServiceCollection AddAppHealthChecks(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            services
                .AddHealthChecks()
                .AddSqlServer(connectionString!);

            return services;
        }

        private static IServiceCollection AddExceptionHandling(this IServiceCollection services)
        {
            services.AddExceptionHandler<GlobalExceptionHandler>();
            return services;
        }
        private static IServiceCollection AddCustomProblemDetails(this IServiceCollection services)
        {
            services.AddProblemDetails(options => options.CustomizeProblemDetails = (context) =>
            {
                context.ProblemDetails.Instance = $"{context.HttpContext.Request.Method} {context.HttpContext.Request.Path}";
                context.ProblemDetails.Extensions.Add("requestId", context.HttpContext.TraceIdentifier);
            });

            return services;
        }

        private static IServiceCollection AddAppRateLimiting(this IServiceCollection services)
        {
            services.AddRateLimiter(options =>
            {
                options.AddSlidingWindowLimiter("SlidingWindow", limiterOptions =>
                {
                    limiterOptions.PermitLimit = 100;
                    limiterOptions.Window = TimeSpan.FromMinutes(1);
                    limiterOptions.SegmentsPerWindow = 6;
                    limiterOptions.QueueLimit = 10;
                    limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    limiterOptions.AutoReplenishment = true;
                });

                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            });

            return services;
        }
    }
}

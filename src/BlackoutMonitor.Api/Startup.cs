using System;
using System.Reflection;
using System.Text.Json.Serialization;
using BlackoutMonitor.Api.Configuration;
using BlackoutMonitor.Api.Infrastructure;
using BlackoutMonitor.Api.Persistence;
using BlackoutMonitor.Api.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Telegram.Bot;

namespace BlackoutMonitor.Api;

public class Startup
{
    public IConfiguration Configuration { get; }

    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services
            .AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

        services.AddHealthChecks();

        services.AddSingleton(sp =>
        {
            var endpoint = Configuration.GetValue<string>("CosmosDb:Endpoint");
            var key = Configuration.GetValue<string>("CosmosDb:Key");
            var options = new CosmosClientOptions
            {
                SerializerOptions = new CosmosSerializationOptions()
                {
                    PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase,
                }
            };
            return new CosmosClient(endpoint, key, options);
        });

        services.AddHttpClient("TelegramBot")
            .AddTypedClient<ITelegramBotClient>(httpClient =>
            {
                var token = Configuration.GetValue<string>("TelegramNotification:BotToken");
                return new TelegramBotClient(token, httpClient);
            });

        services.Configure<BlackoutMonitorDbOptions>(options =>
        {
            options.DbName = Configuration.GetValue<string>("CosmosDb:Database");
        });

        services.Configure<TelegramNotificationOptions>(options =>
        {
            options.ChannelId = Configuration.GetValue<string>("TelegramNotification:ChannelId");
        });

        services.Configure<BeeperExpirationOptions>(options =>
        {
            options.CheckInterval = TimeSpan.FromSeconds(Configuration.GetValue<int>("BeeperExpiration:CheckIntervalSeconds"));
            options.ExpirationTime = TimeSpan.FromSeconds(Configuration.GetValue<int>("BeeperExpiration:ExpirationTimeSeconds"));
        });

        services
            .AddSingleton<BeeperManager>()
            .AddHostedService(sp => sp.GetService<BeeperManager>());

        services.AddScoped<BlackoutRepository>();
        services.AddScoped<NotificationService>();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger)
    {
        app.UseDeveloperExceptionPage();

        app.UseRouting();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
            endpoints.MapHealthChecks("/health", new HealthCheckOptions 
            {
                ResponseWriter = HealthCheckResponseWriter.WriteAsync,
            });
        });
    }
}

using System;
using BlackoutMonitor.Api.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

namespace BlackoutMonitor.Api;

public static class Program
{
    public static void Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .WriteTo.Console()
            .CreateBootstrapLogger();

        var version = ProductVersion.GetFromEntryAssembly();

        try
        {
            Log.Information($"HOST IS STARTING {version}");

            CreateHostBuilder(args).Build().Run();

            Log.Information($"HOST IS SHUTTING DOWN {version}");
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, $"HOST TERMINATED UNEXPECTEDLY {version}");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    public static IHostBuilder CreateHostBuilder(string[] args)
    {
        return Host.CreateDefaultBuilder(args)
            .UseSerilog((context, services, configuration) => 
            {
                configuration
                    .ReadFrom.Configuration(context.Configuration)
                    .WriteTo.Console();
            })
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            });
    }
}
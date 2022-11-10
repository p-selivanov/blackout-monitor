using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace BlackoutMonitor.Api.Infrastructure;

public static class HealthCheckResponseWriter
{
    public static async Task WriteAsync(HttpContext httpContext, HealthReport report)
    {
        httpContext.Response.ContentType = "application/json";
        
        await using var writer = new StreamWriter(httpContext.Response.Body);
        await writer.WriteLineAsync(report.Status.ToString());
        await writer.WriteAsync(ProductVersion.GetFromEntryAssembly());
    }
}

using System.Reflection;

namespace BlackoutMonitor.Api.Infrastructure;

public static class ProductVersion
{
    public static string GetFromEntryAssembly()
    {
        var fileVersion = Assembly
            .GetExecutingAssembly()
            .GetCustomAttribute<AssemblyFileVersionAttribute>()
            ?.Version ?? "1.0.0";

        return "v" + fileVersion;
    }
}

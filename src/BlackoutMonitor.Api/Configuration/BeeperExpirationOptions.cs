using System;

namespace BlackoutMonitor.Api.Configuration;

public class BeeperExpirationOptions
{
    public TimeSpan CheckInterval { get; set; } = TimeSpan.FromSeconds(30);

    public TimeSpan ExpirationTime { get; set; } = TimeSpan.FromSeconds(70);
}

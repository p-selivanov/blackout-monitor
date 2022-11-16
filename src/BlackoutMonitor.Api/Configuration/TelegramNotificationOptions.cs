using System.Collections.Generic;

namespace BlackoutMonitor.Api.Configuration;

public class TelegramNotificationOptions
{
    public Dictionary<string, string> BeeperChannelIds { get; set; }
}

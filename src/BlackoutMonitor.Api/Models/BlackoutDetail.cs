using System;

namespace BlackoutMonitor.Api.Models;

public class BlackoutDetail
{
    public string Id { get; set; }

    public string BeeperId { get; set; }

    public DateTime StartTimestamp { get; set; }

    public DateTime? FinishTimestamp { get; set; }
}

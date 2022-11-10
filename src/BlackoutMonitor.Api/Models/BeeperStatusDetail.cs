using System;

namespace BlackoutMonitor.Api.Models;

public class BeeperStatusDetail
{
    public string Id { get; set; }

    public BeeperStatus Status { get; set; }

    public DateTime UpdatedOn { get; set; }
}

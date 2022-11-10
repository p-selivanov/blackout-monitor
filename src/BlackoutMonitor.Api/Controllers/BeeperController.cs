using System.Threading.Tasks;
using BlackoutMonitor.Api.Models;
using BlackoutMonitor.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace BlackoutMonitor.Api.Controllers;

[ApiController]
[Route("beepers")]
public class BeeperController : ControllerBase
{
    private readonly BeeperManager _beeperManager;
    private readonly ILogger _logger;

    public BeeperController(BeeperManager beeperManager, ILogger<BeeperController> logger)
    {
        _beeperManager = beeperManager;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult GetAllBeepers()
    {
        var statuses = _beeperManager.GetBeeperStatuses();
        return Ok(statuses);
    }

    [HttpGet("{beeperId}")]
    public IActionResult GetBeeper(string beeperId)
    {
        var status = _beeperManager.GetBeeperStatus(beeperId);
        if (status is null)
        {
            return NotFound();
        }

        return Ok(status);
    }

    [HttpPost("{beeperId}/status")]
    public async Task<IActionResult> CreateStatusReport(string beeperId, ReportBeeperStatusRequestDto request)
    {
        _logger.LogInformation("Received beeper status: {beeperId} is {status}", beeperId, request.Status);

        await _beeperManager.SetBeeperStatusAsync(beeperId, request.Status);

        return Accepted();
    }
}
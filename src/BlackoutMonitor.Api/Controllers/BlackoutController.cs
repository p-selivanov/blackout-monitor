using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using BlackoutMonitor.Api.Persistence;
using CsvHelper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BlackoutMonitor.Api.Controllers;

[ApiController]
[Route("blackouts")]
public class BlackoutController : ControllerBase
{
    private readonly BlackoutRepository _blackoutRepository;

    public BlackoutController(BlackoutRepository blackoutRepository)
    {
        _blackoutRepository = blackoutRepository;
    }

    [HttpGet]
    public async Task<IActionResult> GetBlackouts()
    {
        var blackouts = await _blackoutRepository.GetAllBlackoutsAsync();
        return Ok(blackouts);
    }

    [HttpGet("csv")]
    public async Task GetBlackoutsCsv([FromQuery] string tz)
    {
        var blackouts = await _blackoutRepository.GetAllBlackoutsAsync();

        if (string.IsNullOrEmpty(tz) == false &&
            int.TryParse(tz, out var offset))
        {
            foreach (var blackout in blackouts)
            {
                blackout.StartTimestamp = blackout.StartTimestamp.AddHours(offset);
                if (blackout.FinishTimestamp is not null)
                {
                    blackout.FinishTimestamp = blackout.FinishTimestamp.Value.AddHours(offset);
                }
            }
        }

        Response.StatusCode = StatusCodes.Status200OK;
        Response.ContentType = "text/csv";

        await using var writer = new StreamWriter(Response.Body);
        await using var csvWriter = new CsvWriter(writer, CultureInfo.InvariantCulture);
        await csvWriter.WriteRecordsAsync(blackouts);
    }
}

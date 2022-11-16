using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BlackoutMonitor.Api.Configuration;
using BlackoutMonitor.Api.Models;
using BlackoutMonitor.Api.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BlackoutMonitor.Api.Services;

public class BeeperManager : BackgroundService
{
    private class StatusRecord
    {
        public BeeperStatus Status { get; set; }
        public DateTime UpdatedOn { get; set; }
    }

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly BeeperExpirationOptions _options;
    private readonly ILogger _logger;

    private readonly Dictionary<string, StatusRecord> _statuses = new();

    public BeeperManager(
        IServiceScopeFactory scopeFactory,
        IOptions<BeeperExpirationOptions> options,
        ILogger<BeeperManager> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options.Value;
        _logger = logger;
    }

    public IList<BeeperStatusDetail> GetBeeperStatuses()
    {
        return _statuses
            .Select(x => new BeeperStatusDetail
            {
                Id = x.Key,
                Status = x.Value.Status,
                UpdatedOn = x.Value.UpdatedOn,
            })
            .OrderBy(x => x.Id)
            .ToList();
    }

    public BeeperStatusDetail GetBeeperStatus(string beeperId)
    {
        var beeperIdLower = beeperId.ToLower();
        if (_statuses.TryGetValue(beeperIdLower, out var record) == false)
        {
            return null;
        }

        return new BeeperStatusDetail
        {
            Id = beeperIdLower,
            Status = record.Status,
            UpdatedOn = record.UpdatedOn,
        };
    }

    public async Task SetBeeperStatusAsync(string beeperId, BeeperStatus status)
    {
        var beeperIdLower = beeperId.ToLower();
        var timestamp = DateTime.UtcNow;

        if (_statuses.TryGetValue(beeperIdLower, out var record) == false)
        {
            record = new StatusRecord
            {
                Status = status,
                UpdatedOn = timestamp,
            };
            _statuses[beeperIdLower] = record;

            _logger.LogInformation("Beeper {beeperId} initialized with status {status}", beeperId, status);
        }
        else
        {
            var oldStatus = record.Status;
            record.Status = status;
            record.UpdatedOn = timestamp;

            if (oldStatus == BeeperStatus.Healthy &&
                status == BeeperStatus.Unhealthy)
            {
                await CreateBlackoutAsync(beeperId, timestamp);
            }
            else if (oldStatus == BeeperStatus.Unhealthy &&
                status == BeeperStatus.Healthy)
            {
                await FinishBlackoutAsync(beeperId, timestamp);
            }
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (stoppingToken.IsCancellationRequested == false)
        {
            await Task.Delay(_options.CheckInterval, stoppingToken);

            _logger.LogDebug("Running beeper expiration check");

            await ExpireBeepersAsync();
        }
    }

    private async Task ExpireBeepersAsync()
    {
        var updateOnThreshold = DateTime.UtcNow - _options.ExpirationTime;
        foreach (var beeper in _statuses)
        {
            if (beeper.Value.Status != BeeperStatus.Healthy)
            {
                continue;
            }

            if (beeper.Value.UpdatedOn <= updateOnThreshold)
            {
                beeper.Value.Status = BeeperStatus.Unhealthy;
                _logger.LogInformation("Beeper {beeperId} is expired", beeper.Key);

                await CreateBlackoutAsync(beeper.Key, DateTime.UtcNow);
            }
        }
    }

    private async Task CreateBlackoutAsync(string beeperId, DateTime startTimestamp)
    {
        _logger.LogInformation("Creating blackout for {beeperId}", beeperId);

        try
        {
            using var scope = _scopeFactory.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<BlackoutRepository>();
            var notifier = scope.ServiceProvider.GetRequiredService<NotificationService>();

            await repository.CreateBlackoutAsync(beeperId, startTimestamp);
            await notifier.NotifyBlackoutStartedAsync(beeperId, startTimestamp);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while creating blackout for {beeperId}", beeperId);
        }
    }

    private async Task FinishBlackoutAsync(string beeperId, DateTime finishTimestamp)
    {
        _logger.LogInformation("Finishing blackout for {beeperId}", beeperId);

        try
        {
            using var scope = _scopeFactory.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<BlackoutRepository>();
            var notifier = scope.ServiceProvider.GetRequiredService<NotificationService>();

            var blackout = await repository.GetLastBlackoutAsync(beeperId);
            if (blackout is null)
            {
                _logger.LogError("Cannot finish blackout from beeper {beeperId}. It is not found in the database.", beeperId);
            }

            if (blackout.FinishTimestamp is not null)
            {
                _logger.LogError("Cannot finish blackout from beeper {beeperId}. It is already finished.", beeperId);
            }

            await repository.UpdateBlackoutAsync(beeperId, blackout.Id, finishTimestamp);
            await notifier.NotifyBlackoutFinishedAsync(beeperId, finishTimestamp);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while finishing blackout for {beeperId}", beeperId);
        }
    }
}

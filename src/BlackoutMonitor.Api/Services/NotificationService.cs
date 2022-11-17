using System;
using System.Threading.Tasks;
using BlackoutMonitor.Api.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot;

namespace BlackoutMonitor.Api.Services;

public class NotificationService
{
    private readonly ITelegramBotClient _botClient;
    private readonly TelegramNotificationOptions _telegramOptions;
    private readonly ILogger _logger;

    public NotificationService(
        ITelegramBotClient botClient,
        IOptions<TelegramNotificationOptions> telegramOptions,
        ILogger<NotificationService> logger)
    {
        _botClient = botClient;
        _telegramOptions = telegramOptions.Value;
        _logger = logger;
    }

    public async Task NotifyBlackoutStartedAsync(string beeperId, DateTime? prevFinishTimestamp, DateTime startTimestamp)
    {
        var channelId = GetBeeperChannelId(beeperId);
        if (string.IsNullOrEmpty(channelId))
        {
            return;
        }

        var message = "- Вимкнули";
        if (prevFinishTimestamp is not null)
        {
            var duration = startTimestamp - prevFinishTimestamp.Value;
            message += $"\nТривалість увімкнення {FormatDuration(duration)}";
        }

        await _botClient.SendTextMessageAsync(channelId, message);
    }

    public async Task NotifyBlackoutFinishedAsync(string beeperId, DateTime startTimestamp, DateTime finishTimestamp)
    {
        var channelId = GetBeeperChannelId(beeperId);
        if (string.IsNullOrEmpty(channelId))
        {
            return;
        }

        var message = "+ Увімкнули";
        var duration = finishTimestamp - startTimestamp;
        message += $"\nТривалість вимкнення {FormatDuration(duration)}";

        await _botClient.SendTextMessageAsync(channelId, message);
    }

    private string GetBeeperChannelId(string beeperId)
    {
        if (_telegramOptions.BeeperChannelIds?.ContainsKey(beeperId.ToLower()) != true)
        {
            _logger.LogWarning("Telegram channel ID is not found for beeper '{beeperId}'", beeperId);
            return null;
        }

        return _telegramOptions.BeeperChannelIds[beeperId];
    }

    private static string FormatDuration(TimeSpan duration)
    {
        return $"{(int)duration.TotalHours:00}:{duration.Minutes:00}";
    }
}

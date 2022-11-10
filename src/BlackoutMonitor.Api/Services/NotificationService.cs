using System;
using System.Threading.Tasks;
using BlackoutMonitor.Api.Configuration;
using Microsoft.Extensions.Options;
using Telegram.Bot;

namespace BlackoutMonitor.Api.Services;

public class NotificationService
{
    private readonly ITelegramBotClient _botClient;
    private readonly TelegramNotificationOptions _telegramOptions;

    public NotificationService(ITelegramBotClient botClient, IOptions<TelegramNotificationOptions> telegramOptions)
    {
        _botClient = botClient;
        _telegramOptions = telegramOptions.Value;
    }

    public async Task NotifyBlackoutStartedAsync(string beeperId, DateTime startTimestamp)
    {
        await _botClient.SendTextMessageAsync(_telegramOptions.ChannelId, "Вимкнули");
    }

    public async Task NotifyBlackoutFinishedAsync(string beeperId, DateTime finishTimestamp)
    {
        await _botClient.SendTextMessageAsync(_telegramOptions.ChannelId, "Увімкнули");
    }
}

using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using Telegram.Bot;
using TelegramToDiscordBot.Interfaces;

namespace TelegramToDiscordBot.Services
{
    public class TelegramService : ITelegramService
    {
        private readonly TelegramBotClient _telegramBotClient;
        private readonly string _telegramToken;

        // Конструктор теперь принимает токен, и мы не извлекаем его из клиента
        public TelegramService(string token)
        {
            _telegramToken = token;
            _telegramBotClient = new TelegramBotClient(token);
        }

        public void StartReceiving(Func<ITelegramBotClient, Update, CancellationToken, Task> handleUpdate,
                                   Func<ITelegramBotClient, Exception, CancellationToken, Task> handleError)
        {
            var receiverOptions = new ReceiverOptions { AllowedUpdates = Array.Empty<UpdateType>() };
            _telegramBotClient.StartReceiving(handleUpdate, handleError, receiverOptions, CancellationToken.None);
        }

        public async Task<Telegram.Bot.Types.File> GetFileAsync(string fileId, CancellationToken cancellationToken) =>
            await _telegramBotClient.GetFile(fileId, cancellationToken);

        public async Task<Stream> DownloadFileAsync(string filePath, CancellationToken cancellationToken)
        {
            var fileUrl = $"https://api.telegram.org/file/bot{_telegramToken}/{filePath}";
            using var httpClient = new HttpClient();
            return await httpClient.GetStreamAsync(fileUrl);
        }
    }

}

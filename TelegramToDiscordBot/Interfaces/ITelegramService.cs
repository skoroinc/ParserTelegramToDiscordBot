using System.Threading;
using System.Threading.Tasks;
using System.IO;
using Telegram.Bot.Types;
using Telegram.Bot;

namespace TelegramToDiscordBot.Interfaces
{
    public interface ITelegramService
    {
        void StartReceiving(Func<ITelegramBotClient, Update, CancellationToken, Task> handleUpdate,
                            Func<ITelegramBotClient, Exception, CancellationToken, Task> handleError);

        Task<Telegram.Bot.Types.File> GetFileAsync(string fileId, CancellationToken cancellationToken);
        Task<Stream> DownloadFileAsync(string filePath, CancellationToken cancellationToken);
    }
}

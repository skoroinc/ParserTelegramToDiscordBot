using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramToDiscordBot.Models;





using TelegramMessageType = Telegram.Bot.Types.Enums.MessageType;


namespace TelegramToDiscordBot
{
    // Основной класс для запуска бота
    class StartBot
    {


        static async Task Main(string[] args)
        {
            try
            {
                // Загрузка конфигурации
                var config = ConfigurationLoader.LoadConfig();

                // Инициализация ботов
                var telegramHandler = new TelegramHandler(config.TelegramToken);
                var discordHandler = new DiscordHandler(config.DiscordToken, config.DiscordChannelId);
                var messageProcessor = new MessageProcessor(discordHandler, telegramHandler);

                await discordHandler.ConnectAsync();
                telegramHandler.StartReceiving(
                    messageProcessor.ProcessTelegramMessageAsync,
                    messageProcessor.HandleErrorAsync
                );

                Console.WriteLine("Бот запущен. Нажмите Enter для выхода.");
                Console.ReadLine();

                await discordHandler.DisconnectAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }
        }

        // Конфигурация бота
        public class BotConfig
        {
            public string TelegramToken { get; set; } = string.Empty;
            public string DiscordToken { get; set; } = string.Empty;
            public ulong DiscordChannelId { get; set; }
            public const int MaxFileSize = 50 * 1024 * 1024; // 50 MB // Максимальный размер файла
        }

        // Класс для работы с Telegram
        public class TelegramHandler
        {
            private readonly TelegramBotClient _telegramBotClient;
            private readonly string _telegramToken;

            public TelegramHandler(string token)
            {
                _telegramToken = token; // Сохраняем токен для использования в запросах
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
                // Используем токен, сохранённый при инициализации
                var fileUrl = $"https://api.telegram.org/file/bot{_telegramToken}/{filePath}";
                using var httpClient = new HttpClient();
                return await httpClient.GetStreamAsync(fileUrl);
            }
        }

        // Класс для работы с Discord
        public class DiscordHandler
        {
            private readonly DiscordClient _discordClient;
            private readonly ulong _channelId;

            public DiscordHandler(string token, ulong channelId)
            {
                _discordClient = new DiscordClient(new DiscordConfiguration
                {
                    Token = token,
                    TokenType = TokenType.Bot
                });
                _channelId = channelId;
            }

            public async Task ConnectAsync() => await _discordClient.ConnectAsync();
            public async Task DisconnectAsync() => await _discordClient.DisconnectAsync();

            public async Task SendMessageAsync(DiscordMessageBuilder messageBuilder)
            {
                var channel = await _discordClient.GetChannelAsync(_channelId);
                await channel.SendMessageAsync(messageBuilder);
            }
        }

        // Класс для обработки сообщений
        public class MessageProcessor
        {
            private readonly DiscordHandler _discordHandler;
            private readonly TelegramHandler _telegramHandler;

            public MessageProcessor(DiscordHandler discordHandler, TelegramHandler telegramHandler)
            {
                _discordHandler = discordHandler;
                _telegramHandler = telegramHandler;
            }

            public async Task ProcessTelegramMessageAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
            {
                if (update.Message == null) return;

                var message = update.Message;

                try
                {
                    // Проверка текста без медиа
                    if (!string.IsNullOrEmpty(message.Text) && message.Type == TelegramMessageType.Text)
                    {
                        if (message.Text.Length > 2000)
                        {
                            Console.WriteLine("Текстовое сообщение превышает 2000 символов и не будет отправлено.");
                            return;
                        }

                        string formattedText = FormatTelegramHtmlToMarkdown(message.Text, message.Entities);
                        var textBuilder = new DiscordMessageBuilder().WithContent(formattedText);
                        await _discordHandler.SendMessageAsync(textBuilder);
                        Console.WriteLine("Текстовое сообщение отправлено в Discord.");
                    }

                    // Собираем медиафайлы
                    var mediaFiles = new List<FileDetails>();
                    switch (message.Type)
                    {
                        case TelegramMessageType.Photo:
                        case TelegramMessageType.Video:
                        case TelegramMessageType.Document:
                            await HandleMediaMessage(message, mediaFiles, cancellationToken);
                            break;

                        default:
                            Console.WriteLine("Тип сообщения не поддерживается.");
                            return;
                    }

                    // Если есть медиафайлы, отправляем их
                    if (mediaFiles.Any())
                    {
                        await SendMediaFiles(message, mediaFiles.AsReadOnly(), cancellationToken);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при обработке сообщения: {ex.Message}");
                }
            }

            private async Task HandleMediaMessage(Message message, List<FileDetails> mediaFiles, CancellationToken cancellationToken)
            {
                string? caption = message.Caption;

                // Проверка длины подписи
                if (!string.IsNullOrEmpty(caption) && caption.Length > 2000)
                {
                    Console.WriteLine("Подпись к медиафайлу превышает 2000 символов и не будет отправлена.");
                    caption = null; // Удаляем подпись, чтобы не отправлять её
                }

                switch (message.Type)
                {
                    case TelegramMessageType.Photo:
                        var photoFileId = message.Photo!.Last().FileId;
                        await AddMediaFile(photoFileId, "photo.jpg", "image/jpeg", mediaFiles, cancellationToken);
                        break;

                    case TelegramMessageType.Video:
                        var videoFileId = message.Video!.FileId;
                        await AddMediaFile(videoFileId, "video.mp4", "video/mp4", mediaFiles, cancellationToken);
                        break;

                    case TelegramMessageType.Document:
                        var documentFileId = message.Document!.FileId;
                        await AddMediaFile(documentFileId, "document", message.Document.MimeType ?? "application/octet-stream", mediaFiles, cancellationToken);
                        break;
                }

                Console.WriteLine("Медиафайлы собраны для отправки.");
            }

            private async Task AddMediaFile(string fileId, string fileName, string fileType, List<FileDetails> mediaFiles, CancellationToken cancellationToken)
            {
                var fileInfo = await _telegramHandler.GetFileAsync(fileId, cancellationToken);

                // Проверяем размер файла
                if (fileInfo.FileSize > BotConfig.MaxFileSize)
                {
                    Console.WriteLine($"Файл {fileName} слишком большой ({fileInfo.FileSize / (1024 * 1024)} МБ) и не будет отправлен.");
                    return;
                }

                var fileStream = await _telegramHandler.DownloadFileAsync(fileInfo.FilePath!, cancellationToken);
                mediaFiles.Add(new FileDetails
                {
                    FileId = fileId,
                    FileName = fileName,
                    FileType = fileType,
                    FileStream = fileStream
                });
            }

            private async Task SendMediaFiles(Message telegramMessage, IReadOnlyCollection<FileDetails> fileDetailsList, CancellationToken cancellationToken)
            {
                var discordMessageBuilder = new DiscordMessageBuilder();

                if (!string.IsNullOrEmpty(telegramMessage.Caption))
                {
                    string formattedCaption = FormatTelegramHtmlToMarkdown(telegramMessage.Caption, telegramMessage.CaptionEntities);
                    discordMessageBuilder.WithContent(formattedCaption);
                }

                var fileStreams = new List<(string FileName, Stream FileStream)>();

                try
                {
                    foreach (var fileDetails in fileDetailsList)
                    {
                        fileStreams.Add((fileDetails.FileName, fileDetails.FileStream));
                    }

                    if (fileStreams.Any())
                    {
                        discordMessageBuilder.AddFiles(fileStreams.ToDictionary(fs => fs.FileName, fs => fs.FileStream));
                        await _discordHandler.SendMessageAsync(discordMessageBuilder);
                        Console.WriteLine("Медиафайлы отправлены в Discord.");
                    }
                }
                finally
                {
                    foreach (var (_, stream) in fileStreams)
                    {
                        stream.Dispose();
                    }
                }
            }


            private static string FormatTelegramHtmlToMarkdown(string text, MessageEntity[]? entities)
            {
                if (entities == null || entities.Length == 0) return text;

                var sb = new StringBuilder();
                int lastIndex = 0;

                foreach (var entity in entities.OrderBy(e => e.Offset))
                {
                    if (lastIndex < entity.Offset)
                        sb.Append(text.Substring(lastIndex, entity.Offset - lastIndex));

                    string content = text.Substring(entity.Offset, entity.Length);
                    sb.Append(entity.Type switch
                    {
                        MessageEntityType.Bold => $"**{content}**",
                        MessageEntityType.Italic => $"*{content}*",
                        MessageEntityType.TextLink => $"[{content}]({entity.Url})",
                        MessageEntityType.Url => $"[{content}]({content})",
                        _ => content
                    });

                    lastIndex = entity.Offset + entity.Length;
                }

                if (lastIndex < text.Length)
                    sb.Append(text.Substring(lastIndex));

                return sb.ToString();
            }

            public Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
            {
                Console.WriteLine($"Ошибка: {exception.Message}");
                return Task.CompletedTask;
            }
        }
    }
}
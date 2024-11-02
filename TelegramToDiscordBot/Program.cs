using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using System.Text;

namespace TelegramToDiscordBot
{
    class Program
    {
        // Объявляем клиентов для Discord и Telegram
        private static DiscordClient discordClient = null!;
        private static TelegramBotClient telegramBotClient = null!;
        private static string discordChannelId = "ВАШ ID КАНАЛА DISCORD"; // Укажите ваш ID канала Discord
        private static string telegramBotToken = "ВАШ ТОКЕН БОТА TG"; // Укажите ваш токен бота Telegram
        private const int MaxFileSize = 50 * 1024 * 1024; // Максимальный размер файла - 50 МБ

        static async Task Main(string[] args)
        {
            try
            {
                // Инициализация клиента Discord
                discordClient = new DiscordClient(new DiscordConfiguration
                {
                    Token = "ТОКЕН БОТА DISCORD", // Укажите ваш токен бота Discord
                    TokenType = TokenType.Bot
                });
                await discordClient.ConnectAsync();

                // Инициализация клиента Telegram
                telegramBotClient = new TelegramBotClient(telegramBotToken);

                // Настройка обработчика получения обновлений
                var receiverOptions = new ReceiverOptions
                {
                    AllowedUpdates = Array.Empty<UpdateType>() // Получение всех типов обновлений
                };

                // Запуск получения сообщений
                telegramBotClient.StartReceiving(
                    HandleUpdateAsync, // Обработчик сообщений
                    HandleErrorAsync,  // Обработчик ошибок
                    receiverOptions,
                    CancellationToken.None
                );

                Console.WriteLine("Бот запущен. Нажмите Enter для выхода.");
                Console.ReadLine();

                // Завершение программы, получатель остановится автоматически
                await discordClient.DisconnectAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Произошла ошибка при запуске бота: {ex.Message}");
            }
        }

        // Обработчик обновлений от Telegram
        private static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Message == null) return; // Проверка, что сообщение не пустое

            try
            {
                var message = update.Message;
                var discordChannel = await discordClient.GetChannelAsync(ulong.Parse(discordChannelId));
                var builder = new DiscordMessageBuilder();

                // Обработка пересланных сообщений
                //if (message.ForwardFromChat != null || message.ForwardFrom != null)
                //{
                //   string forwardInfo = message.ForwardFromChat?.Title ?? message.ForwardFrom?.FirstName ?? "Пользователь";
                //   builder.WithContent($"Сообщение переслано из: {forwardInfo}");
                //}

                // Переменная для хранения текста сообщения
                string originalText = message.Text ?? string.Empty;

                // Удаляем строки, содержащие "КиберТопор"
                if (!string.IsNullOrEmpty(originalText))
                {
                    originalText = RemoveLinesContaining(originalText, "КиберТопор");
                    Console.WriteLine($"Очищенный текст: {originalText}"); // Для отладки
                    if (!string.IsNullOrEmpty(originalText))
                    {
                        builder.WithContent(originalText);
                    }
                }

                // Обработка различных типов сообщений
                switch (message.Type)
                {
                    case Telegram.Bot.Types.Enums.MessageType.Photo:
                        await HandleMediaWithCaption(builder, message.Photo.Last().FileId, message.Caption, message.CaptionEntities, "photo.jpg", "image/jpeg", cancellationToken);
                        break;

                    case Telegram.Bot.Types.Enums.MessageType.Video:
                        await HandleMediaWithCaption(builder, message.Video.FileId, message.Caption, message.CaptionEntities, "video.mp4", "video/mp4", cancellationToken);
                        break;

                    // Обработка текстовых сообщений
                    case Telegram.Bot.Types.Enums.MessageType.Text:
                        // Если сообщение не пересланное, просто добавляем текст
                        if (message.ForwardFromChat == null && message.ForwardFrom == null)
                        {
                            string formattedText = FormatTelegramHtmlToMarkdown(message.Text, message.Entities);
                            builder.WithContent(formattedText);
                        }
                        break;

                    default:
                        Console.WriteLine("Тип сообщения не поддерживается");
                        return;
                }


                // Проверка содержимого и отправка сообщения в Discord
                if (!string.IsNullOrEmpty(builder.Content) || builder.Files.Any())
                {
                    await discordChannel.SendMessageAsync(builder);
                }
                //if (builder.Content != null || builder.Files.Any())
                //{
                //    await discordChannel.SendMessageAsync(builder);
                //}

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при обработке сообщения: {ex.Message}");
            }
        }

        // Метод для удаления строк, содержащих заданное слово
        private static string RemoveLinesContaining(string input, string keyword)
        {
            var lines = input.Split(new[] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            var cleanedLines = lines.Where(line => !line.Contains(keyword, StringComparison.OrdinalIgnoreCase));
            return string.Join("\n", cleanedLines).Trim();
        }

        // Метод для обработки медиа с подписью
        private static async Task HandleMediaWithCaption(DiscordMessageBuilder builder, string fileId, string? caption, MessageEntity[]? captionEntities, string fileName, string fileType, CancellationToken cancellationToken)
        {
            try
            {
                if (!string.IsNullOrEmpty(caption))
                {
                    string formattedCaption = FormatTelegramHtmlToMarkdown(caption, captionEntities);
                    builder.WithContent(formattedCaption);
                }
                await AttachFileToBuilder(builder, fileId, fileName, fileType, cancellationToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при обработке медиа: {ex.Message}");
            }
        }

        // Метод для присоединения файла к Discord
        private static async Task AttachFileToBuilder(DiscordMessageBuilder builder, string fileId, string fileName, string fileType, CancellationToken cancellationToken)
        {
            try
            {
                var fileInfo = await telegramBotClient.GetFileAsync(fileId, cancellationToken);

                if (fileInfo.FileSize > MaxFileSize)
                {
                    Console.WriteLine($"Файл {fileName} слишком большой для загрузки из Telegram (размер: {fileInfo.FileSize / (1024 * 1024)} МБ)");
                    return;
                }

                var fileUrl = $"https://api.telegram.org/file/bot{telegramBotToken}/{fileInfo.FilePath}";

                using var client = new HttpClient();
                var fileData = await client.GetByteArrayAsync(fileUrl);

                if (fileData.Length > MaxFileSize)
                {
                    Console.WriteLine($"Файл {fileName} слишком большой для отправки в Discord (размер: {fileData.Length / (1024 * 1024)} МБ)");
                    return;
                }

                builder.AddFile(fileName, new MemoryStream(fileData));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при скачивании или добавлении файла: {ex.Message} \nПропуск...Жду следующего поста");
            }
        }

        private static string FormatTelegramHtmlToMarkdown(string text, MessageEntity[]? entities)
        {
            if (entities == null || entities.Length == 0) return text;

            var sb = new StringBuilder();
            int lastIndex = 0;

            foreach (var entity in entities.OrderBy(e => e.Offset))
            {
                // Проверка на валидные индексы
                if (lastIndex < entity.Offset)
                {
                    sb.Append(text.Substring(lastIndex, entity.Offset - lastIndex));
                }

                switch (entity.Type)
                {
                    case MessageEntityType.TextLink when entity.Url != null:
                        string linkText = text.Substring(entity.Offset, entity.Length);
                        sb.Append($"[{linkText}]({entity.Url})");
                        break;

                    case MessageEntityType.Url:
                        string url = text.Substring(entity.Offset, entity.Length);
                        sb.Append($"[{url}]({url})");
                        break;

                    case MessageEntityType.Bold:
                        if (entity.Offset + entity.Length <= text.Length)
                        {
                            string boldText = text.Substring(entity.Offset, entity.Length);
                            sb.Append($"**{boldText}**");
                        }
                        break;

                    case MessageEntityType.Italic:
                        if (entity.Offset + entity.Length <= text.Length)
                        {
                            string italicText = text.Substring(entity.Offset, entity.Length);
                            sb.Append($"*{italicText}*");
                        }
                        break;

                    // Другие типы...
                    default:
                        sb.Append(text.Substring(entity.Offset, entity.Length));
                        break;
                }

                lastIndex = entity.Offset + entity.Length;
            }

            if (lastIndex < text.Length)
            {
                sb.Append(text.Substring(lastIndex));
            }

            return sb.ToString();
        }

        // Обработчик ошибок
        private static Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            Console.WriteLine($"Ошибка: {exception.Message}");
            return Task.CompletedTask;
        }
    }
}
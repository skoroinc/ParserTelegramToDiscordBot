using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot;
using TelegramMessageType = Telegram.Bot.Types.Enums.MessageType;
using TelegramToDiscordBot.Interfaces;
using TelegramToDiscordBot.Models;
using TelegramToDiscordBot.Configuration;
using TelegramToDiscordBot.KeyWords;

namespace TelegramToDiscordBot.Logic
{
    public class MessageProcessor
    {
        private readonly IDiscordService _discordService;
        private readonly ITelegramService _telegramService;
        private readonly IMessageFilter _messageFilter;
        private readonly IMessageFormatter _messageFormatter;

        public MessageProcessor(IDiscordService discordService, ITelegramService telegramService,
                                IMessageFilter messageFilter, IMessageFormatter messageFormatter)
        {
            _discordService = discordService;
            _telegramService = telegramService;
            _messageFilter = messageFilter;
            _messageFormatter = messageFormatter;
        }

        public async Task ProcessTelegramMessageAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Message == null) return;

            var message = update.Message;

            try
            {

                // Обработка команд
                if (message.Text != null &&
                    (message.Text.StartsWith("/add_keyword") ||
                     message.Text.StartsWith("/remove_keyword") ||
                     message.Text == "/list_keywords"))
                {
                    await KeywordCommandHandler.HandleCommandAsync(botClient, message);
                    return; // Команда обработана, прекращаем выполнение
                }

                // Если сообщение содержит медиа
                if (message.Type == TelegramMessageType.Photo ||
                    message.Type == TelegramMessageType.Video ||
                    message.Type == TelegramMessageType.Document)
                {
                    var mediaFiles = new List<FileDetails>();
                    await HandleMediaMessage(message, mediaFiles, cancellationToken);

                    // Если есть медиафайлы, отправляем их
                    if (mediaFiles.Any())
                    {
                        await SendMediaFiles(message, mediaFiles.AsReadOnly(), cancellationToken);
                        Console.WriteLine("Медиафайлы отправлены.");
                    }

                    return; // Завершаем обработку, чтобы не обрабатывать текст отдельно
                }

                // Если сообщение текстовое
                if (!string.IsNullOrEmpty(message.Text))
                {
                    // Фильтруем текст перед отправкой
                    string filteredText = _messageFilter.Filter(message.Text);

                    // Проверяем, не стало ли сообщение пустым после фильтрации
                    if (string.IsNullOrWhiteSpace(filteredText))
                    {
                        Console.WriteLine("Сообщение удалено после фильтрации.");
                        return; // Не отправляем пустое сообщение
                    }

                    // Форматируем текст и отправляем в Discord
                    string formattedText = _messageFormatter.Format(filteredText, message.Entities);

                    var textBuilder = new DiscordMessageBuilder().WithContent(formattedText);
                    await _discordService.SendMessageAsync(textBuilder);

                    Console.WriteLine("Текстовое сообщение отправлено в Discord.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при обработке сообщения: {ex.Message}");
            }
        }

        // Обработка медиафайлов
        private async Task HandleMediaMessage(Message message, List<FileDetails> mediaFiles, CancellationToken cancellationToken)
        {
            string? caption = message.Caption;

            // Проверка длины подписи
            if (!string.IsNullOrEmpty(caption) && caption.Length > 2000)
            {
                Console.WriteLine("Подпись к медиафайлу превышает 2000 символов и не будет отправлена.");
                caption = null; // Удаляем подпись, чтобы не отправлять её
            }

            // Фильтруем подпись
            if (!string.IsNullOrEmpty(caption))
            {
                caption = _messageFilter.Filter(caption);
                if (string.IsNullOrWhiteSpace(caption))
                {
                    Console.WriteLine("Подпись удалена после фильтрации.");
                    caption = null; // Удаляем подпись
                }
            }

            // Обработка медиафайлов
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

                default:
                    Console.WriteLine("Тип сообщения не поддерживается.");
                    return;
            }

            Console.WriteLine("Медиафайлы собраны для отправки.");
        }

        // Добавление медиафайлов
        private async Task AddMediaFile(string fileId, string fileName, string fileType, List<FileDetails> mediaFiles, CancellationToken cancellationToken)
        {
            var fileInfo = await _telegramService.GetFileAsync(fileId, cancellationToken);

            // Проверяем размер файла
            if (fileInfo.FileSize > BotConfig.MaxFileSize)
            {
                Console.WriteLine($"Файл {fileName} слишком большой ({fileInfo.FileSize / (1024 * 1024)} МБ) и не будет отправлен.");
                return;
            }

            var fileStream = await _telegramService.DownloadFileAsync(fileInfo.FilePath!, cancellationToken);
            mediaFiles.Add(new FileDetails
            {
                FileId = fileId,
                FileName = fileName,
                FileType = fileType,
                FileStream = fileStream
            });

            Console.WriteLine($"Медиафайл {fileName} добавлен в список.");
        }

        // Отправка медиафайлов
        private async Task SendMediaFiles(Message telegramMessage, IReadOnlyCollection<FileDetails> fileDetailsList, CancellationToken cancellationToken)
        {
            var discordMessageBuilder = new DiscordMessageBuilder();

            if (!string.IsNullOrEmpty(telegramMessage.Caption))
            {
                string filteredCaption = _messageFilter.Filter(telegramMessage.Caption);
                if (!string.IsNullOrWhiteSpace(filteredCaption))
                {
                    string formattedCaption = _messageFormatter.Format(filteredCaption, telegramMessage.CaptionEntities);
                    discordMessageBuilder.WithContent(formattedCaption);
                }
            }

            var fileStreams = new List<(string FileName, Stream FileStream)>();

            try
            {
                foreach (var fileDetails in fileDetailsList)
                {
                    fileStreams.Add((fileDetails.FileName, fileDetails.FileStream));
                    Console.WriteLine($"Добавлен файл {fileDetails.FileName} для отправки.");
                }

                if (fileStreams.Any())
                {
                    discordMessageBuilder.AddFiles(fileStreams.ToDictionary(fs => fs.FileName, fs => fs.FileStream));
                    await _discordService.SendMessageAsync(discordMessageBuilder);
                    Console.WriteLine("Медиафайлы отправлены в Discord.");
                }
                else
                {
                    Console.WriteLine("Нет медиафайлов для отправки.");
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

        // Метод для обработки ошибок
        public async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {

            Console.WriteLine($"Произошла ошибка при обработке сообщения: {exception.Message}");
            await Task.CompletedTask;
        }

    }

}

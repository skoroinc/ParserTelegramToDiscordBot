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
using TelegramToDiscordBot.Utilities;
using TelegramToDiscordBot.Services;
using TelegramToDiscordBot.Logic;



namespace TelegramToDiscordBot
{

    public class StartBot
    {
        static async Task Main(string[] args)
        {
            try
            {
                // Инициализация базы данных
                DatabaseInitializer.Initialize();

                // Загрузка конфигурации
                var config = ConfigurationLoader.LoadConfig();

                // Создаем экземпляры сервисов
                var telegramHandler = new TelegramService(config.TelegramToken);
                var discordHandler = new DiscordService(config.DiscordToken, config.DiscordChannelId);
                var messageFilter = new MessageFilter();  
                var messageFormatter = new MessageFormatter();  
                var messageProcessor = new MessageProcessor(discordHandler, telegramHandler, new MessageFilter(), new MessageFormatter());


                // Запуск сервисов
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
    }
}
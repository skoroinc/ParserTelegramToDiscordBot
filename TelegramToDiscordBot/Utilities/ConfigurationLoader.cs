using Microsoft.Extensions.Configuration;
using System.IO;
using TelegramToDiscordBot.Configuration;

namespace TelegramToDiscordBot.Utilities
{
    public static class ConfigurationLoader
    {
        public static BotConfig LoadConfig(string configFileName = "appsettings.json")
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory()) // Указываем текущую директорию
                .AddJsonFile("Configuration/appsettings.json", optional: false, reloadOnChange: true) // Подключаем файл
                .Build();

            // Проверим, что конфигурация загружена корректно
            Console.WriteLine($"Telegram Token: {configuration["TelegramToken"]}");
            Console.WriteLine($"Discord Token: {configuration["DiscordToken"]}");

            var telegramToken = configuration["TelegramToken"];
            var discordToken = configuration["DiscordToken"];
            var discordChannelId = ulong.Parse(configuration["DiscordChannelId"] ?? "0");

            if (string.IsNullOrEmpty(telegramToken) || string.IsNullOrEmpty(discordToken))
            {
                throw new InvalidOperationException("Токены не найдены в конфигурации.");
            }

            return new BotConfig
            {
                TelegramToken = telegramToken,
                DiscordToken = discordToken,
                DiscordChannelId = discordChannelId
            };
        }

    }
}

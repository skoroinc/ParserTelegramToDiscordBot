using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace TelegramToDiscordBot.KeyWords
{
    public static class KeywordCommandHandler
    {
        public static async Task HandleCommandAsync(ITelegramBotClient botClient, Message message)
        {
            if (message.Text.StartsWith("/add_keyword"))
            {
                var keyword = message.Text.Replace("/add_keyword", "").Trim();
                if (!string.IsNullOrWhiteSpace(keyword))
                {
                    KeywordRepository.AddKeyword(keyword);
                    await botClient.SendMessage(
                        chatId: message.Chat.Id,
                        text: $"Ключевое слово '{keyword}' успешно добавлено."
                    );
                }
                else
                {
                    await botClient.SendMessage(
                        chatId: message.Chat.Id,
                        text: "Ошибка: ключевое слово не может быть пустым."
                    );
                }
            }
            else if (message.Text.StartsWith("/remove_keyword"))
            {
                var keyword = message.Text.Replace("/remove_keyword", "").Trim();
                if (!string.IsNullOrWhiteSpace(keyword))
                {
                    KeywordRepository.RemoveKeyword(keyword);
                    await botClient.SendMessage(
                        chatId: message.Chat.Id,
                        text: $"Ключевое слово '{keyword}' успешно удалено."
                    );
                }
                else
                {
                    await botClient.SendMessage(
                        chatId: message.Chat.Id,
                        text: "Ошибка: ключевое слово не может быть пустым."
                    );
                }
            }
            else if (message.Text == "/list_keywords")
            {
                var keywords = KeywordRepository.GetKeywords();
                string response = keywords.Count > 0
                    ? "Список ключевых слов:\n" + string.Join("\n", keywords)
                    : "Список ключевых слов пуст.";

                await botClient.SendMessage(
                    chatId: message.Chat.Id,
                    text: response
                );
            }
        }
    }
}

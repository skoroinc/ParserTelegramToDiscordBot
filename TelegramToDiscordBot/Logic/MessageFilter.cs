using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TelegramToDiscordBot.Interfaces;
using TelegramToDiscordBot.KeyWords;

namespace TelegramToDiscordBot.Logic
{
    public class MessageFilter : IMessageFilter
    {
        public string Filter(string message)
        {
            var keywords = KeywordRepository.GetKeywords(); // Получаем ключевые слова из базы данных
            foreach (var keyword in keywords)
            {
                var regex = new Regex(@"\b" + Regex.Escape(keyword) + @"\b", RegexOptions.IgnoreCase);
                message = regex.Replace(message, string.Empty); // Удаляем ключевое слово из текста
            }
            return Regex.Replace(message, @"\s+", " ").Trim(); // Убираем лишние пробелы
        }
    }
}

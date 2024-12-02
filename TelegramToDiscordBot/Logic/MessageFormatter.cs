using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using TelegramToDiscordBot.Interfaces;

namespace TelegramToDiscordBot.Logic
{
    public class MessageFormatter : IMessageFormatter
    {
        public string Format(string text, MessageEntity[]? entities)
        {
            if (entities == null || entities.Length == 0) return text;

            var sb = new StringBuilder();
            int lastIndex = 0;

            foreach (var entity in entities.OrderBy(e => e.Offset))
            {
                if (entity.Offset < 0 || entity.Offset >= text.Length || entity.Offset + entity.Length > text.Length)
                    continue;

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
    }
}

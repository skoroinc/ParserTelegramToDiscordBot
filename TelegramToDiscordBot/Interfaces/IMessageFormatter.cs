using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace TelegramToDiscordBot.Interfaces
{
    public interface IMessageFormatter
    {
        string Format(string text, MessageEntity[]? entities);
    }
}

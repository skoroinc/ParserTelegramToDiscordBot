using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramToDiscordBot.Interfaces
{
    public interface IMessageFilter
    {
        string Filter(string message);
    }
}

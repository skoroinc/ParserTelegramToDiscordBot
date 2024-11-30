using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace TelegramToDiscordBot
{
    public class BotConfig
    {
        public string TelegramToken { get; set; } = string.Empty;
        public string DiscordToken { get; set; } = string.Empty;
        public ulong DiscordChannelId { get; set; }
        public const int MaxFileSize = 50 * 1024 * 1024; // 50 MB
    }
}

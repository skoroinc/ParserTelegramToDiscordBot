using DSharpPlus.Entities;
using System.Threading.Tasks;

namespace TelegramToDiscordBot.Interfaces
{
    public interface IDiscordService
    {
        Task ConnectAsync();
        Task DisconnectAsync();
        Task SendMessageAsync(DiscordMessageBuilder messageBuilder);
    }
}

using DSharpPlus.Entities;
using DSharpPlus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelegramToDiscordBot.Interfaces;

namespace TelegramToDiscordBot.Services
{
    public class DiscordService : IDiscordService
    {
        private readonly DiscordClient _discordClient;
        private readonly ulong _channelId;

        public DiscordService(string token, ulong channelId)
        {
            _discordClient = new DiscordClient(new DiscordConfiguration
            {
                Token = token,
                TokenType = TokenType.Bot
            });
            _channelId = channelId;
        }

        public async Task ConnectAsync() => await _discordClient.ConnectAsync();
        public async Task DisconnectAsync() => await _discordClient.DisconnectAsync();

        public async Task SendMessageAsync(DiscordMessageBuilder messageBuilder)
        {
            var channel = await _discordClient.GetChannelAsync(_channelId);
            await channel.SendMessageAsync(messageBuilder);
        }
    }
}

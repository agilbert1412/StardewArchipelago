using System.Threading.Tasks;
using Discord.WebSocket;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using StardewArchipelago.ViewerEventsModule.EventsExecution;

namespace StardewArchipelago.ViewerEventsModule.DiscordIntegration
{
    internal class DiscordBot
    {
        private IBotCommunicator _discord;
        private DiscordModule _discordModule;

        public DiscordBot(ILogger logger, ViewerEventsExecutor eventsExecutor)
        {
            _discord = new DiscordWrapper();
            _discordModule = new DiscordModule(logger, _discord, eventsExecutor);
        }

        public async Task InitializeAsync(string token)
        {
            _discord.InitializeClient();
            _discord.InitializeLog();

            _discord.Login(token);
            _discord.Start(HandleCommandAsync);
        }

        private async Task HandleCommandAsync(SocketMessage arg)
        {
            // Bail out if it's a System Message.
            if (arg is not SocketUserMessage msg)
            {
                return;
            }

            // We don't want the bot to respond to itself
            if (msg.Author.Id == _discord.MyId)
            {
                return;
            }

            await _discordModule.ExecuteViewerCommand(msg);
        }
    }
}

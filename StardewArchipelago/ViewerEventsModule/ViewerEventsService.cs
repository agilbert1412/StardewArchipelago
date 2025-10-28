using System;
using System.Threading.Tasks;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using StardewArchipelago.ViewerEventsModule.DiscordIntegration;
using StardewArchipelago.ViewerEventsModule.EventsExecution;

namespace StardewArchipelago.ViewerEventsModule
{
    public class ViewerEventsService
    {
        private ILogger _logger;
        private ViewerEventsConfig _config;
        private ViewerEventsExecutor _eventsExecutor;

        public ViewerEventsService(ILogger logger, string configFilePath, ViewerEventsExecutor eventsExecutor)
        {
            _logger = logger;
            _eventsExecutor = eventsExecutor;

            if (ViewerEventsConfig.TryReadConfig(configFilePath, out var config))
            {
                _logger.LogInfo($"Successfully read config file for user events integration.");
                _config = config;
            }
            else
            {
                _logger.LogInfo($"No config file found for Viewer Events Integration.");
                _config = new ViewerEventsConfig();
            }
        }

        public async Task Initialize()
        {
            await InitializeDiscordIntegration();
            await InitializeTwitchIntegration();
        }

        private async Task InitializeDiscordIntegration()
        {
            if (string.IsNullOrWhiteSpace(_config.DiscordToken))
            {
                return;
            }

            try
            {
                _logger.LogInfo($"Initializing Discord Integration...");
                var bot = new DiscordBot(_logger, _eventsExecutor);
                await bot.InitializeAsync(_config.DiscordToken);

                _logger.LogInfo($"Discord Integration Initialized!");
                await Task.Delay(-1);
            }
            catch (Exception e)
            {
                _logger.LogError($"Could not initialize Discord Integration.", e);
            }
        }

        private async Task InitializeTwitchIntegration()
        {
            if (string.IsNullOrWhiteSpace(_config.TwitchToken))
            {
                return;
            }


            try
            {
                throw new NotImplementedException($"Twitch Integration is not implemented at the moment");

                //_logger.LogInfo($"Initializing Twitch Integration...");
                //var bot = new TwitchBot();
                //await bot.InitializeAsync();

                //_logger.LogInfo($"Twitch Integration Initialized!");
                //await Task.Delay(-1);
            }
            catch (Exception e)
            {
                _logger.LogError($"Could not initialize Twitch Integration.", e);
            }
        }
    }
}

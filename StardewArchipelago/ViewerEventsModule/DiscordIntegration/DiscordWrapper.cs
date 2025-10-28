using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace StardewArchipelago.ViewerEventsModule.DiscordIntegration
{
    internal class DiscordWrapper : IBotCommunicator
    {
        private ILogger _logger;
        private DiscordSocketClient _client;
        private CommandService _commands;
        private IServiceProvider _services;

        private static readonly DiscordSocketConfig SocketConfig = new DiscordSocketConfig
        {
            // How much logging do you want to see?
            LogLevel = LogSeverity.Info,

            // If you or another service needs to do anything with messages
            // (eg. checking Reactions, checking the content of edited/deleted messages),
            // you must set the MessageCacheSize. You may adjust the number as needed.
            //MessageCacheSize = 50,

            // If your platform doesn't have native WebSockets,
            // add Discord.Net.Providers.WS4Net from NuGet,
            // add the `using` at the top, and uncomment this line:
            //WebSocketProvider = WS4NetProvider.Instance
            GatewayIntents = GatewayIntents.All & ~(GatewayIntents.GuildPresences | GatewayIntents.GuildInvites | GatewayIntents.GuildScheduledEvents),
        };

        private static readonly CommandServiceConfig CommandConfig = new CommandServiceConfig
        {
            // Again, log level:
            LogLevel = LogSeverity.Info,

            // There's a few more properties you can set,
            // for example, case-insensitive commands.
            CaseSensitiveCommands = false,
        };

        public ulong MyId => _client.CurrentUser.Id;

        public DiscordWrapper()
        {
        }

        public void InitializeClient()
        {
            _client = new DiscordSocketClient(SocketConfig);
            _commands = new CommandService(CommandConfig);

            // Setup your DI container.
            _services = ConfigureServices();
        }

        public void InitializeLog()
        {
            _client.Log += Log;
            _commands.Log += Log;
        }

        public async void Login(string token)
        {
            await _client.LoginAsync(TokenType.Bot, token);
        }

        public async void Start(Func<SocketMessage, Task> messageReceivedFunction)
        {
            await InitCommands(messageReceivedFunction);
            await _client.StartAsync();
        }

        public void SendMessage(ulong channelId, string text)
        {
            SendMessageAsync(channelId, text);
        }

        public async Task SendMessageAsync(ulong channelId, string text)
        {
            var channel = _client.GetChannel(channelId);

            if (!(channel is ISocketMessageChannel messageChannel))
            {
                return;
            }

            await SendMessageAsync(messageChannel, text);
        }

        public async Task SendMessageAsync(ISocketMessageChannel channel, string text)
        {
            await channel.SendMessageAsync(text);
        }

        public void ReplyTo(SocketUserMessage message, string text)
        {
            message.ReplyAsync(text);
        }

        public async Task ReplyToAsync(SocketUserMessage message, string text)
        {
            await message.ReplyAsync(text);
        }

        public string GetDisplayName(ulong userId)
        {
            var user = _client.GetUser(userId);

            if (user == null)
            {
                return "";
            }

            if (!string.IsNullOrWhiteSpace(user.GlobalName))
            {
                return user.GlobalName;
            }

            return user.Username;
        }

        public string GetUserName(ulong userId)
        {
            var user = _client.GetUser(userId);

            if (user == null)
            {
                return "";
            }

            return user.Username;
        }

        public ulong GetUserId(string username)
        {
            var user = _client.GetUser(username);

            if (user == null)
            {
                return 0;
            }

            return user.Id;
        }

        public async void SetStatusMessage(string statusText, ActivityType activity = ActivityType.Playing)
        {
            await _client.SetGameAsync(statusText, type: activity);
        }

        public async Task DeleteMessage(IMessage message)
        {
            await message.DeleteAsync();
        }

        public async Task DeleteAllMessagesInChannel(ulong channelId)
        {
            var channel = _client.GetChannel(channelId);

            if (!(channel is ISocketMessageChannel messageChannel))
            {
                return;
            }

            await DeleteAllMessagesInChannel(messageChannel);
        }

        public async Task DeleteAllMessagesInChannel(ISocketMessageChannel channel)
        {
            var messages = channel.GetMessagesAsync();
            var messagesToDelete = await messages.FlattenAsync();

            foreach (var message in messagesToDelete)
            {
                await DeleteMessage(message);
                Thread.Sleep(200);
            }
        }

        public async Task<IUser[]> GetUsersInChannel(ISocketMessageChannel messageChannel)
        {
            var users = messageChannel.GetUsersAsync();
            var flattenedUsers = await users.FlattenAsync();
            return flattenedUsers.ToArray();
        }

        private Task Log(LogMessage message)
        {
            var errorText = $"Discord Bot | {DateTime.Now,-19} [{message.Severity,8}] {message.Source}: {message.Message} {message.Exception}";
            switch (message.Severity)
            {
                case LogSeverity.Critical:
                case LogSeverity.Error:
                    _logger.LogError(errorText);
                    break;
                case LogSeverity.Warning:
                    _logger.LogWarning(errorText);
                    break;
                case LogSeverity.Info:
                    _logger.LogInfo(errorText);
                    break;
                case LogSeverity.Verbose:
                case LogSeverity.Debug:
                    _logger.LogDebug(errorText);
                    break;
            }

            return Task.CompletedTask;
        }

        private static IServiceProvider ConfigureServices()
        {
            var map = new ServiceCollection();
            // Repeat this for all the service classes
            // and other dependencies that your commands might need.
            //.AddSingleton(new SomeServiceClass());

            // When all your required services are in the collection, build the container.
            // Tip: There's an overload taking in a 'validateScopes' bool to make sure
            // you haven't made any mistakes in your dependency graph.
            return map.BuildServiceProvider();
        }

        private async Task InitCommands(Func<SocketMessage, Task> messageReceivedFunction)
        {
            // Either search the program and add all Module classes that can be found.
            // Module classes MUST be marked 'public' or they will be ignored.
            // You also need to pass your 'IServiceProvider' instance now,
            // so make sure that's done before you get here.
            // await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
            // Or add Modules manually if you prefer to be a little more explicit:
            //await _commands.AddModuleAsync<SomeModule>(_services);
            // Note that the first one is 'Modules' (plural) and the second is 'Module' (singular).

            // Subscribe a handler to see if a message invokes a command.
            _client.MessageReceived += messageReceivedFunction;
        }
    }
}

using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace StardewArchipelago.ViewerEventsModule
{
    public interface IBotCommunicator
    {
        ulong MyId { get; }
        void InitializeClient();
        void InitializeLog();
        void Login(string token);
        void Start(Func<SocketMessage, Task> messageReceivedFunction);
        void SendMessage(ulong channelId, string text);
        Task SendMessageAsync(ulong channelId, string text);
        Task SendMessageAsync(ISocketMessageChannel channel, string text);
        void ReplyTo(SocketUserMessage message, string text);
        Task ReplyToAsync(SocketUserMessage message, string text);
        string GetDisplayName(ulong userId);
        string GetUserName(ulong userId);
        ulong GetUserId(string username);
        void SetStatusMessage(string statusText, ActivityType activity = ActivityType.Playing);
        Task DeleteMessage(IMessage message);
        Task DeleteAllMessagesInChannel(ulong channelId);
        Task DeleteAllMessagesInChannel(ISocketMessageChannel channel);
        Task<IUser[]> GetUsersInChannel(ISocketMessageChannel messageChannel);
    }
}
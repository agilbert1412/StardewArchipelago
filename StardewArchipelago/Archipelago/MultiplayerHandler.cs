using KaitoKid.ArchipelagoUtilities.Net.Client;
using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace StardewArchipelago.Archipelago
{
    internal class MultiplayerHandler
    {
        public static MultiplayerHandler Instance;
        private IModHelper _helper;

        public void OnModMessageReceived(object sender, ModMessageReceivedEventArgs e)
        {
            if (e.FromModID != ModEntry.Instance.ModManifest.UniqueID || e.Type != nameof(ArchipelagoConnectionInfo))
            {
                return;
            }
            ArchipelagoConnectionInfo message = e.ReadAs<ArchipelagoConnectionInfo>();
            ModEntry.Instance.ArchipelagoConnect(message.HostUrl, message.Port, message.SlotName, message.Password, out var errorMessage);
        }

        public void OnPeerConnected(object sender, PeerConnectedEventArgs e)
        {
            ArchipelagoConnectionInfo message = ModEntry.Instance.State.APConnectionInfo;
            _helper.Multiplayer.SendMessage(message, nameof(ArchipelagoConnectionInfo), modIDs: new[] { ModEntry.Instance.ModManifest.UniqueID }, playerIDs: new[] { e.Peer.PlayerID });
        }
    }
}

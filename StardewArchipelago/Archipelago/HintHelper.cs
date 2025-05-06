using StardewValley;
using System;
using Archipelago.MultiClient.Net;
using Microsoft.Xna.Framework;

namespace StardewArchipelago.Archipelago
{
    public class HintHelper
    {
        private bool _canAffordHintYesterday;

        public HintHelper()
        {
            _canAffordHintYesterday = false;
        }

        public void GiveHintTip(ArchipelagoSession session)
        {
            if (session == null)
            {
                return;
            }

            if (!TryGetCurrentHintCost(session, out var hintCost))
            {
                return;
            }

            var canAffordHintToday = session.RoomState.HintPoints >= hintCost;
            if (!_canAffordHintYesterday && canAffordHintToday)
            {
                Game1.chatBox?.addMessage($"You can now afford a hint. Syntax: '!hint [itemName]'", Color.Gold);
            }

            _canAffordHintYesterday = canAffordHintToday;
        }

        private static bool TryGetCurrentHintCost(ArchipelagoSession session, out int hintCost)
        {
            hintCost = session.RoomState.HintCost;
            if (hintCost <= 0)
            {
                hintCost = (int)Math.Max(0M,
                    session.Locations.AllLocations.Count * 0.01M *
                    session.RoomState.HintCostPercentage);

                if (hintCost <= 0)
                {
                    return false;
                }
            }

            return true;
        }
    }
}

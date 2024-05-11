using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Constants;
using StardewArchipelago.Extensions;
using StardewArchipelago.Items.Mail;
using StardewArchipelago.Locations;
using StardewArchipelago.Locations.CodeInjections.Vanilla;
using StardewValley;
using StardewValley.Constants;
using StardewValley.Locations;

namespace StardewArchipelago.Items.Unlocks
{
    public class ToolUnlockManager : IUnlockManager
    {
        public const string PROGRESSIVE_TOOL_AP_PREFIX = "Progressive ";
        public const string PROGRESSIVE_FISHING_ROD = "Progressive Fishing Rod";
        public const string RETURN_SCEPTER = "Return Scepter";
        public const string PROGRESSIVE_SCYTHE = "Progressive Scythe";
        public const string GOLDEN_SCYTHE = "Golden Scythe";

        public ToolUnlockManager()
        {
        }

        public void RegisterUnlocks(IDictionary<string, Func<ReceivedItem, LetterAttachment>> unlocks)
        {
            RegisterProgressiveTools(unlocks);
        }

        private void RegisterProgressiveTools(IDictionary<string, Func<ReceivedItem, LetterAttachment>> unlocks)
        {
            unlocks.Add($"{PROGRESSIVE_TOOL_AP_PREFIX}Axe", SendProgressiveAxeLetter);
            unlocks.Add($"{PROGRESSIVE_TOOL_AP_PREFIX}Pickaxe", SendProgressivePickaxeLetter);
            unlocks.Add($"{PROGRESSIVE_TOOL_AP_PREFIX}Hoe", SendProgressiveHoeLetter);
            unlocks.Add($"{PROGRESSIVE_TOOL_AP_PREFIX}Watering Can", SendProgressiveWateringCanLetter);
            unlocks.Add($"{PROGRESSIVE_TOOL_AP_PREFIX}Trash Can", SendProgressiveTrashCanLetter);
            unlocks.Add($"{PROGRESSIVE_TOOL_AP_PREFIX}Pan", SendProgressivePanLetter);
            unlocks.Add(PROGRESSIVE_FISHING_ROD, SendProgressiveFishingRodLetter);
            unlocks.Add(RETURN_SCEPTER, SendReturnScepterLetter);
            unlocks.Add(PROGRESSIVE_SCYTHE, SendProgressiveScytheLetter);
            unlocks.Add(GOLDEN_SCYTHE, SendGoldenScytheLetter); // Deprecated, but kept in case of start inventory
        }

        private LetterActionAttachment SendProgressiveAxeLetter(ReceivedItem receivedItem)
        {
            return new LetterActionAttachment(receivedItem, LetterActionsKeys.ProgressiveTool, "Axe");
        }

        private LetterActionAttachment SendProgressivePickaxeLetter(ReceivedItem receivedItem)
        {
            return new LetterActionAttachment(receivedItem, LetterActionsKeys.ProgressiveTool, "Pickaxe");
        }

        private LetterActionAttachment SendProgressiveHoeLetter(ReceivedItem receivedItem)
        {
            return new LetterActionAttachment(receivedItem, LetterActionsKeys.ProgressiveTool, "Hoe");
        }

        private LetterActionAttachment SendProgressiveWateringCanLetter(ReceivedItem receivedItem)
        {
            return new LetterActionAttachment(receivedItem, LetterActionsKeys.ProgressiveTool, "Watering Can");
        }

        private LetterActionAttachment SendProgressiveTrashCanLetter(ReceivedItem receivedItem)
        {
            return new LetterActionAttachment(receivedItem, LetterActionsKeys.ProgressiveTool, "Trash Can");
        }

        private LetterActionAttachment SendProgressivePanLetter(ReceivedItem receivedItem)
        {
            return new LetterActionAttachment(receivedItem, LetterActionsKeys.ProgressiveTool, "Pan");
        }

        private LetterActionAttachment SendProgressiveFishingRodLetter(ReceivedItem receivedItem)
        {
            return new LetterActionAttachment(receivedItem, LetterActionsKeys.FishingRod);
        }

        private LetterActionAttachment SendReturnScepterLetter(ReceivedItem receivedItem)
        {
            return new LetterActionAttachment(receivedItem, LetterActionsKeys.ReturnScepter);
        }

        private LetterActionAttachment SendGoldenScytheLetter(ReceivedItem receivedItem)
        {
            if (!Game1.player.mailReceived.Contains("gotGoldenScythe"))
            {
                Game1.player.mailReceived.Add("gotGoldenScythe");
            }
            return new LetterActionAttachment(receivedItem, LetterActionsKeys.GoldenScythe);
        }

        private LetterActionAttachment SendProgressiveScytheLetter(ReceivedItem receivedItem)
        {
            if (!Game1.player.mailReceived.Contains("gotGoldenScythe"))
            {
                Game1.player.mailReceived.Add("gotGoldenScythe");
            }
            return new LetterActionAttachment(receivedItem, LetterActionsKeys.ProgressiveScythe);
        }
    }
}

using System;
using System.Collections.Generic;
using StardewArchipelago.Stardew;
using StardewValley;
using StardewValley.Tools;

namespace StardewArchipelago.Items.Mail
{
    public class ModLetterActions
    {
        private StardewItemManager _stardewItemManager;

        public ModLetterActions(StardewItemManager stardewItemManager)
        {
            _stardewItemManager = stardewItemManager;
        }

        public void AddModLetterActions(Dictionary<string, Action<string>> letterActions)
        {
            letterActions.Add(LetterActionsKeys.DiamondWand, (_) => ReceiveDiamondWand(_stardewItemManager));
        }

        private void ReceiveDiamondWand(StardewItemManager _stardewItemManager)
        {
            Game1.playSound("parry");
            var diamondWandId = _stardewItemManager.GetWeaponByName("Diamond Wand").Id;
            var diamondWand = new MeleeWeapon(diamondWandId);
            Game1.player.holdUpItemThenMessage(diamondWand);
            Game1.player.addItemByMenuIfNecessary(diamondWand);
        }
    }
}

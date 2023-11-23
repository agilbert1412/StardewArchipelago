using System;
using System.Linq;
using System.Collections.Generic;
using StardewValley;
using StardewValley.Tools;

namespace StardewArchipelago.Items.Mail
{
    public class ModLetterActions
    {

        public void ModLetters(Dictionary<string, Action<string>> letterActions)
        {
            letterActions.Add(LetterActionsKeys.DiamondWand, (_) => ReceiveDiamondWand());
        }

        private void ReceiveDiamondWand()
        {
            Game1.playSound("parry");
            var weaponData = Game1.content.Load<Dictionary<int, string>>("Data\\weapons");
            var id = weaponData.FirstOrDefault(x => x.Value == "Diamond Wand/A magic relic from Galdora. Teleports enemies away from the user. Effective against flyers./1/2/31/6/800/0/3/0/-1/-1/1/1/Diamond Wand");
            var diamondWand = new MeleeWeapon(id.Key);
            Game1.player.holdUpItemThenMessage(diamondWand);
            Game1.player.addItemByMenuIfNecessary(diamondWand);
        } 
    }
}
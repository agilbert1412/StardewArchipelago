using System;
using System.Collections.Generic;
using StardewValley;
using StardewValley.Objects;
using StardewValley.Tools;

namespace StardewArchipelago.Items.Mail
{
    public class LetterActions
    {
        private readonly Mailman _mail;
        private Dictionary<string, Action<string>> _letterActions;

        public LetterActions(Mailman mail)
        {
            _mail = mail;
            _letterActions = new Dictionary<string, Action<string>>();
            _letterActions.Add(LetterActionsKeys.Frienship, IncreaseFriendshipWithEveryone);
            _letterActions.Add(LetterActionsKeys.Backpack, (_) => IncreaseBackpackLevel());
            _letterActions.Add(LetterActionsKeys.DwarvishTranslationGuide, (_) => ReceiveDwarvishTranslationGuide());
            _letterActions.Add(LetterActionsKeys.SkullKey, (_) => ReceiveSkullKey());
            _letterActions.Add(LetterActionsKeys.RustyKey, (_) => ReceiveRustyKey());
            _letterActions.Add(LetterActionsKeys.GoldenScythe, (_) => ReceiveGoldenScythe());
            _letterActions.Add(LetterActionsKeys.ProgressiveTool, ReceiveProgressiveTool);
            _letterActions.Add(LetterActionsKeys.FishingRod, (_) => GetFishingRodOfNextLevel());
            _letterActions.Add(LetterActionsKeys.GiveRing, ReceiveRing);
            _letterActions.Add(LetterActionsKeys.GiveBoots, ReceiveBoots);
            _letterActions.Add(LetterActionsKeys.GiveMeleeWeapon, ReceiveMeleeWeapon);
            _letterActions.Add(LetterActionsKeys.GiveSlingshot, ReceiveSlingshot);
        }

        public void ExecuteLetterAction(string key, string parameter)
        {
            _letterActions[key](parameter);
        }

        private void IncreaseFriendshipWithEveryone(string friendshipPoints)
        {
            var farmer = Game1.player;
            var numberOfPoints = int.Parse(friendshipPoints);
            foreach (var npc in farmer.friendshipData.Keys)
            {
                farmer.friendshipData[npc].Points += numberOfPoints;
            }
        }

        private void IncreaseBackpackLevel()
        {
            var previousMaxItems = Game1.player.MaxItems;
            var backpackName = "";
            switch (Game1.player.MaxItems)
            {
                case < 12:
                    Game1.player.MaxItems = 12;
                    break;
                case < 24:
                    Game1.player.MaxItems = 24;
                    backpackName = Game1.content.LoadString("Strings\\StringsFromCSFiles:GameLocation.cs.8708");
                    break;
                case >= 24:
                    Game1.player.MaxItems = 36;
                    backpackName = Game1.content.LoadString("Strings\\StringsFromCSFiles:GameLocation.cs.8709");
                    break;
            }

            if (previousMaxItems >= Game1.player.MaxItems)
            {
                return;
            }

            while (Game1.player.Items.Count < Game1.player.MaxItems)
            {
                Game1.player.Items.Add(null);
            }
            Game1.player.holdUpItemThenMessage(new SpecialItem(99, backpackName));
        }

        private void ReceiveDwarvishTranslationGuide()
        {
            Game1.player.canUnderstandDwarves = true;
            Game1.playSound("fireball");
        }

        private void ReceiveSkullKey()
        {
            Game1.player.hasSkullKey = true;
            Game1.player.addQuest(19);
        }

        private void ReceiveRustyKey()
        {
            Game1.player.hasRustyKey = true;
        }

        private void ReceiveGoldenScythe()
        {
            Game1.playSound("parry");
            var goldenScythe = new MeleeWeapon(53);
            Game1.player.holdUpItemThenMessage(goldenScythe);
            Game1.player.addItemByMenuIfNecessary(goldenScythe);
        }

        private void ReceiveProgressiveTool(string toolGenericName)
        {
            if (toolGenericName.Contains("Trash Can"))
            {
                ReceiveTrashCanUpgrade();
                return;
            }

            var upgradedTool = UpgradeToolInEntireWorld(toolGenericName);

            if (upgradedTool == null)
            {
                throw new Exception($"Could not find a tool of type {toolGenericName} in this entire world");
            }

            Game1.player.holdUpItemThenMessage(upgradedTool);
        }

        private static Tool UpgradeToolInEntireWorld(string toolGenericName)
        {
            var player = Game1.player;
            foreach (var playerItem in player.Items)
            {
                if (playerItem is not Tool toolToUpgrade || !toolToUpgrade.Name.Contains(toolGenericName))
                {
                    continue;
                }

                toolToUpgrade.UpgradeLevel++;
                return toolToUpgrade;
            }

            foreach (var gameLocation in Game1.locations)
            {
                foreach (var (tile, gameObject) in gameLocation.Objects.Pairs)
                {
                    if (gameObject is not Chest chest)
                    {
                        continue;
                    }

                    foreach (var chestItem in chest.items)
                    {
                        if (chestItem is not Tool toolToUpgrade || !toolToUpgrade.Name.Contains(toolGenericName))
                        {
                            continue;
                        }

                        toolToUpgrade.UpgradeLevel++;
                        return toolToUpgrade;
                    }
                }
            }

            return null;
        }

        private static void ReceiveTrashCanUpgrade()
        {
            Game1.player.trashCanLevel++;
            var trashCanToHoldUp = new GenericTool("Trash Can",
                Game1.content.LoadString("Strings\\StringsFromCSFiles:TrashCan_Description",
                    ((Game1.player.trashCanLevel * 15).ToString() ?? "")), Game1.player.trashCanLevel,
                12 + Game1.player.trashCanLevel, 12 + Game1.player.trashCanLevel);
            trashCanToHoldUp.upgradeLevel.Value = Game1.player.trashCanLevel;
            Game1.player.holdUpItemThenMessage(trashCanToHoldUp);
        }

        private void GetFishingRodOfNextLevel()
        {
            var numberOfPreviousFishingRodLetters = _mail.OpenedMailsContainingKey(UnlockManager.PROGRESSIVE_FISHING_ROD_AP_NAME);
            var itemToAdd = new FishingRod(numberOfPreviousFishingRodLetters - 1);

            Game1.player.holdUpItemThenMessage(itemToAdd);
            Game1.player.addItemByMenuIfNecessary(itemToAdd);
        }

        private void ReceiveRing(string ringId)
        {
            var id = int.Parse(ringId);
            var boots = new Ring(id);
            Game1.player.addItemByMenuIfNecessaryElseHoldUp(boots);
        }

        private void ReceiveBoots(string bootsId)
        {
            var id = int.Parse(bootsId);
            var boots = new Boots(id);
            Game1.player.addItemByMenuIfNecessaryElseHoldUp(boots);
        }

        private void ReceiveMeleeWeapon(string weaponId)
        {
            var id = int.Parse(weaponId);
            var weapon = new MeleeWeapon(id);
            Game1.player.addItemByMenuIfNecessaryElseHoldUp(weapon);
        }

        private void ReceiveSlingshot(string slingshotId)
        {
            var id = int.Parse(slingshotId);
            var slingshot = new Slingshot(id);
            Game1.player.addItemByMenuIfNecessaryElseHoldUp(slingshot);
        }
    }
}

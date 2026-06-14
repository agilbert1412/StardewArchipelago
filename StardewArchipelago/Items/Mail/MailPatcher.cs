using HarmonyLib;
using KaitoKid.ArchipelagoUtilities.Net;
using KaitoKid.ArchipelagoUtilities.Net.Constants;
using KaitoKid.Utilities.Interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Archipelago.SlotData.SlotEnums;
using StardewArchipelago.Goals;
using StardewArchipelago.Locations.CodeInjections.Vanilla;
using StardewArchipelago.Serialization;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Linq;
using xTile.Dimensions;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace StardewArchipelago.Items.Mail
{
    public class MailPatcher
    {
        private static ILogger _logger;
        private readonly Harmony _harmony;
        private static StardewArchipelagoClient _archipelago;
        private static LocationChecker _locationChecker;
        private static ArchipelagoStateDto _state;
        private static LetterActions _letterActions;

        public MailPatcher(ILogger logger, Harmony harmony, StardewArchipelagoClient archipelago, LocationChecker locationChecker, ArchipelagoStateDto state, LetterActions letterActions)
        {
            _logger = logger;
            _harmony = harmony;
            _archipelago = archipelago;
            _locationChecker = locationChecker;
            _state = state;
            _letterActions = letterActions;
        }

        public void PatchMailBoxForApItems()
        {
            _harmony.Patch(
                original: AccessTools.Method(typeof(IClickableMenu), nameof(IClickableMenu.exitThisMenu)),
                postfix: new HarmonyMethod(typeof(MailPatcher), nameof(ExitThisMenu_ApplyLetterAction_Postfix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.mailbox)),
                prefix: new HarmonyMethod(typeof(MailPatcher), nameof(Mailbox_RemoveAndReplaceLetters_Prefix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(Farm), nameof(Farm.draw)),
                postfix: new HarmonyMethod(typeof(MailPatcher), nameof(Draw_AddMailNumber_Postfix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.checkAction)),
                postfix: new HarmonyMethod(typeof(MailPatcher), nameof(CheckAction_ClickGhostMailbox_Postfix))
            );
        }

        public static void ExitThisMenu_ApplyLetterAction_Postfix(IClickableMenu __instance, bool playSound)
        {
            try
            {
                if (__instance is not LetterViewerMenu letterMenuInstance || letterMenuInstance.mailTitle == null || letterMenuInstance.isFromCollection)
                {
                    return;
                }

                var title = letterMenuInstance.mailTitle;
                if (!MailKey.TryParse(title, out var apMailKey))
                {
                    return;
                }

                var apActionName = apMailKey.LetterOpenedAction;
                var apActionParameter = apMailKey.ActionParameter;

                if (string.IsNullOrWhiteSpace(apActionName))
                {
                    return;
                }

                _letterActions.ExecuteLetterAction(apActionName, apActionParameter);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(ExitThisMenu_ApplyLetterAction_Postfix)}:\n{ex}");
                return;
            }
        }

        // public void mailbox()
        public static bool Mailbox_RemoveAndReplaceLetters_Prefix(GameLocation __instance)
        {
            try
            {
                MailboxHideEmptyApLetters();
                MailboxHideNpcGiftMail();
                MailboxRemoveMasterAnglerStardropOnFishsanity();
                MailboxRemoveRarecrowSocietyRecipeOnFestivals();
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(Mailbox_RemoveAndReplaceLetters_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        public static void MailboxHideEmptyApLetters()
        {
            try
            {
                if (!ModEntry.Instance.Config.HideEmptyArchipelagoLetters)
                {
                    return;
                }

                CleanMailboxUntilNonEmptyLetter();
                var mailbox = Game1.mailbox;
                if (mailbox == null || !mailbox.Any())
                {
                    return;
                }

                var nextLetter = mailbox.First();

                if (!MailKey.TryParse(nextLetter, out _))
                {
                    return;
                }

                var mailData = DataLoader.Mail(Game1.content);
                if (!mailData.ContainsKey(nextLetter))
                {
                    mailData.Add(nextLetter, _state.LettersGenerated[nextLetter]);
                }

                // We force add the letter because it can contain custom content that can be then considered "to not be remembered" by the base game.
                // So if it's an ap letter, always remember it
                Game1.player.mailReceived.Add(nextLetter);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(MailboxHideEmptyApLetters)}:\n{ex}");
                return;
            }
        }

        private static void CleanMailboxUntilNonEmptyLetter()
        {
            var mailbox = Game1.mailbox;
            while (mailbox.Count > 1)
            {
                var nextLetterInMailbox = Game1.mailbox[1]; //Check starting from the second letter, attempting to remove the first letter causes the entire mailbox to be emptied, including received items and quests

                if (!MailKey.TryParse(nextLetterInMailbox, out var apMailKey))
                {
                    return;
                }

                if (!apMailKey.IsEmpty)
                {
                    return;
                }

                Game1.player.mailReceived.Add(nextLetterInMailbox);
                mailbox.RemoveAt(1);
            }
        }

        public static void MailboxRemoveMasterAnglerStardropOnFishsanity()
        {
            try
            {
                if (_archipelago.SlotData.Fishsanity == Fishsanity.None)
                {
                    return;
                }

                var mailbox = Game1.mailbox;
                if (mailbox == null || !mailbox.Any())
                {
                    return;
                }

                var nextLetter = mailbox.First();
                if (!nextLetter.Equals(GoalCodeInjection.MASTER_ANGLER_LETTER))
                {
                    return;
                }

                ReplaceStardropWithSeafoamPudding();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(MailboxRemoveMasterAnglerStardropOnFishsanity)}:\n{ex}");
                return;
            }
        }

        private static void ReplaceStardropWithSeafoamPudding()
        {
            const string stardropText = "stardrop";
            const string stardropItemText = "%item id (O)434 1 %%";
            const string puddingText = "pudding";
            const string seafoamPuddingItemText = "%item id (O)265 10 %%";

            var mailContent = DataLoader.Mail(Game1.content);
            var masterAnglerLetterContent = mailContent[GoalCodeInjection.MASTER_ANGLER_LETTER];
            mailContent[GoalCodeInjection.MASTER_ANGLER_LETTER] = masterAnglerLetterContent
                .Replace(stardropItemText, seafoamPuddingItemText)
                .Replace(stardropText, puddingText);
        }

        private const string RARECROW_SOCIETY_LETTER = "RarecrowSociety";
        private const string RARECROW_SOCIETY_AP_LOCATION = "Collect All Rarecrows";

        public static void MailboxRemoveRarecrowSocietyRecipeOnFestivals()
        {
            try
            {
                if (_archipelago.SlotData.FestivalLocations == FestivalLocations.Vanilla)
                {
                    return;
                }

                var mailbox = Game1.mailbox;
                if (mailbox == null || !mailbox.Any())
                {
                    return;
                }

                var nextLetter = mailbox.First();
                if (!nextLetter.Equals(RARECROW_SOCIETY_LETTER))
                {
                    return;
                }

                RemoveDeluxeScarecrowRecipe();
                _locationChecker.AddCheckedLocation(RARECROW_SOCIETY_AP_LOCATION);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(MailboxRemoveRarecrowSocietyRecipeOnFestivals)}:\n{ex}");
                return;
            }
        }

        private static void RemoveDeluxeScarecrowRecipe()
        {
            const string deluxeScarecrowRecipeText = "Please accept this blueprint to commemorate your achievement.";
            const string deluxeScarecrowRecipeItemText = "%item craftingRecipe Deluxe_Scarecrow %%";

            var scoutedLocation = _archipelago.ScoutStardewLocation(RARECROW_SOCIETY_AP_LOCATION);
            var scoutedItemName = scoutedLocation.ItemName;
            var scoutedPlayer = scoutedLocation.PlayerName;
            var replacementText = $"We will send {scoutedItemName} to {scoutedPlayer} to commemorate your achievement.";
            const string recipeReplacementText = "";

            var mailContent = DataLoader.Mail(Game1.content);
            var masterAnglerLetterContent = mailContent[RARECROW_SOCIETY_LETTER];
            mailContent[RARECROW_SOCIETY_LETTER] = masterAnglerLetterContent
                .Replace(deluxeScarecrowRecipeItemText, recipeReplacementText)
                .Replace(deluxeScarecrowRecipeText, replacementText);
        }

        // public override void draw(SpriteBatch b)
        public static void Draw_AddMailNumber_Postfix(Farm __instance, SpriteBatch b)
        {
            try
            {
                if (Game1.mailbox.Count <= 0)
                {
                    return;
                }

                var animationOffsetY = (float)(4.0 * Math.Round(Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 250.0), 2));
                var mailboxPositionTile = Game1.player.getMailboxPosition();
                var mailBoxPositionPixel = new Vector2(mailboxPositionTile.X * 64, mailboxPositionTile.Y * 64);
                var globalPosition = new Vector2(mailBoxPositionPixel.X + 8, mailBoxPositionPixel.Y - 96 - 48 + animationOffsetY + 8);
                var localPosition = Game1.GlobalToLocal(Game1.viewport, globalPosition);
                var numLetters = Game1.mailbox.Count;
                const float scale = 1f;
                Utility.drawTinyDigits(numLetters, b, localPosition + new Vector2(64 - Utility.getWidthOfTinyDigitString(numLetters, 3f * scale) + 3f * scale, (float)(64.0 - 18.0 * scale + 1.0)), 3f * scale, 1f, Color.Red);

                if (_archipelago.HasReceivedItem(CarpenterInjections.BUILDING_PROGRESSIVE_HOUSE))
                {
                    return;
                }

                DrawGhostMailbox(__instance, b);

                return;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(Draw_AddMailNumber_Postfix)}:\n{ex}");
                return;
            }
        }

        private static void DrawGhostMailbox(Farm farm, SpriteBatch spriteBatch)
        {
            var texture = Game1.content.Load<Texture2D>("Buildings\\Mailbox");
            var mailboxPositionTile = Game1.player.getMailboxPosition();
            var position = Game1.GlobalToLocal(Game1.viewport, new Vector2(mailboxPositionTile.X * 64 + 8, mailboxPositionTile.Y * 64 - 64));
            var sourceRect = new Rectangle(0, 0, 16, 32);
            var alpha = 0.5f;
            var color = Color.White * alpha;
            var layerDepth = (((mailboxPositionTile.Y + 2) * 64) - 1) / 10000f;
            spriteBatch.Draw(texture, position, sourceRect, color, 0.0f, new Vector2(0.0f, 0.0f), 4f, SpriteEffects.None, layerDepth);
        }

        public static void MailboxHideNpcGiftMail()
        {
            try
            {
                if (!ModEntry.Instance.Config.HideNpcGiftMail)
                {
                    return;
                }

                var mailbox = Game1.mailbox;
                if (mailbox == null | !mailbox.Any())
                {
                    return;
                }

                while (mailbox.Count > 1)
                {
                    var nextLetter = Game1.mailbox[1]; //Check starting from the second letter, attempting to remove the first letter causes the entire mailbox to be emptied, including received items and quests
                    if (!Game1.player.friendshipData.Keys.Contains(nextLetter))
                    {
                        return;
                    }
                    Game1.player.mailReceived.Add(nextLetter);
                    mailbox.RemoveAt(1);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(MailboxHideNpcGiftMail)}:\n{ex}");
                return;
            }
        }

        public void ReceiveFirstFewBackpacks()
        {
            while (Game1.player.MaxItems < 6)
            {
                if (TryReceiveOneMailBackpack())
                {
                    continue;
                }
                return;
            }
        }

        private static bool TryReceiveOneMailBackpack()
        {

            var mailbox = Game1.player.mailbox.ToArray();
            for (var i = 0; i < mailbox.Length; i++)
            {
                var mail = mailbox[i];
                if (mail.Contains("Progressive_Backpack"))
                {
                    _letterActions.IncreaseBackpackLevel();
                    Game1.player.mailbox.RemoveAt(i);
                    Game1.player.mailReceived.Add(mail);
                    return true;
                }
            }

            return false;
        }

        // public virtual bool checkAction(Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who)
        public static void CheckAction_ClickGhostMailbox_Postfix(GameLocation __instance, Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who, ref bool __result)
        {
            try
            {
                if (__result || __instance is not Farm farm)
                {
                    return;
                }

                if (!_archipelago.SlotData.StartWithout.HasFlag(StartWithout.House) || _archipelago.HasReceivedItem(CarpenterInjections.BUILDING_PROGRESSIVE_HOUSE))
                {
                    return;
                }

                if (Game1.mailbox.Count <= 0)
                {
                    return;
                }

                var mailboxPositionTile = Game1.player.getMailboxPosition();
                if (tileLocation.X == mailboxPositionTile.X && tileLocation.Y == mailboxPositionTile.Y)
                {
                    farm.mailbox();
                    __result = true;
                    return;
                }

                return;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(CheckAction_ClickGhostMailbox_Postfix)}:\n{ex}");
                return;
            }
        }
    }
}

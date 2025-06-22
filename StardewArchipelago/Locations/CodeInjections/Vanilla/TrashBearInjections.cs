using System;
using System.Linq;
using KaitoKid.ArchipelagoUtilities.Net;
using KaitoKid.ArchipelagoUtilities.Net.Constants;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Locations;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using StardewArchipelago.Archipelago;
using StardewValley.Characters;
using System.Reflection;
using Microsoft.Xna.Framework;
using StardewArchipelago.Bundles;
using StardewArchipelago.Constants.Vanilla;
using StardewArchipelago.Serialization;
using StardewArchipelago.Stardew;
using StardewModdingAPI;
using Netcode;
using StardewValley.Network;

namespace StardewArchipelago.Locations.CodeInjections.Vanilla
{
    public static class TrashBearInjections
    {
        private static ILogger _logger;
        private static IModHelper _modHelper;
        private static StardewArchipelagoClient _archipelago;
        private static LocationChecker _locationChecker;
        private static BundlesManager _bundlesManager;
        private static StardewItemManager _itemManager;
        private static ArchipelagoStateDto _state;
        
        public static void Initialize(ILogger logger, IModHelper modHelper, StardewArchipelagoClient archipelago, LocationChecker locationChecker, BundlesManager bundlesManager, StardewItemManager itemManager, ArchipelagoStateDto state)
        {
            _logger = logger;
            _modHelper = modHelper;
            _archipelago = archipelago;
            _locationChecker = locationChecker;
            _bundlesManager = bundlesManager;
            _itemManager = itemManager;
            _state = state;
        }

        // protected override void resetLocalState()
        public static void ResetLocalState_SpawnTrashBear_Postfix(Forest __instance)
        {
            try
            {
                var receivedTrashBear = _archipelago.HasReceivedItem("Trash Bear Arrival");
                var trashBear = __instance.getCharacterFromName("TrashBear");
                var trashBearIsHere = trashBear != null;
                if (receivedTrashBear)
                {
                    if (!trashBearIsHere)
                    {
                        __instance.characters.Add(new TrashBear());
                    }
                }
                else
                {
                    if (trashBearIsHere)
                    {
                        __instance.characters.Remove(trashBear);
                    }
                }

                var hasReceivedCleanup = _archipelago.HasReceivedItem("Trash Bear Cleanup");
                var hasSeenCutscene = NetWorldState.checkAnywhereForWorldStateID("trashBearDone");

                if (hasReceivedCleanup && !hasSeenCutscene)
                {
                    NetWorldState.addWorldStateIDEverywhere("trashBear3");
                    NetWorldState.addWorldStateIDEverywhere("trashBear2");
                    NetWorldState.addWorldStateIDEverywhere("trashBear1");
                    NetWorldState.addWorldStateIDEverywhere("trashBearDone");
                    StartTrashBearCleanupCutscene();
                }

                return;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(ResetLocalState_SpawnTrashBear_Postfix)}:\n{ex}");
                return;
            }
        }

        private static void StartTrashBearCleanupCutscene()
        {
            if (!(Game1.currentLocation is Forest))
                return;
            if (Game1.activeClickableMenu != null && Game1.activeClickableMenu.readyToClose())
                Game1.activeClickableMenu.exitThisMenuNoSound();
            if (Game1.activeClickableMenu != null)
                return;
            Game1.player.freezePause = 2000;
            Game1.globalFadeToBlack(() => Game1.currentLocation.startEvent(new StardewValley.Event("spring_day_ambient/-1000 -1000/farmer 104 95 3/skippable/addTemporaryActor TrashBear 32 32 102 95 0 false/animate TrashBear false true 250 0 1/viewport 102 97 clamp true/pause 3000/stopAnimation TrashBear/move TrashBear 0 2 2/faceDirection farmer 2/pause 1000/animate TrashBear false true 275 13 14 15 14/playSound trashbear_flute/specificTemporarySprite trashBearPrelude/viewport move -1 1 4000/pause 9000/stopAnimation TrashBear/playSound yoba/specificTemporarySprite trashBearMagic/pause 500/animate farmer false true 100 94/jump farmer/pause 2000/viewport move 1 -1 4000/stopAnimation farmer/move farmer 0 2 2/pause 4000/playSound trashbear/specificTemporarySprite trashBearUmbrella1/warp TrashBear -100 -100/pause 2000/faceDirection farmer 1/pause 2000/fade/viewport -5000 -5000/changeLocation Town/viewport 54 68 true/specificTemporarySprite trashBearTown/pause 10000/end", null, "777111")));
        }

        // private void doCutscene()
        public static bool DoCutscene_DontPlayTheCutsceneNormally_Prefix(TrashBear __instance)
        {
            try
            {
                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(DoCutscene_DontPlayTheCutsceneNormally_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        // public override void draw(SpriteBatch b)
        public static bool Draw_DrawAllDesiredItemsAtOnce_Prefix(TrashBear __instance, SpriteBatch b)
        {
            try
            {
                CallBaseDraw(__instance, b);
                // private int showWantBubbleTimer;
                var showWantBubbleTimerField = _modHelper.Reflection.GetField<int>(__instance, "showWantBubbleTimer");
                var showWantBubbleTimer = showWantBubbleTimerField.GetValue();
                if (showWantBubbleTimer <= 0)
                {
                    return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                }

                var offsetPerBubble = 64;
                var i = 0;
                foreach (var (requestType, requestItems) in _bundlesManager.TrashBearRequests)
                {
                    var itemsRemaining = requestItems.Where(x => !_state.TrashBearItemsEaten.Contains(x)).ToArray();
                    if (!itemsRemaining.Any())
                    {
                        continue;
                    }

                    var offsetY = -(offsetPerBubble * i);
                    var offsetXBase = -((offsetPerBubble/2) * (itemsRemaining.Length - 1));
                    for (var j = 0; j < itemsRemaining.Length; j++)
                    {
                        var offsetX = offsetXBase + (offsetPerBubble * j);
                        var item = itemsRemaining[j];
                        var itemId = _itemManager.GetObjectByName(item).GetQualifiedId();
                        DrawOneDesiredItem(__instance, b, itemId, new Point(offsetX, offsetY));
                    }
                    i++;
                }

                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(Draw_DrawAllDesiredItemsAtOnce_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        private static void CallBaseDraw(TrashBear trashBear, SpriteBatch b)
        {
            // base.draw(b);
            var characterDrawMethod = typeof(Character).GetMethod("draw", BindingFlags.Instance | BindingFlags.Public);
            var functionPointer = characterDrawMethod.MethodHandle.GetFunctionPointer();
            var baseDraw = (Action)Activator.CreateInstance(typeof(Action), trashBear, b, functionPointer);
            baseDraw();
        }

        private static void DrawOneDesiredItem(TrashBear trashBear, SpriteBatch spriteBatch, string desiredItemQualifiedId, Point offset)
        {
            var currentFrameOffset = (float)(2.0 * Math.Round(Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 250.0), 2));
            var tilePoint = trashBear.TilePoint;
            var num2 = (tilePoint.Y + 1) * 64 / 10000f;
            var num3 = currentFrameOffset - 40f;
            var bubblePosition = Game1.GlobalToLocal(Game1.viewport, new Vector2(tilePoint.X * 64 + 32 + offset.X, tilePoint.Y * 64 - 96 - 48 + num3 + offset.Y));
            spriteBatch.Draw(Game1.mouseCursors, bubblePosition, new Rectangle(141, 465, 20, 24), Color.White * 0.75f, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, num2 + 1E-06f);
            var dataOrErrorItem = ItemRegistry.GetDataOrErrorItem(desiredItemQualifiedId);
            var texture = dataOrErrorItem.GetTexture();
            var sourceRect = dataOrErrorItem.GetSourceRect();
            var itemPosition = Game1.GlobalToLocal(Game1.viewport, new Vector2(tilePoint.X * 64 + 64 + 8 + offset.X, tilePoint.Y * 64 - 64 - 32 - 8 + num3 + offset.Y));
            spriteBatch.Draw(texture, itemPosition, sourceRect, Color.White, 0.0f, new Vector2(8f, 8f), 4f, SpriteEffects.None, num2 + 1E-05f);
        }

        // public override bool tryToReceiveActiveObject(Farmer who, bool probe = false)
        public static bool TryToReceiveActiveObject_AcceptAnyDesiredItem_Prefix(TrashBear __instance, Farmer who, bool probe, ref bool __result)
        {
            try
            {
                __result = false;
                if (who == null || who.ActiveObject == null || string.IsNullOrWhiteSpace(who.ActiveObject.QualifiedItemId))
                {
                    return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                }

                var heldItemId = who.ActiveObject.QualifiedItemId;

                var desiredItems = _bundlesManager.TrashBearRequests.SelectMany(x => x.Value);
                var remainingItems = desiredItems.Where(x => !_state.TrashBearItemsEaten.Contains(x));
                var remainingItemIds = remainingItems.Select(x => _itemManager.GetObjectByName(x).GetQualifiedId()).ToHashSet();

                if (!remainingItemIds.Contains(heldItemId))
                {
                    return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                }

                __instance.itemWantedIndex = QualifiedItemIds.UnqualifyId(heldItemId);
                if (!probe)
                {
                    Game1.currentLocation.playSound("coin");
                    // private readonly NetEvent1Field<string, NetString> eatEvent = new NetEvent1Field<string, NetString>();
                    var eatEventField = _modHelper.Reflection.GetField<NetEvent1Field<string, NetString>>(__instance, "eatEvent");
                    var eatEvent = eatEventField.GetValue();
                    eatEvent.Fire(__instance.itemWantedIndex);
                    who.reduceActiveItemByOne();
                }

                CheckAllTrashBearFinishedLocations();
                __result = true;
                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(TryToReceiveActiveObject_AcceptAnyDesiredItem_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        private static void CheckAllTrashBearFinishedLocations()
        {
            foreach (var (requestType, requestItems) in _bundlesManager.TrashBearRequests)
            {
                var itemsRemaining = requestItems.Where(x => !_state.TrashBearItemsEaten.Contains(x)).ToArray();
                if (itemsRemaining.Any())
                {
                    continue;
                }

                _locationChecker.AddCheckedLocation($"Trash Bear {requestType}");
            }
        }
    }
}

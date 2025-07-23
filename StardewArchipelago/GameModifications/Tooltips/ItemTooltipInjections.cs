using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using KaitoKid.ArchipelagoUtilities.Net.Constants;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using StardewArchipelago.Locations;
using StardewArchipelago.Locations.CodeInjections.Vanilla;
using StardewArchipelago.Logging;
using StardewArchipelago.Stardew.NameMapping;
using StardewArchipelago.Textures;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;
using Object = StardewValley.Object;

namespace StardewArchipelago.GameModifications.Tooltips
{
    public class ItemTooltipInjections
    {
        private static ILogger _logger;
        private static IModHelper _modHelper;
        private static ModConfig _config;
        private static ArchipelagoClient _archipelago;
        private static StardewLocationChecker _locationChecker;
        private static NameSimplifier _nameSimplifier;
        private static Texture2D _miniArchipelagoIcon;

        public static void Initialize(LogHandler logger, IModHelper modHelper, ModConfig config, ArchipelagoClient archipelago, StardewLocationChecker locationChecker, NameSimplifier nameSimplifier)
        {
            _logger = logger;
            _modHelper = modHelper;
            _config = config;
            _archipelago = archipelago;
            _locationChecker = locationChecker;
            _nameSimplifier = nameSimplifier;

            var desiredTextureName = ArchipelagoTextures.COLOR;
            _miniArchipelagoIcon = ArchipelagoTextures.GetArchipelagoLogo(12, desiredTextureName);
        }

        // public abstract void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
        public static void DrawInMenuObject_AddArchipelagoLogoIfNeeded_Postfix(Object __instance, SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
        {
            try
            {
                ItemDrawInMenuPostfix(__instance, spriteBatch, location, scaleSize, transparency, layerDepth, color, new Vector2(14f, 14f), new Vector2(8, 8));
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(DrawInMenuObject_AddArchipelagoLogoIfNeeded_Postfix)}:\n{ex}");
                return;
            }
        }

        // public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency,
        // float layerDepth, StackDrawType drawStackNumber, Color colorOverride, bool drawShadow)
        public static void DrawInMenuColoredObject_AddArchipelagoLogoIfNeeded_Postfix(ColoredObject __instance, SpriteBatch spriteBatch,
            Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber,
            Color colorOverride, bool drawShadow)
        {
            try
            {
                ItemDrawInMenuPostfix(__instance, spriteBatch, location, scaleSize, transparency, layerDepth, colorOverride, new Vector2(14f, 14f), new Vector2(8, 8));
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(DrawInMenuColoredObject_AddArchipelagoLogoIfNeeded_Postfix)}:\n{ex}");
                return;
            }
        }

        // public abstract void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
        public static void DrawInMenuHat_AddArchipelagoLogoIfNeeded_Postfix(Hat __instance, SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
        {
            try
            {
                ItemDrawInMenuPostfix(HatInjections.GetHatLocations(__instance), spriteBatch, location, scaleSize, transparency, layerDepth, color, new Vector2(14f, 14f), new Vector2(8, 8), true);
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(DrawInMenuHat_AddArchipelagoLogoIfNeeded_Postfix)}:\n{ex}");
                return;
            }
        }

        // public virtual void draw(SpriteBatch b, Color c, float layerDepth, int frameOffset = 0, int xOffset = 0, int yOffset = 0)
        public static void DrawRecipe_AddArchipelagoLogoIfNeeded_Postfix(ClickableTextureComponent __instance, SpriteBatch b, Color c, float layerDepth, int frameOffset, int xOffset, int yOffset)
        {
            try
            {
                DrawIndicatorOnRecipe(__instance, b, c, layerDepth, xOffset, yOffset);
                // DrawIndicatorOnSecretNote(__instance, b, c, layerDepth, xOffset, yOffset); ;
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(DrawRecipe_AddArchipelagoLogoIfNeeded_Postfix)}:\n{ex}");
                return;
            }
        }

        private static void DrawIndicatorOnRecipe(ClickableTextureComponent menu, SpriteBatch spriteBatch, Color color, float layerDepth, int xOffset, int yOffset)
        {
            try
            {
                if (!TryGetActiveCraftingPage(out var craftingPage))
                {
                    return;
                }

                if (craftingPage.pagesOfCraftingRecipes.Count <= craftingPage.currentCraftingPage)
                {
                    return;
                }

                if (!craftingPage.pagesOfCraftingRecipes[craftingPage.currentCraftingPage].ContainsKey(menu))
                {
                    return;
                }

                var recipe = craftingPage.pagesOfCraftingRecipes[craftingPage.currentCraftingPage][menu];
                var itemData = recipe.GetItemData();
                var name = itemData.InternalName;
                var simplifiedName = _nameSimplifier.GetSimplifiedName(name, itemData.QualifiedItemId, itemData.ItemId);
                var locationX = menu.bounds.X + xOffset + menu.sourceRect.Width / 2;// * menu.baseScale;
                var locationY = menu.bounds.Y + yOffset + menu.sourceRect.Height / 2;// * menu.baseScale;
                var location = new Vector2(locationX, locationY);
                var origin = new Vector2(menu.sourceRect.Width / 2, menu.sourceRect.Height / 2);
                var prefix = craftingPage.cooking ? "Cook " : "Craft ";
                ItemDrawInMenuPostfix($"{prefix}{simplifiedName}", spriteBatch, location, 1.0f, 1f, layerDepth, color, new Vector2(0, 0), origin);

                return;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(DrawIndicatorOnRecipe)}:\n{ex}");
                return;
            }
        }

        private static bool TryGetActiveCraftingPage(out CraftingPage craftingPage)
        {
            craftingPage = null;
            var activeMenu = Game1.activeClickableMenu;
            if (activeMenu == null)
            {
                return false;
            }

            if (activeMenu is CraftingPage)
            {
                craftingPage = (CraftingPage)activeMenu;
                return true;
            }

            if (Game1.activeClickableMenu is GameMenu gameMenu && gameMenu.GetCurrentPage() is CraftingPage)
            {
                craftingPage = (CraftingPage)gameMenu.GetCurrentPage();
                return true;
            }

            return false;
        }

        // public override void draw(SpriteBatch b)
        public static bool Draw_AddArchipelagoLogoOnSecretNotes_Prefix(CollectionsPage __instance, SpriteBatch b)
        {
            try
            {
                if (__instance.collections.Count <= __instance.currentTab || __instance.currentTab != CollectionsPage.secretNotesTab)
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                foreach (var textureComponent in __instance.sideTabs.Values)
                {
                    textureComponent.draw(b);
                }
                if (__instance.currentPage > 0)
                {
                    __instance.backButton.draw(b);
                }
                if (__instance.currentPage < __instance.collections[__instance.currentTab].Count - 1)
                {
                    __instance.forwardButton.draw(b);
                }
                b.End();
                DrawSecretNotes(__instance, b);
                b.Begin(blendState: BlendState.AlphaBlend, samplerState: SamplerState.PointClamp);

                // private Item hoverItem;
                var hoverItemField = _modHelper.Reflection.GetField<Item>(__instance, "hoverItem");
                var hoverItem = hoverItemField.GetValue();

                // private string hoverText = "";
                var hoverTextField = _modHelper.Reflection.GetField<string>(__instance, "hoverText");
                var hoverText = hoverTextField.GetValue();

                // private CraftingRecipe hoverCraftingRecipe;
                var hoverCraftingRecipeField = _modHelper.Reflection.GetField<CraftingRecipe>(__instance, "hoverCraftingRecipe");
                var hoverCraftingRecipe = hoverCraftingRecipeField.GetValue();

                // private int value;
                var valueField = _modHelper.Reflection.GetField<int>(__instance, "value");
                var value = valueField.GetValue();

                if (hoverItem != null)
                {
                    var hoverItemText = hoverItem.getDescription();
                    var hoverTitle = hoverItem.DisplayName;
                    if (hoverItemText.Contains("{0}"))
                    {
                        var str1 = Game1.content.LoadStringReturnNullIfNotFound("Strings\\Objects:" + hoverItem.Name + "_CollectionsTabDescription");
                        if (str1 != null)
                        {
                            hoverItemText = str1;
                        }
                        var str2 = Game1.content.LoadStringReturnNullIfNotFound("Strings\\Objects:" + hoverItem.Name + "_CollectionsTabName");
                        if (str2 != null)
                        {
                            hoverTitle = str2;
                        }
                    }
                    IClickableMenu.drawToolTip(b, hoverItemText, hoverTitle, hoverItem, craftingIngredients: hoverCraftingRecipe);
                }
                else if (!hoverText.Equals(""))
                {
                    IClickableMenu.drawHoverText(b, hoverText, Game1.smallFont, moneyAmountToDisplayAtBottom: value);
                    if (__instance.secretNoteImage != -1)
                    {
                        IClickableMenu.drawTextureBox(b, Game1.getOldMouseX(), Game1.getOldMouseY() + 64 + 32, 288, 288, Color.White);
                        b.Draw(__instance.secretNoteImageTexture, new Vector2(Game1.getOldMouseX() + 16, Game1.getOldMouseY() + 64 + 32 + 16), new Rectangle?(new Rectangle(__instance.secretNoteImage * 64 % __instance.secretNoteImageTexture.Width, __instance.secretNoteImage * 64 / __instance.secretNoteImageTexture.Width * 64, 64, 64)), Color.White, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, 0.865f);
                    }
                }
                __instance.letterviewerSubMenu?.draw(b);
                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(Draw_AddArchipelagoLogoOnSecretNotes_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        private static void DrawSecretNotes(CollectionsPage collectionsPage, SpriteBatch b)
        {
            b.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp);
            foreach (var textureComponent in collectionsPage.collections[collectionsPage.currentTab][collectionsPage.currentPage])
            {
                var strArray = ArgUtility.SplitBySpace(textureComponent.name);
                var boolean = Convert.ToBoolean(strArray[1]);
                var color = GetDrawColor(collectionsPage, textureComponent, strArray, boolean);
                textureComponent.draw(b, color, 0.86f);
                if (collectionsPage.currentTab == 5 & boolean)
                {
                    var num = Utility.CreateRandom(Convert.ToInt32(strArray[0])).Next(12);
                    b.Draw(Game1.mouseCursors, new Vector2(textureComponent.bounds.X + 16 + 16, textureComponent.bounds.Y + 20 + 16), new Rectangle?(new Rectangle(256 + num % 6 * 64 / 2, 128 + num / 6 * 64 / 2, 32, 32)), Color.White, 0.0f, new Vector2(16f, 16f), textureComponent.scale, SpriteEffects.None, 0.88f);
                }
            }
            b.End();
            b.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp);
            foreach (var textureComponent in collectionsPage.collections[collectionsPage.currentTab][collectionsPage.currentPage])
            {
                var color = GetDrawColor(collectionsPage, textureComponent);
                DrawIndicatorOnSecretNote(textureComponent, b, color, 0.86f);
            }
            b.End();
        }

        private static Color GetDrawColor(CollectionsPage __instance, ClickableTextureComponent textureComponent)
        {
            var strArray = ArgUtility.SplitBySpace(textureComponent.name);
            var boolean = Convert.ToBoolean(strArray[1]);
            return GetDrawColor(__instance, textureComponent, strArray, boolean);
        }

        private static Color GetDrawColor(CollectionsPage __instance, ClickableTextureComponent textureComponent, string[] strArray, bool boolean)
        {
            var flag = __instance.currentTab == 4 && Convert.ToBoolean(strArray[2]) || __instance.currentTab == 5 && !boolean && textureComponent.hoverText != "???";
            var color = flag ? Color.DimGray * 0.4f : (boolean ? Color.White : Color.Black * 0.2f);
            return color;
        }

        private static void DrawIndicatorOnSecretNote(ClickableTextureComponent component, SpriteBatch spriteBatch, Color color, float layerDepth, int xOffset = 0, int yOffset = 0)
        {
            try
            {
                var secretNoteComponentName = component.name;
                if (!int.TryParse(secretNoteComponentName.Split(" ").First(), out var number))
                {
                    return;
                }

                var locationX = component.bounds.X + xOffset + component.sourceRect.Width / 2;// * component.baseScale;
                var locationY = component.bounds.Y + yOffset + component.sourceRect.Height / 2;// * component.baseScale;
                var location = new Vector2(locationX, locationY);
                var origin = new Vector2(component.sourceRect.Width / 2, component.sourceRect.Height / 2);
                var locationName = number > 1000 ? $"Journal Scrap #{number-1000}" : $"Secret Note #{number}:";
                ItemDrawInMenuPostfix(locationName, spriteBatch, location, 1.0f, 1f, layerDepth, color, new Vector2(0, 0), origin);

                return;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(DrawIndicatorOnSecretNote)}:\n{ex}");
                return;
            }
        }

        private static void ItemDrawInMenuPostfix(Item item, SpriteBatch spriteBatch, Vector2 location,
            float scaleSize, float transparency, float layerDepth, Color color, Vector2 offset, Vector2 origin)
        {
            if (item == null)
            {
                return;
            }

            var simplifiedName = _nameSimplifier.GetSimplifiedName(item);
            ItemDrawInMenuPostfix(simplifiedName, spriteBatch, location, scaleSize, transparency, layerDepth, color, offset, origin);
        }

        private static void ItemDrawInMenuPostfix(string itemSimplifiedName, SpriteBatch spriteBatch, Vector2 location,
            float scaleSize, float transparency, float layerDepth, Color color, Vector2 offset, Vector2 origin)
        {
            ItemDrawInMenuPostfix(new[] { itemSimplifiedName }, spriteBatch, location, scaleSize, transparency, layerDepth, color, offset, origin);
        }

        private static void ItemDrawInMenuPostfix(IEnumerable<string> itemSimplifiedNames, SpriteBatch spriteBatch, Vector2 location,
            float scaleSize, float transparency, float layerDepth, Color color, Vector2 offset, Vector2 origin, bool matchExactly = false)
        {
            if (_config.ShowItemIndicators == ItemIndicatorPreference.False)
            {
                return;
            }

            Func<string, IEnumerable<string>> matchingMethod = matchExactly ? _locationChecker.GetAllLocationsNotCheckedMatchingExactly : _locationChecker.GetAllLocationsNotCheckedContainingWord;
            var allUncheckedLocations = itemSimplifiedNames.SelectMany(x => matchingMethod(x));

            allUncheckedLocations = FilterLocationsBasedOnConfig(allUncheckedLocations);

            if (!allUncheckedLocations.Any())
            {
                // _logger.LogWarning($"Skipping Indicator for {itemSimplifiedNames.First()}");
                return;
            }

            var position = location + offset;
            var sourceRectangle = new Rectangle(0, 0, 12, 12);
            var transparentColor = color * transparency;

            spriteBatch.Draw(_miniArchipelagoIcon, position, sourceRectangle, transparentColor, 0.0f, origin, scaleSize,
                SpriteEffects.None, layerDepth);
            // _logger.LogWarning($"Drawing Indicator for {itemSimplifiedNames.First()}");
            return;
        }

        // public override string getDescription()
        public static void GetDescription_AddMissingChecks_Postfix(Item __instance, ref string __result)
        {
            try
            {
                if (__instance == null || _config.ShowItemIndicators == ItemIndicatorPreference.False)
                {
                    return;
                }

                var simplifiedName = _nameSimplifier.GetSimplifiedName(__instance);
                IEnumerable<string> allUncheckedLocations = _locationChecker.GetAllLocationsNotCheckedContainingWord(simplifiedName);

                allUncheckedLocations = FilterLocationsBasedOnConfig(allUncheckedLocations);

                foreach (var uncheckedLocation in allUncheckedLocations)
                {
                    __result += $"{Environment.NewLine}{uncheckedLocation}";
                }

                return;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(GetDescription_AddMissingChecks_Postfix)}:\n{ex}");
                return;
            }
        }

        private static IEnumerable<string> FilterLocationsBasedOnConfig(IEnumerable<string> allUncheckedLocations)
        {
            if (_config.ShowItemIndicators == ItemIndicatorPreference.OnlyShipsanity)
            {
                return allUncheckedLocations.Where(x =>
                    x.Contains(NightShippingBehaviors.SHIPSANITY_PREFIX,
                        StringComparison.InvariantCultureIgnoreCase)).ToArray();
            }

            return allUncheckedLocations;
        }
    }
}

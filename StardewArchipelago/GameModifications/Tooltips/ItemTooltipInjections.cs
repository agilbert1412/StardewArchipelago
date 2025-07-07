using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using KaitoKid.ArchipelagoUtilities.Net.Client;
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
                ItemDrawInMenuPostfix(__instance, spriteBatch, location, scaleSize, transparency, layerDepth, color, new Vector2(14f, 14f), new Vector2(8, 8));
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
                DrawIndicatorOnSecretNote(__instance, b, c, layerDepth, xOffset, yOffset); ;
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

        private static void DrawIndicatorOnSecretNote(ClickableTextureComponent component, SpriteBatch spriteBatch, Color color, float layerDepth, int xOffset, int yOffset)
        {
            try
            {
                if (!TryGetActiveCollectionsPage(out var collectionsPage))
                {
                    return;
                }

                // this.collections[this.currentTab][this.currentPage]
                if (collectionsPage.collections.Count <= collectionsPage.currentTab || collectionsPage.currentTab != CollectionsPage.secretNotesTab)
                {
                    return;
                }

                if (!collectionsPage.collections[collectionsPage.currentTab].Any(x => x.Contains(component)))
                {
                    return;
                }

                var secretNoteComponentName = component.name;
                var number = int.Parse(secretNoteComponentName.Split(" ").First());
                var locationX = component.bounds.X + xOffset + component.sourceRect.Width / 2;// * component.baseScale;
                var locationY = component.bounds.Y + yOffset + component.sourceRect.Height / 2;// * component.baseScale;
                var location = new Vector2(locationX, locationY);
                var origin = new Vector2(component.sourceRect.Width / 2, component.sourceRect.Height / 2);
                ItemDrawInMenuPostfix($"Secret Note #{number}:", spriteBatch, location, 1.0f, 1f, layerDepth, color, new Vector2(0, 0), origin);

                return;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(DrawIndicatorOnSecretNote)}:\n{ex}");
                return;
            }
        }

        private static bool TryGetActiveCollectionsPage(out CollectionsPage collectionsPage)
        {
            collectionsPage = null;
            var activeMenu = Game1.activeClickableMenu;
            if (activeMenu == null)
            {
                return false;
            }

            if (activeMenu is CollectionsPage)
            {
                collectionsPage = (CollectionsPage)activeMenu;
                return true;
            }

            if (Game1.activeClickableMenu is GameMenu gameMenu && gameMenu.GetCurrentPage() is CollectionsPage)
            {
                collectionsPage = (CollectionsPage)gameMenu.GetCurrentPage();
                return true;
            }

            return false;
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
            if (_config.ShowItemIndicators == ItemIndicatorPreference.False)
            {
                return;
            }

            var allUncheckedLocations = _locationChecker.GetAllLocationsNotCheckedContainingWord(itemSimplifiedName);

            allUncheckedLocations = FilterLocationsBasedOnConfig(allUncheckedLocations);

            if (!allUncheckedLocations.Any())
            {
                return;
            }

            var position = location + offset;
            var sourceRectangle = new Rectangle(0, 0, 12, 12);
            var transparentColor = color * transparency;

            spriteBatch.Draw(_miniArchipelagoIcon, position, sourceRectangle, transparentColor, 0.0f, origin, scaleSize,
                SpriteEffects.None, layerDepth);
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
                var allUncheckedLocations = _locationChecker.GetAllLocationsNotCheckedContainingWord(simplifiedName);

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

        private static string[] FilterLocationsBasedOnConfig(string[] allUncheckedLocations)
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

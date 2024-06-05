using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Menus;

namespace StardewArchipelago.GameModifications.CodeInjections.Bundles
{
    public class BundleMenuInjection
    {
        private static IMonitor _monitor;
        public static void setupLogging(IMonitor monitor)
        {
            _monitor = monitor;
        }
        
        //This is what a replacement method would be
        public static bool SetUpBundleSpecificPage(JunimoNoteMenu __instance, Bundle b)
        {
            JunimoNoteMenu.tempSprites.Clear();
            __instance.currentPageBundle = b;
            __instance.specificBundlePage = true;
            if (__instance.whichArea == 4)
            {
                if (!__instance.fromGameMenu)
                {
                    __instance.purchaseButton = new ClickableTextureComponent(new Rectangle(__instance.xPositionOnScreen + 800, __instance.yPositionOnScreen + 504, 260, 72), __instance.noteTexture, new Rectangle(517, 286, 65, 20), 4f)
                    {
                        myID = 797,
                        leftNeighborID = 103
                    };
                    if (Game1.options.SnappyMenus)
                    {
                        __instance.currentlySnappedComponent = __instance.purchaseButton;
                        __instance.snapCursorToCurrentSnappedComponent();
                    }
                }
                return false;
            }
            int numberOfIngredientSlots = b.numberOfIngredientSlots;
            List<Rectangle> ingredientSlotRectangles = new List<Rectangle>();
            var addRectangleRowsToList = typeof(JunimoNoteMenu).GetMethod("addRectangleRowsToList", BindingFlags.Instance | BindingFlags.NonPublic);
            addRectangleRowsToList.Invoke(__instance, new object[] { ingredientSlotRectangles, numberOfIngredientSlots, 932, 540 });
            for (int k = 0; k < ingredientSlotRectangles.Count; k++)
            {
                __instance.ingredientSlots.Add(new ClickableTextureComponent(ingredientSlotRectangles[k], __instance.noteTexture, new Rectangle(512, 244, 18, 18), 4f)
                {
                    myID = k + 250,
                    upNeighborID = -99998,
                    rightNeighborID = -99998,
                    leftNeighborID = -99998,
                    downNeighborID = -99998
                });
            }
            List<Rectangle> ingredientListRectangles = new List<Rectangle>();
            addRectangleRowsToList.Invoke(__instance, new object[] { ingredientListRectangles, b.ingredients.Count, 932, 364 });
            for (int j = 0; j < ingredientListRectangles.Count; j++)
            {
                BundleIngredientDescription ingredient = b.ingredients[j];
                string id = JunimoNoteMenu.GetRepresentativeItemId(ingredient);
                ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem(id);
                // skip `if (itemData.HasTypeObject())` there
                string displayName = ingredient.category switch
                {
                    -2 => Game1.content.LoadString("Strings\\StringsFromCSFiles:CraftingRecipe.cs.569"),
                    -75 => Game1.content.LoadString("Strings\\StringsFromCSFiles:CraftingRecipe.cs.570"),
                    -4 => Game1.content.LoadString("Strings\\StringsFromCSFiles:CraftingRecipe.cs.571"),
                    -5 => Game1.content.LoadString("Strings\\StringsFromCSFiles:CraftingRecipe.cs.572"),
                    -6 => Game1.content.LoadString("Strings\\StringsFromCSFiles:CraftingRecipe.cs.573"),
                    _ => itemData.DisplayName,
                };
                Item item;
                if (ingredient.preservesId != null)
                {
                    item = Utility.CreateFlavoredItem(ingredient.id, ingredient.preservesId, ingredient.quality, ingredient.stack);
                    displayName = item.DisplayName;
                }
                else
                {
                    item = ItemRegistry.Create(id, ingredient.stack, ingredient.quality);
                }
                Texture2D texture = itemData.GetTexture();
                Rectangle sourceRect = itemData.GetSourceRect();
                __instance.ingredientList.Add(new ClickableTextureComponent("ingredient_list_slot", ingredientListRectangles[j], "", displayName, texture, sourceRect, 4f)
                {
                    myID = j + 1000,
                    item = item,
                    upNeighborID = -99998,
                    rightNeighborID = -99998,
                    leftNeighborID = -99998,
                    downNeighborID = -99998,
                });
            }
            var updateIngredientSlots = typeof(JunimoNoteMenu).GetMethod("updateIngredientSlots", BindingFlags.Instance | BindingFlags.NonPublic);
            updateIngredientSlots.Invoke(__instance, System.Array.Empty<object>());
            if (!Game1.options.SnappyMenus)
            {
                return false;
            }
            __instance.populateClickableComponentList();
            if (__instance.inventory?.inventory != null)
            {
                for (int i = 0; i < __instance.inventory.inventory.Count; i++)
                {
                    if (__instance.inventory.inventory[i] != null)
                    {
                        if (__instance.inventory.inventory[i].downNeighborID == 101)
                        {
                            __instance.inventory.inventory[i].downNeighborID = -1;
                        }
                        if (__instance.inventory.inventory[i].leftNeighborID == -1)
                        {
                            __instance.inventory.inventory[i].leftNeighborID = 103;
                        }
                        if (__instance.inventory.inventory[i].upNeighborID >= 1000)
                        {
                            __instance.inventory.inventory[i].upNeighborID = 103;
                        }
                    }
                }
            }
            __instance.currentlySnappedComponent = __instance.getComponentWithID(0);
            __instance.snapCursorToCurrentSnappedComponent();
            return false;
        }
        
        //Transpiler way
        public static IEnumerable<CodeInstruction> SkipObjectCheck(IEnumerable<CodeInstruction> instructions)
        {
            // We want to skip `if (dataOrErrorItem.HasTypeObject())`, just after `ParsedItemData dataOrErrorItem = ItemRegistry.GetDataOrErrorItem(representativeItemId);`
            // In 1.6.8.24119, it looks like this
            // IL_019c: ldloc.s 7
            // IL_019e: call bool StardewValley.Extensions.ItemExtensions::HasTypeObject(class StardewValley.ItemTypeDefinitions.IHaveItemTypeId)
            // IL_01a3: brfalse IL_0330

            var getDataOrErrorItemMethodInfo = AccessTools.Method(typeof(ItemExtensions), nameof(ItemExtensions.HasTypeObject));

            var foundMethod = false;
            Label? label = null;
            var enumerator = instructions.GetEnumerator();
            enumerator.MoveNext();
            var previousInstruction = enumerator.Current;
            while (enumerator.MoveNext())
            {
                var newInstruction = enumerator.Current;
                // Recognize the call of the method
                if (newInstruction.Calls(getDataOrErrorItemMethodInfo))
                {
                    if (foundMethod) // Second trigger of this method, which means Stardew code added a second unexpected call
                    {
                        _monitor.Log("Community Center object checking has been skipped more than once", LogLevel.Error);
                    }
                    foundMethod = true;
                    enumerator.MoveNext();
                    label = (Label)enumerator.Current.operand;
                    enumerator.MoveNext();
                    previousInstruction = enumerator.Current;
                }
                else
                {
                    if (label is not null)
                    {
                        if (previousInstruction.labels.Remove(label.Value))
                        {
                            label = null;
                        }
                    }
                    yield return previousInstruction;
                    previousInstruction = newInstruction;
                }
            }
            yield return previousInstruction;
            enumerator.Dispose();

            if (!foundMethod)
            {
                _monitor.Log("Community Center couldn't be patched to show anything", LogLevel.Error);
            }
            if (label != null)
            {
                _monitor.Log("The label somehow couldn't be cleared, expect issues on the Community Center bundles", LogLevel.Error);
            }
        }
    }
}

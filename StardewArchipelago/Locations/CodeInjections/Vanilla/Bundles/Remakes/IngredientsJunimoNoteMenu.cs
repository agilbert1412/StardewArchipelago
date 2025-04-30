using StardewValley.Menus;
using StardewValley;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using KaitoKid.ArchipelagoUtilities.Net;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Logging;
using StardewArchipelago.Serialization;
using StardewArchipelago.Stardew;
using StardewModdingAPI;
using System;
using System.Linq;
using StardewValley.BellsAndWhistles;
using StardewValley.Locations;
using Microsoft.Xna.Framework.Graphics;

namespace StardewArchipelago.Locations.CodeInjections.Vanilla.Bundles.Remakes
{
    internal class IngredientsJunimoNoteMenu : ArchipelagoJunimoNoteMenu
    {
        public List<ClickableTextureComponent> IngredientSlots = new();
        public List<ClickableTextureComponent> IngredientList = new();
        public Item PartialDonationItem;
        public List<Item> PartialDonationComponents = new();
        public BundleIngredientDescription? CurrentPartialIngredientDescription;
        public int CurrentPartialIngredientDescriptionIndex = -1;

        public IngredientsJunimoNoteMenu(bool fromGameMenu, int area = 1, bool fromThisMenu = false) : base(fromGameMenu, area, fromThisMenu)
        {
        }

        public IngredientsJunimoNoteMenu(int whichArea, Dictionary<int, bool[]> bundlesComplete) : base(whichArea, bundlesComplete)
        {
        }

        public IngredientsJunimoNoteMenu(BundleRemake bundle, string noteTexturePath) : base(bundle, noteTexturePath)
        {
        }

        protected override void SetUpBundleSpecificPage(BundleRemake b)
        {
            base.SetUpBundleSpecificPage(b);
            SetUpIngredientButtons(b);
        }

        private void SetUpIngredientButtons(BundleRemake b)
        {
            var ofIngredientSlots = b.NumberOfIngredientSlots;
            var toAddTo1 = new List<Rectangle>();
            AddRectangleRowsToList(toAddTo1, ofIngredientSlots, 932, 540);
            for (var index = 0; index < toAddTo1.Count; ++index)
            {
                var ingredientSlots = this.IngredientSlots;
                var textureComponent = new ClickableTextureComponent(toAddTo1[index], NoteTexture, new Rectangle(512, 244, 18, 18), 4f);
                textureComponent.myID = index + REGION_INGREDIENT_SLOT_MODIFIER;
                textureComponent.upNeighborID = -99998;
                textureComponent.rightNeighborID = -99998;
                textureComponent.leftNeighborID = -99998;
                textureComponent.downNeighborID = -99998;
                ingredientSlots.Add(textureComponent);
            }
            var toAddTo2 = new List<Rectangle>();
            AddRectangleRowsToList(toAddTo2, b.Ingredients.Count, 932, 364);
            for (var index = 0; index < toAddTo2.Count; ++index)
            {
                var ingredient = b.Ingredients[index];
                var representativeItemId = GetRepresentativeItemId(ingredient);
                var dataOrErrorItem = ItemRegistry.GetDataOrErrorItem(representativeItemId);
                // StardewArchipelago: Patch to allow any object type in a bundle
                //if (dataOrErrorItem.HasTypeObject())
                //{
                var category = ingredient.category;
                string hoverText;
                if (category.HasValue)
                {
                    switch (category.GetValueOrDefault())
                    {
                        case -75:
                            hoverText = Game1.content.LoadString("Strings\\StringsFromCSFiles:CraftingRecipe.cs.570");
                            goto label_18;
                        case -6:
                            hoverText = Game1.content.LoadString("Strings\\StringsFromCSFiles:CraftingRecipe.cs.573");
                            goto label_18;
                        case -5:
                            hoverText = Game1.content.LoadString("Strings\\StringsFromCSFiles:CraftingRecipe.cs.572");
                            goto label_18;
                        case -4:
                            hoverText = Game1.content.LoadString("Strings\\StringsFromCSFiles:CraftingRecipe.cs.571");
                            goto label_18;
                        case -2:
                            hoverText = Game1.content.LoadString("Strings\\StringsFromCSFiles:CraftingRecipe.cs.569");
                            goto label_18;
                    }
                }
                hoverText = dataOrErrorItem.DisplayName;
            label_18:
                Item flavoredItem;
                if (ingredient.preservesId != null)
                {
                    flavoredItem = Utility.CreateFlavoredItem(ingredient.id, ingredient.preservesId, ingredient.quality, ingredient.stack);
                    hoverText = flavoredItem.DisplayName;
                }
                else
                {
                    flavoredItem = ItemRegistry.Create(representativeItemId, ingredient.stack, ingredient.quality);
                }
                var texture = dataOrErrorItem.GetTexture();
                var sourceRect = dataOrErrorItem.GetSourceRect();
                var ingredientList = this.IngredientList;
                var textureComponent = new ClickableTextureComponent("ingredient_list_slot", toAddTo2[index], "", hoverText, texture, sourceRect, 4f);
                textureComponent.myID = index + REGION_INGREDIENT_LIST_MODIFIER;
                textureComponent.item = flavoredItem;
                textureComponent.upNeighborID = -99998;
                textureComponent.rightNeighborID = -99998;
                textureComponent.leftNeighborID = -99998;
                textureComponent.downNeighborID = -99998;
                ingredientList.Add(textureComponent);
                //}
            }
            UpdateIngredientSlots();
            if (!Game1.options.SnappyMenus)
            {
                return;
            }
            populateClickableComponentList();
            if (Inventory?.inventory != null)
            {
                for (var index = 0; index < Inventory.inventory.Count; ++index)
                {
                    if (Inventory.inventory[index] != null)
                    {
                        if (Inventory.inventory[index].downNeighborID == REGION_AREA_NEXT_BUTTON)
                        {
                            Inventory.inventory[index].downNeighborID = -1;
                        }
                        if (Inventory.inventory[index].leftNeighborID == -1)
                        {
                            Inventory.inventory[index].leftNeighborID = REGION_BACK_BUTTON;
                        }
                        if (Inventory.inventory[index].upNeighborID >= REGION_INGREDIENT_LIST_MODIFIER)
                        {
                            Inventory.inventory[index].upNeighborID = REGION_BACK_BUTTON;
                        }
                    }
                }
            }
            currentlySnappedComponent = getComponentWithID(0);
            snapCursorToCurrentSnappedComponent();
        }

        public override void TakeDownBundleSpecificPage()
        {
            base.TakeDownBundleSpecificPage();
            IngredientSlots.Clear();
            IngredientList.Clear();
        }

        protected override bool TryReceiveLeftClickInBundleArea(int x, int y)
        {
            if (PartialDonationItem != null)
            {
                if (HeldItem != null && Game1.oldKBState.IsKeyDown(Keys.LeftShift))
                {
                    for (var index = 0; index < IngredientSlots.Count; ++index)
                    {
                        if (IngredientSlots[index].item == PartialDonationItem)
                        {
                            HandlePartialDonation(HeldItem, IngredientSlots[index]);
                        }
                    }
                }
                else
                {
                    for (var index = 0; index < IngredientSlots.Count; ++index)
                    {
                        if (IngredientSlots[index].containsPoint(x, y) && IngredientSlots[index].item == PartialDonationItem)
                        {
                            if (HeldItem != null)
                            {
                                HandlePartialDonation(HeldItem, IngredientSlots[index]);
                                return true;
                            }
                            ReturnPartialDonations(!Game1.oldKBState.IsKeyDown(Keys.LeftShift));
                            return true;
                        }
                    }
                }
            }
            else if (HeldItem != null)
            {
                if (Game1.oldKBState.IsKeyDown(Keys.LeftShift))
                {
                    for (var index = 0; index < IngredientSlots.Count; ++index)
                    {
                        if (CurrentPageBundle.CanAcceptThisItem(HeldItem, IngredientSlots[index]))
                        {
                            if (IngredientSlots[index].item == null)
                            {
                                HeldItem = CurrentPageBundle.TryToDepositThisItem(HeldItem, IngredientSlots[index], NOTE_TEXTURE_NAME, this);
                                CheckIfBundleIsComplete();
                                return true;
                            }
                        }
                        else if (IngredientSlots[index].item == null)
                        {
                            HandlePartialDonation(HeldItem, IngredientSlots[index]);
                        }
                    }
                }
                for (var index = 0; index < IngredientSlots.Count; ++index)
                {
                    if (IngredientSlots[index].containsPoint(x, y))
                    {
                        if (CurrentPageBundle.CanAcceptThisItem(HeldItem, IngredientSlots[index]))
                        {
                            HeldItem = CurrentPageBundle.TryToDepositThisItem(HeldItem, IngredientSlots[index], NOTE_TEXTURE_NAME, this);
                            CheckIfBundleIsComplete();
                        }
                        else if (IngredientSlots[index].item == null)
                        {
                            HandlePartialDonation(HeldItem, IngredientSlots[index]);
                        }
                    }
                }
            }

            return false;
        }

        public virtual void ReturnPartialDonation(Item item, bool playSound = true)
        {
            var affectedItemsList = new List<Item>();
            var inventory = Game1.player.addItemToInventory(item, affectedItemsList);
            foreach (var obj in affectedItemsList)
            {
                this.Inventory.ShakeItem(obj);
            }
            if (inventory != null)
            {
                Utility.CollectOrDrop(inventory);
                this.Inventory.ShakeItem(inventory);
            }
            if (!playSound)
            {
                return;
            }
            Game1.playSound("coin");
        }

        public override void ReturnPartialDonations(bool toHand = true)
        {
            base.ReturnPartialDonations(toHand);
            if (PartialDonationComponents.Count > 0)
            {
                var playSound = true;
                foreach (var donationComponent in PartialDonationComponents)
                {
                    if (HeldItem == null & toHand)
                    {
                        Game1.playSound("dwop");
                        HeldItem = donationComponent;
                    }
                    else
                    {
                        ReturnPartialDonation(donationComponent, playSound);
                        playSound = false;
                    }
                }
            }
            ResetPartialDonation();
        }

        public virtual void ResetPartialDonation()
        {
            PartialDonationComponents.Clear();
            CurrentPartialIngredientDescription = new BundleIngredientDescription?();
            CurrentPartialIngredientDescriptionIndex = -1;
            foreach (var ingredientSlot in IngredientSlots)
            {
                if (ingredientSlot.item == PartialDonationItem)
                {
                    ingredientSlot.item = null;
                }
            }
            PartialDonationItem = null;
        }

        public virtual void HandlePartialDonation(Item item, ClickableTextureComponent slot)
        {
            if (CurrentPageBundle != null && !CurrentPageBundle.DepositsAllowed || PartialDonationItem != null && slot.item != PartialDonationItem || !CanBePartiallyOrFullyDonated(item))
            {
                return;
            }
            if (!CurrentPartialIngredientDescription.HasValue)
            {
                CurrentPartialIngredientDescriptionIndex = CurrentPageBundle.GetBundleIngredientDescriptionIndexForItem(item);
                if (CurrentPartialIngredientDescriptionIndex != -1)
                {
                    CurrentPartialIngredientDescription = CurrentPageBundle.Ingredients[CurrentPartialIngredientDescriptionIndex];
                }
            }
            if (!CurrentPartialIngredientDescription.HasValue || !CurrentPageBundle.IsValidItemForThisIngredientDescription(item, CurrentPartialIngredientDescription.Value))
            {
                return;
            }
            var flag1 = true;
            var flag2 = item == HeldItem;
            int amount;
            if (slot.item == null)
            {
                Game1.playSound("sell");
                flag1 = false;
                PartialDonationItem = item.getOne();
                amount = Math.Min(CurrentPartialIngredientDescription.Value.stack, item.Stack);
                PartialDonationItem.Stack = amount;
                item = item.ConsumeStack(amount);
                PartialDonationItem.Quality = CurrentPartialIngredientDescription.Value.quality;
                slot.item = PartialDonationItem;
                slot.sourceRect.X = 512;
                slot.sourceRect.Y = 244;
            }
            else
            {
                amount = Math.Min(CurrentPartialIngredientDescription.Value.stack - PartialDonationItem.Stack, item.Stack);
                PartialDonationItem.Stack += amount;
                item = item.ConsumeStack(amount);
            }
            if (amount > 0)
            {
                var one = HeldItem.getOne();
                one.Stack = amount;
                foreach (var donationComponent in PartialDonationComponents)
                {
                    if (donationComponent.canStackWith(HeldItem))
                    {
                        one.Stack = donationComponent.addToStack(one);
                    }
                }
                if (one.Stack > 0)
                {
                    PartialDonationComponents.Add(one);
                }
                PartialDonationComponents.Sort((a, b) => b.Stack.CompareTo(a.Stack));
            }
            if (flag2 && item == null)
            {
                HeldItem = null;
            }
            if (PartialDonationItem.Stack >= CurrentPartialIngredientDescription.Value.stack)
            {
                slot.item = null;
                this.PartialDonationItem = CurrentPageBundle.TryToDepositThisItem(this.PartialDonationItem, slot, NOTE_TEXTURE_NAME, this);
                var partialDonationItem = this.PartialDonationItem;
                if ((partialDonationItem != null ? partialDonationItem.Stack > 0 ? 1 : 0 : 0) != 0)
                {
                    ReturnPartialDonation(this.PartialDonationItem);
                }
                this.PartialDonationItem = null;
                ResetPartialDonation();
                CheckIfBundleIsComplete();
            }
            else
            {
                if (amount <= 0 || !flag1)
                {
                    return;
                }
                Game1.playSound("sell");
            }
        }

        protected override void ReceiveRightTriggerInSpecificBundlePage()
        {
            base.ReceiveRightTriggerInSpecificBundlePage();
            var snappedComponent1 = currentlySnappedComponent;
            if ((snappedComponent1 != null ? snappedComponent1.myID < 50 ? 1 : 0 : 0) == 0)
            {
                return;
            }
            _oldTriggerSpot = currentlySnappedComponent.myID;
            var id = 250;
            foreach (var ingredientSlot in IngredientSlots)
            {
                if (ingredientSlot.item == null)
                {
                    id = ingredientSlot.myID;
                    break;
                }
            }
            setCurrentlySnappedComponentTo(id);
            snapCursorToCurrentSnappedComponent();
        }

        protected override void CloseBundlePage()
        {
            if (PartialDonationItem != null)
            {
                ReturnPartialDonations(false);
            }
            else
            {
                base.CloseBundlePage();
            }
        }

        private void UpdateIngredientSlots()
        {
            var index = 0;
            foreach (var ingredient in CurrentPageBundle.Ingredients)
            {
                if (ingredient.completed && index < IngredientSlots.Count)
                {
                    var representativeItemId = GetRepresentativeItemId(ingredient);
                    if (ingredient.preservesId != null)
                    {
                        IngredientSlots[index].item = Utility.CreateFlavoredItem(representativeItemId, ingredient.preservesId, ingredient.quality, ingredient.stack);
                    }
                    else
                    {
                        IngredientSlots[index].item = ItemRegistry.Create(representativeItemId, ingredient.stack, ingredient.quality);
                    }
                    CurrentPageBundle.IngredientDepositAnimation(IngredientSlots[index], NOTE_TEXTURE_NAME, true);
                    ++index;
                }
            }
        }

        private void CheckIfBundleIsComplete()
        {
            ReturnPartialDonations();
            if (!SpecificBundlePage || CurrentPageBundle == null)
            {
                return;
            }
            var num = 0;
            foreach (var ingredientSlot in IngredientSlots)
            {
                if (ingredientSlot.item != null && ingredientSlot.item != PartialDonationItem)
                {
                    ++num;
                }
            }
            if (num < CurrentPageBundle.NumberOfIngredientSlots)
            {
                return;
            }
            if (HeldItem != null)
            {
                Game1.player.addItemToInventory(HeldItem);
                HeldItem = null;
            }
            if (!_singleBundleMenu)
            {
                var location = Game1.RequireLocation<CommunityCenter>("CommunityCenter");
                for (var index = 0; index < location.bundles[CurrentPageBundle.BundleIndex].Length; ++index)
                    location.bundles.FieldDict[CurrentPageBundle.BundleIndex][index] = true;
                location.checkForNewJunimoNotes();
                ScreenSwipe = new ScreenSwipe(0, w: width, h: height);
                CurrentPageBundle.CompletionAnimation(this, delay: 400);
                CanClick = false;
                location.bundleRewards[CurrentPageBundle.BundleIndex] = true;
                Game1.Multiplayer.globalChatInfoMessage("Bundle");
                var flag = false;
                foreach (var bundle in Bundles)
                {
                    if (!bundle.Complete && !bundle.Equals(CurrentPageBundle))
                    {
                        flag = true;
                        break;
                    }
                }
                if (!flag)
                {
                    if (WhichArea == 6)
                    {
                        exitFunction = restoreaAreaOnExit_AbandonedJojaMart;
                    }
                    else
                    {
                        location.markAreaAsComplete(WhichArea);
                        exitFunction = restoreAreaOnExit;
                        location.areaCompleteReward(WhichArea);
                    }
                }
                else
                {
                    location.getJunimoForArea(WhichArea)?.bringBundleBackToHut(BundleRemake.GetColorFromColorIndex(CurrentPageBundle.BundleColor), location);
                }
                CheckForRewards();
            }
            else
            {
                if (OnBundleComplete == null)
                {
                    return;
                }
                OnBundleComplete(this);
            }
        }

        protected override void ReceiveRightClickInSpecificBundlePage(int x, int y)
        {
            base.ReceiveRightClickInSpecificBundlePage(x, y);
            HeldItem = Inventory.rightClick(x, y, HeldItem);
            if (PartialDonationItem != null)
            {
                for (var index = 0; index < IngredientSlots.Count; ++index)
                {
                    if (IngredientSlots[index].containsPoint(x, y) && IngredientSlots[index].item == PartialDonationItem)
                    {
                        if (PartialDonationComponents.Count > 0)
                        {
                            var one = PartialDonationComponents[0].getOne();
                            var flag = false;
                            if (HeldItem == null)
                            {
                                HeldItem = one;
                                Game1.playSound("dwop");
                                flag = true;
                            }
                            else if (HeldItem.canStackWith(one))
                            {
                                HeldItem.addToStack(one);
                                Game1.playSound("dwop");
                                flag = true;
                            }
                            if (flag)
                            {
                                if (PartialDonationComponents[0].ConsumeStack(1) == null)
                                {
                                    PartialDonationComponents.RemoveAt(0);
                                }
                                if (PartialDonationItem != null)
                                {
                                    var num = 0;
                                    foreach (var donationComponent in PartialDonationComponents)
                                    {
                                        num += donationComponent.Stack;
                                    }
                                    PartialDonationItem.Stack = num;
                                }
                                if (PartialDonationComponents.Count == 0)
                                {
                                    ResetPartialDonation();
                                }
                            }
                        }
                        break;
                    }
                }
            }
        }

        protected override void PerformHoverActionInSpecificBundlePage(int x, int y)
        {
            base.PerformHoverActionInSpecificBundlePage(x, y);
            foreach (var ingredient in IngredientList)
            {
                if (ingredient.bounds.Contains(x, y))
                {
                    HoverText = ingredient.hoverText;
                    break;
                }
            }
            if (HeldItem != null)
            {
                foreach (var ingredientSlot in IngredientSlots)
                {
                    if (ingredientSlot.bounds.Contains(x, y) && CanBePartiallyOrFullyDonated(HeldItem) && (PartialDonationItem == null || ingredientSlot.item == PartialDonationItem))
                    {
                        ingredientSlot.sourceRect.X = 530;
                        ingredientSlot.sourceRect.Y = 262;
                    }
                    else
                    {
                        ingredientSlot.sourceRect.X = 512;
                        ingredientSlot.sourceRect.Y = 244;
                    }
                }
            }
        }

        protected virtual void DrawBundleRequirements(SpriteBatch b)
        {
            base.DrawBundleRequirements(b);
            DrawTempSprites(b);
            DrawIngredientSlots(b);
            DrawIngredients(b);
        }

        private void DrawTempSprites(SpriteBatch b)
        {
            var extraAlpha = 1f;
            if (PartialDonationItem != null)
            {
                extraAlpha = 0.25f;
            }

            foreach (var tempSprite in TempSprites)
            {
                tempSprite.draw(b, true, extraAlpha: extraAlpha);
            }
        }

        private void DrawIngredientSlots(SpriteBatch b)
        {

            foreach (var ingredientSlot in IngredientSlots)
            {
                var alpha = 1f;
                if (PartialDonationItem != null && ingredientSlot.item != PartialDonationItem)
                {
                    alpha = 0.25f;
                }
                if (ingredientSlot.item == null || PartialDonationItem != null && ingredientSlot.item == PartialDonationItem)
                {
                    ingredientSlot.draw(b, (FromGameMenu ? Color.LightGray * 0.5f : Color.White) * alpha, 0.89f);
                }
                ingredientSlot.drawItem(b, 4, 4, alpha);
            }
        }

        private void DrawIngredients(SpriteBatch b)
        {
            for (var index = 0; index < IngredientList.Count; ++index)
            {
                var num3 = 1f;
                if (CurrentPartialIngredientDescriptionIndex >= 0 && CurrentPartialIngredientDescriptionIndex != index)
                {
                    num3 = 0.25f;
                }
                var ingredient = IngredientList[index];
                var flag = false;
                var num4 = index;
                var count = CurrentPageBundle?.Ingredients?.Count;
                var valueOrDefault = count.GetValueOrDefault();
                if (num4 < valueOrDefault & count.HasValue && CurrentPageBundle.Ingredients[index].completed)
                {
                    flag = true;
                }
                if (!flag)
                {
                    b.Draw(Game1.shadowTexture, new Vector2(ingredient.bounds.Center.X - Game1.shadowTexture.Bounds.Width * 4 / 2 - 4, ingredient.bounds.Center.Y + 4), Game1.shadowTexture.Bounds, Color.White * num3, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, 0.1f);
                }
                if (ingredient.item != null && ingredient.visible)
                {
                    ingredient.item.drawInMenu(b, new Vector2(ingredient.bounds.X, ingredient.bounds.Y), ingredient.scale / 4f, 1f, 0.9f, StackDrawType.Draw, Color.White * (flag ? 0.25f : num3), false);
                }
            }
        }

        protected override void GameWindowSizeChangedForSpecificBundle()
        {
            base.GameWindowSizeChangedForSpecificBundle();
            if (!SpecificBundlePage)
            {
                return;
            }

            var ingredientSlotsRectangles = new List<Rectangle>();
            AddRectangleRowsToList(ingredientSlotsRectangles, CurrentPageBundle.NumberOfIngredientSlots, 932, 540);
            IngredientSlots.Clear();
            for (var index = 0; index < ingredientSlotsRectangles.Count; ++index)
            {
                IngredientSlots.Add(new ClickableTextureComponent(ingredientSlotsRectangles[index], NoteTexture, new Rectangle(512, 244, 18, 18), 4f));
            }
            var ingredientRectangles = new List<Rectangle>();
            IngredientList.Clear();
            AddRectangleRowsToList(ingredientRectangles, CurrentPageBundle.Ingredients.Count, 932, 364);
            for (var index = 0; index < ingredientRectangles.Count; ++index)
            {
                var ingredient = CurrentPageBundle.Ingredients[index];
                var metadata = ItemRegistry.GetMetadata(ingredient.id);
                if (metadata?.TypeIdentifier == "(O)")
                {
                    var parsedOrErrorData = metadata.GetParsedOrErrorData();
                    var texture = parsedOrErrorData.GetTexture();
                    var sourceRect = parsedOrErrorData.GetSourceRect();
                    var obj = ingredient.preservesId != null ? Utility.CreateFlavoredItem(ingredient.id, ingredient.preservesId, ingredient.quality, ingredient.stack) : ItemRegistry.Create(ingredient.id, ingredient.stack, ingredient.quality);
                    var ingredientList = this.IngredientList;
                    var textureComponent = new ClickableTextureComponent("", ingredientRectangles[index], "", obj.DisplayName, texture, sourceRect, 4f);
                    textureComponent.myID = index + REGION_INGREDIENT_LIST_MODIFIER;
                    textureComponent.item = obj;
                    textureComponent.upNeighborID = -99998;
                    textureComponent.rightNeighborID = -99998;
                    textureComponent.leftNeighborID = -99998;
                    textureComponent.downNeighborID = -99998;
                    ingredientList.Add(textureComponent);
                }
            }
            UpdateIngredientSlots();
        }

        public override bool IsAutomaticSnapValid(
            int direction,
            ClickableComponent a,
            ClickableComponent b)
        {
            // What the fuck is this
            var thereIsNoPartialDonation = CurrentPartialIngredientDescriptionIndex < 0;
            var itemBIsAPartialDonation = b.item == PartialDonationItem;
            var bCanGoInEmptySlot = !IngredientSlots.Contains(b) || itemBIsAPartialDonation;
            var bIsIngredient = IngredientList.Contains(b);
            var bIsNotPartialDonation = !bIsIngredient || IngredientList.IndexOf(b as ClickableTextureComponent) == CurrentPartialIngredientDescriptionIndex;
            var aIsInBundleRegion = a.myID is >= REGION_BUNDLE_MODIFIER or >= REGION_AREA_NEXT_BUTTON or REGION_AREA_BACK_BUTTON;
            var bIsInBundleRegion = b.myID is >= REGION_BUNDLE_MODIFIER or >= REGION_AREA_NEXT_BUTTON or REGION_AREA_BACK_BUTTON;
            var sameRegion = aIsInBundleRegion == bIsInBundleRegion;
            return (thereIsNoPartialDonation || bCanGoInEmptySlot && bIsNotPartialDonation) && base.IsAutomaticSnapValid(direction, a, b);
        }

        public override bool HighlightObjects(Item item)
        {
            if (CurrentPageBundle != null)
            {
                if (PartialDonationItem != null && CurrentPartialIngredientDescriptionIndex >= 0)
                {
                    return CurrentPageBundle.IsValidItemForThisIngredientDescription(item, CurrentPageBundle.Ingredients[CurrentPartialIngredientDescriptionIndex]);
                }
                foreach (var ingredient in CurrentPageBundle.Ingredients)
                {
                    if (CurrentPageBundle.IsValidItemForThisIngredientDescription(item, ingredient))
                    {
                        return true;
                    }
                }
            }

            return base.HighlightObjects(item);
        }

        public virtual bool CanBePartiallyOrFullyDonated(Item item)
        {
            if (CurrentPageBundle == null)
            {
                return false;
            }
            var descriptionIndexForItem = CurrentPageBundle.GetBundleIngredientDescriptionIndexForItem(item);
            if (descriptionIndexForItem < 0)
            {
                return false;
            }
            var ingredient = CurrentPageBundle.Ingredients[descriptionIndexForItem];
            var num = 0;
            if (CurrentPageBundle.IsValidItemForThisIngredientDescription(item, ingredient))
            {
                num += item.Stack;
            }
            foreach (var obj in Game1.player.Items)
            {
                if (CurrentPageBundle.IsValidItemForThisIngredientDescription(obj, ingredient))
                {
                    num += obj.Stack;
                }
            }
            if (descriptionIndexForItem == CurrentPartialIngredientDescriptionIndex && PartialDonationItem != null)
            {
                num += PartialDonationItem.Stack;
            }
            return num >= ingredient.stack;
        }
    }
}

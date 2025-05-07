using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewArchipelago.Bundles;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Extensions;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Locations;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
// ReSharper disable PossibleLossOfFraction

#nullable disable
namespace StardewArchipelago.Locations.CodeInjections.Vanilla.Bundles.Remakes
{
    public class JunimoNoteMenuRemake : IClickableMenu
    {
        public const int REGION_INGREDIENT_SLOT_MODIFIER = 250;
        public const int REGION_INGREDIENT_LIST_MODIFIER = 1000;
        public const int REGION_BUNDLE_MODIFIER = 5000;
        public const int REGION_AREA_NEXT_BUTTON = 101;
        public const int REGION_AREA_BACK_BUTTON = 102;
        public const int REGION_BACK_BUTTON = 103;
        public const int REGION_PURCHASE_BUTTON = 104;
        public const int REGION_PRESENT_BUTTON = 105;
        public const int BASE_WIDTH = 320;
        public const int BASE_HEIGHT = 180;
        public const string NOTE_TEXTURE_NAME = "LooseSprites\\JunimoNote";
        public Texture2D NoteTexture;
        public bool SpecificBundlePage;
        public InventoryMenu Inventory;
        public Item PartialDonationItem;
        public List<Item> PartialDonationComponents = new();
        public BundleIngredientDescription? CurrentPartialIngredientDescription;
        public int CurrentPartialIngredientDescriptionIndex = -1;
        public Item HeldItem;
        public Item HoveredItem;
        public static bool CanClick = true;
        public int WhichArea;
        public int GameMenuTabToReturnTo = -1;
        public IClickableMenu MenuToReturnTo;
        public bool BundlesChanged;
        public static ScreenSwipe ScreenSwipe;
        public static string HoverText = "";
        public List<ArchipelagoBundle> Bundles = new();
        public static TemporaryAnimatedSpriteList TempSprites = new();
        public List<ClickableTextureComponent> IngredientSlots = new();
        public List<ClickableTextureComponent> IngredientList = new();
        public bool FromGameMenu;
        public bool FromThisMenu;
        public bool ScrambledText;
        private readonly bool _singleBundleMenu;
        public ClickableTextureComponent BackButton;
        public ClickableTextureComponent PurchaseButton;
        public ClickableTextureComponent AreaNextButton;
        public ClickableTextureComponent AreaBackButton;
        public ClickableAnimatedComponent PresentButton;
        public Action<int> OnIngredientDeposit;
        public Action<JunimoNoteMenuRemake> OnBundleComplete;
        public Action<JunimoNoteMenuRemake> OnScreenSwipeFinished;
        public ArchipelagoBundle CurrentPageBundle;
        private int _oldTriggerSpot;

        public JunimoNoteMenuRemake(bool fromGameMenu, int area = 1, bool fromThisMenu = false)
          : base(Game1.uiViewport.Width / 2 - 640, Game1.uiViewport.Height / 2 - 360, 1280, 720, true)
        {
            var communityCenter = Game1.RequireLocation<CommunityCenter>("CommunityCenter");
            if (fromGameMenu && !fromThisMenu)
            {
                for (var index = 0; index < communityCenter.areasComplete.Count; ++index)
                {
                    if (communityCenter.shouldNoteAppearInArea(index) && !communityCenter.areasComplete[index])
                    {
                        area = index;
                        WhichArea = area;
                        break;
                    }
                }
                if (Utility.doesMasterPlayerHaveMailReceivedButNotMailForTomorrow("abandonedJojaMartAccessible") && !Game1.MasterPlayer.hasOrWillReceiveMail("ccMovieTheater"))
                {
                    area = 6;
                }
            }
            SetUpMenu(area, communityCenter.bundlesDict());
            Game1.player.forceCanMove();
            var textureComponent1 = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + width - 128, yPositionOnScreen, 48, 44), Game1.mouseCursors, new Rectangle(365, 495, 12, 11), 4f);
            textureComponent1.visible = false;
            textureComponent1.myID = REGION_AREA_NEXT_BUTTON;
            textureComponent1.leftNeighborID = REGION_AREA_BACK_BUTTON;
            textureComponent1.leftNeighborImmutable = true;
            textureComponent1.downNeighborID = -99998;
            AreaNextButton = textureComponent1;
            var textureComponent2 = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + 64, yPositionOnScreen, 48, 44), Game1.mouseCursors, new Rectangle(352, 495, 12, 11), 4f);
            textureComponent2.visible = false;
            textureComponent2.myID = REGION_AREA_BACK_BUTTON;
            textureComponent2.rightNeighborID = REGION_AREA_NEXT_BUTTON;
            textureComponent2.rightNeighborImmutable = true;
            textureComponent2.downNeighborID = -99998;
            AreaBackButton = textureComponent2;
            var num = 6;
            for (var area1 = 0; area1 < num; ++area1)
            {
                if (area1 != area && communityCenter.shouldNoteAppearInArea(area1))
                {
                    AreaNextButton.visible = true;
                    AreaBackButton.visible = true;
                    break;
                }
            }
            this.FromGameMenu = fromGameMenu;
            this.FromThisMenu = fromThisMenu;
            foreach (var bundle in Bundles)
            {
                bundle.DepositsAllowed = false;
            }
            if (!Game1.options.SnappyMenus)
            {
                return;
            }
            populateClickableComponentList();
            snapToDefaultClickableComponent();
        }

        public JunimoNoteMenuRemake(int whichArea, Dictionary<int, bool[]> bundlesComplete)
          : base(Game1.uiViewport.Width / 2 - 640, Game1.uiViewport.Height / 2 - 360, 1280, 720, true)
        {
            SetUpMenu(whichArea, bundlesComplete);
            if (!Game1.options.SnappyMenus)
            {
                return;
            }
            populateClickableComponentList();
            snapToDefaultClickableComponent();
        }

        public JunimoNoteMenuRemake(ArchipelagoBundle b, string noteTexturePath)
          : base(Game1.uiViewport.Width / 2 - 640, Game1.uiViewport.Height / 2 - 360, 1280, 720, true)
        {
            _singleBundleMenu = true;
            WhichArea = -1;
            NoteTexture = Game1.temporaryContent.Load<Texture2D>(noteTexturePath);
            TempSprites.Clear();
            Inventory = new InventoryMenu(xPositionOnScreen + 128, yPositionOnScreen + 140, true, highlightMethod: HighlightObjects, capacity: 36, rows: 6, horizontalGap: 8, verticalGap: 8, drawSlots: false)
            {
                capacity = 36
            };
            for (var index = 0; index < Inventory.inventory.Count; ++index)
            {
                if (index >= Inventory.actualInventory.Count)
                {
                    Inventory.inventory[index].visible = false;
                }
            }
            foreach (var clickableComponent in Inventory.GetBorder(InventoryMenu.BorderSide.Bottom))
            {
                clickableComponent.downNeighborID = -99998;
            }
            foreach (var clickableComponent in Inventory.GetBorder(InventoryMenu.BorderSide.Right))
            {
                clickableComponent.rightNeighborID = -99998;
            }
            Inventory.dropItemInvisibleButton.visible = false;
            CanClick = true;
            SetUpBundleSpecificPage(b);
            if (!Game1.options.SnappyMenus)
            {
                return;
            }
            populateClickableComponentList();
            snapToDefaultClickableComponent();
        }

        public override void snapToDefaultClickableComponent()
        {
            if (SpecificBundlePage)
            {
                currentlySnappedComponent = getComponentWithID(0);
            }
            else
            {
                currentlySnappedComponent = getComponentWithID(REGION_BUNDLE_MODIFIER);
            }
            snapCursorToCurrentSnappedComponent();
        }

        protected override bool _ShouldAutoSnapPrioritizeAlignedElements() => !SpecificBundlePage;

        protected override void customSnapBehavior(int direction, int oldRegion, int oldId)
        {
            if (!Game1.player.hasOrWillReceiveMail("canReadJunimoText") || oldId - REGION_BUNDLE_MODIFIER < 0 || oldId - REGION_BUNDLE_MODIFIER >= 10 || currentlySnappedComponent == null)
            {
                return;
            }
            var num1 = -1;
            var num2 = 999999;
            var center1 = currentlySnappedComponent.bounds.Center;
            for (var index = 0; index < Bundles.Count; ++index)
            {
                if (Bundles[index].myID != oldId)
                {
                    var num3 = 999999;
                    var center2 = Bundles[index].bounds.Center;
                    switch (direction)
                    {
                        case 0:
                            if (center2.Y < center1.Y)
                            {
                                num3 = center1.Y - center2.Y + Math.Abs(center1.X - center2.X) * 3;
                            }
                            break;
                        case 1:
                            if (center2.X > center1.X)
                            {
                                num3 = center2.X - center1.X + Math.Abs(center1.Y - center2.Y) * 3;
                            }
                            break;
                        case 2:
                            if (center2.Y > center1.Y)
                            {
                                num3 = center2.Y - center1.Y + Math.Abs(center1.X - center2.X) * 3;
                            }
                            break;
                        case 3:
                            if (center2.X < center1.X)
                            {
                                num3 = center1.X - center2.X + Math.Abs(center1.Y - center2.Y) * 3;
                            }
                            break;
                    }
                    if (num3 < 10000 && num3 < num2)
                    {
                        num2 = num3;
                        num1 = index;
                    }
                }
            }
            if (num1 != -1)
            {
                currentlySnappedComponent = getComponentWithID(num1 + REGION_BUNDLE_MODIFIER);
                snapCursorToCurrentSnappedComponent();
            }
            else
            {
                switch (direction)
                {
                    case 1:
                        if (AreaNextButton == null || !AreaNextButton.visible)
                        {
                            break;
                        }
                        currentlySnappedComponent = AreaNextButton;
                        snapCursorToCurrentSnappedComponent();
                        AreaNextButton.leftNeighborID = oldId;
                        break;
                    case 2:
                        if (PresentButton == null)
                        {
                            break;
                        }
                        currentlySnappedComponent = PresentButton;
                        snapCursorToCurrentSnappedComponent();
                        PresentButton.upNeighborID = oldId;
                        break;
                    case 3:
                        if (AreaBackButton == null || !AreaBackButton.visible)
                        {
                            break;
                        }
                        currentlySnappedComponent = AreaBackButton;
                        snapCursorToCurrentSnappedComponent();
                        AreaBackButton.rightNeighborID = oldId;
                        break;
                }
            }
        }

        public virtual void SetUpMenu(int whichArea, Dictionary<int, bool[]> bundlesComplete)
        {
            NoteTexture = Game1.temporaryContent.Load<Texture2D>(NOTE_TEXTURE_NAME);
            if (!Game1.player.hasOrWillReceiveMail("seenJunimoNote"))
            {
                Game1.player.removeQuest("26");
                Game1.player.mailReceived.Add("seenJunimoNote");
            }
            if (!Game1.player.hasOrWillReceiveMail("wizardJunimoNote"))
            {
                Game1.addMailForTomorrow("wizardJunimoNote");
            }
            if (!Game1.player.hasOrWillReceiveMail("hasSeenAbandonedJunimoNote") && whichArea == 6)
            {
                Game1.player.mailReceived.Add("hasSeenAbandonedJunimoNote");
            }
            ScrambledText = !Game1.player.hasOrWillReceiveMail("canReadJunimoText");
            TempSprites.Clear();
            this.WhichArea = whichArea;
            Inventory = new InventoryMenu(xPositionOnScreen + 128, yPositionOnScreen + 140, true, highlightMethod: HighlightObjects, capacity: 36, rows: 6, horizontalGap: 8, verticalGap: 8, drawSlots: false)
            {
                capacity = 36
            };
            for (var index = 0; index < Inventory.inventory.Count; ++index)
            {
                if (index >= Inventory.actualInventory.Count)
                {
                    Inventory.inventory[index].visible = false;
                }
            }
            foreach (var clickableComponent in Inventory.GetBorder(InventoryMenu.BorderSide.Bottom))
            {
                clickableComponent.downNeighborID = -99998;
            }
            foreach (var clickableComponent in Inventory.GetBorder(InventoryMenu.BorderSide.Right))
            {
                clickableComponent.rightNeighborID = -99998;
            }
            Inventory.dropItemInvisibleButton.visible = false;
            var bundleData = Game1.netWorldState.Value.BundleData;
            var areaNameFromNumber = CommunityCenter.getAreaNameFromNumber(whichArea);
            var whichBundle = 0;
            foreach (var key in bundleData.Keys)
            {
                if (key.Contains(areaNameFromNumber))
                {
                    var bundles = this.Bundles;
                    var bundle = CreateBundle(bundlesComplete, key, bundleData, whichBundle);
                    bundles.Add(bundle);
                    ++whichBundle;
                }
            }
            var textureComponent = new ClickableTextureComponent("Back", new Rectangle(xPositionOnScreen + borderWidth * 2 + 8, yPositionOnScreen + borderWidth * 2 + 4, 64, 64), null, null, Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44), 1f);
            textureComponent.myID = REGION_BACK_BUTTON;
            BackButton = textureComponent;
            CheckForRewards();
            CanClick = true;
            Game1.playSound("shwip");
            var flag = false;
            foreach (var bundle in Bundles)
            {
                if (!bundle.Complete && !bundle.Equals(CurrentPageBundle))
                {
                    flag = true;
                    break;
                }
            }
            if (flag)
            {
                return;
            }
            var communityCenter = Game1.RequireLocation<CommunityCenter>("CommunityCenter");
            communityCenter.markAreaAsComplete(whichArea);
            exitFunction = restoreAreaOnExit;
            communityCenter.areaCompleteReward(whichArea);
        }

        protected virtual ArchipelagoBundle CreateBundle(Dictionary<int, bool[]> bundlesComplete, string key, Dictionary<string, string> bundleData, int whichBundle)
        {
            var int32 = Convert.ToInt32(key.Split('/')[1]);
            var bundle = new ArchipelagoBundle(int32, bundleData[key], bundlesComplete[int32], GetBundleLocationFromNumber(whichBundle), NOTE_TEXTURE_NAME, (ArchipelagoJunimoNoteMenu)this);
            bundle.myID = whichBundle + REGION_BUNDLE_MODIFIER;
            bundle.rightNeighborID = -7777;
            bundle.leftNeighborID = -7777;
            bundle.upNeighborID = -7777;
            bundle.downNeighborID = -7777;
            bundle.fullyImmutable = true;
            return bundle;
        }

        public virtual bool HighlightObjects(Item item)
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
            return false;
        }

        public override bool readyToClose()
        {
            return (!SpecificBundlePage || _singleBundleMenu) && IsReadyToCloseMenuOrBundle();
        }

        /// <inheritdoc />
        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            if (!CanClick)
            {
                return;
            }
            base.receiveLeftClick(x, y, playSound);
            if (ScrambledText)
            {
                return;
            }
            if (SpecificBundlePage)
            {
                if (ReceiveLeftClickInSpecificBundlePage(x, y))
                {
                    return;
                }
            }
            else
            {
                if (ReceiveLeftClickInBundleRoomPage(x, y))
                {
                    return;
                }
            }
            if (HeldItem == null || isWithinBounds(x, y) || !HeldItem.canBeTrashed())
            {
                return;
            }
            Game1.playSound("throwDownITem");
            Game1.createItemDebris(HeldItem, Game1.player.getStandingPosition(), Game1.player.FacingDirection);
            HeldItem = null;
        }

        protected virtual bool ReceiveLeftClickInSpecificBundlePage(int x, int y)
        {
            if (!CurrentPageBundle.Complete && CurrentPageBundle.CompletionTimer <= 0)
            {
                HeldItem = Inventory.leftClick(x, y, HeldItem);
            }
            if (BackButton != null && BackButton.containsPoint(x, y) && HeldItem == null)
            {
                CloseBundlePage();
            }
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
            ReceiveLeftClickInButtons(x, y);
            if (upperRightCloseButton != null && IsReadyToCloseMenuOrBundle() && upperRightCloseButton.containsPoint(x, y))
            {
                CloseBundlePage();
                return true;
            }
            return false;
        }

        protected virtual void ReceiveLeftClickInButtons(int x, int y)
        {
            ReceiveLeftClickPurchaseButton(x, y);
        }

        protected virtual void ReceiveLeftClickPurchaseButton(int x, int y)
        {
            if (PurchaseButton == null || !PurchaseButton.containsPoint(x, y))
            {
                return;
            }
            var stack = CurrentPageBundle.Ingredients.Last().stack;
            if (Game1.player.Money >= stack)
            {
                Game1.player.Money -= stack;
                Game1.playSound("select");
                CurrentPageBundle.CompletionAnimation(this);
                if (PurchaseButton != null)
                {
                    PurchaseButton.scale = PurchaseButton.baseScale * 0.75f;
                }
                var communityCenter = Game1.RequireLocation<CommunityCenter>("CommunityCenter");
                communityCenter.bundleRewards[CurrentPageBundle.BundleIndex] = true;
                communityCenter.bundles.FieldDict[CurrentPageBundle.BundleIndex][0] = true;
                CheckForRewards();
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
                    communityCenter.markAreaAsComplete(WhichArea);
                    exitFunction = restoreAreaOnExit;
                    communityCenter.areaCompleteReward(WhichArea);
                }
                else
                {
                    communityCenter.getJunimoForArea(WhichArea)?.bringBundleBackToHut(BundleRemake.GetColorFromColorIndex(CurrentPageBundle.BundleColor), Game1.RequireLocation("CommunityCenter"));
                }
                Game1.Multiplayer.globalChatInfoMessage("Bundle");
            }
            else
            {
                Game1.dayTimeMoneyBox.moneyShakeTimer = 600;
            }
        }

        private bool ReceiveLeftClickInBundleRoomPage(int x, int y)
        {
            foreach (var bundle in Bundles)
            {
                if (bundle.CanBeClicked() && bundle.containsPoint(x, y))
                {
                    SetUpBundleSpecificPage(bundle);
                    Game1.playSound("shwip");
                    return true;
                }
            }
            if (PresentButton != null && PresentButton.containsPoint(x, y) && !FromGameMenu && !FromThisMenu)
            {
                OpenRewardsMenu();
            }
            if (FromGameMenu)
            {
                if (AreaNextButton.containsPoint(x, y))
                {
                    SwapPage(1);
                }
                else if (AreaBackButton.containsPoint(x, y))
                {
                    SwapPage(-1);
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

        public virtual void ReturnPartialDonations(bool toHand = true)
        {
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

        public bool IsReadyToCloseMenuOrBundle()
        {
            if (SpecificBundlePage)
            {
                var currentPageBundle = this.CurrentPageBundle;
                if ((currentPageBundle != null ? currentPageBundle.CompletionTimer > 0 ? 1 : 0 : 0) != 0)
                {
                    return false;
                }
            }
            return HeldItem == null;
        }

        /// <inheritdoc />
        public override void receiveGamePadButton(Buttons button)
        {
            base.receiveGamePadButton(button);
            if (SpecificBundlePage)
            {
                switch (button)
                {
                    case Buttons.RightTrigger:
                        var snappedComponent1 = currentlySnappedComponent;
                        if ((snappedComponent1 != null ? snappedComponent1.myID < 50 ? 1 : 0 : 0) == 0)
                        {
                            break;
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
                        break;
                    case Buttons.LeftTrigger:
                        var snappedComponent2 = currentlySnappedComponent;
                        if ((snappedComponent2 != null ? snappedComponent2.myID >= REGION_INGREDIENT_SLOT_MODIFIER ? 1 : 0 : 0) == 0)
                        {
                            break;
                        }
                        setCurrentlySnappedComponentTo(_oldTriggerSpot);
                        snapCursorToCurrentSnappedComponent();
                        break;
                }
            }
            else
            {
                if (!FromGameMenu)
                {
                    return;
                }
                if (button != Buttons.RightTrigger)
                {
                    if (button != Buttons.LeftTrigger)
                    {
                        return;
                    }
                    SwapPage(-1);
                }
                else
                {
                    SwapPage(1);
                }
            }
        }

        public void SwapPage(int direction)
        {
            if (direction > 0 && !AreaNextButton.visible || direction < 0 && !AreaBackButton.visible)
            {
                return;
            }
            var communityCenter = Game1.RequireLocation<CommunityCenter>("CommunityCenter");
            var whichArea = this.WhichArea;
            var num1 = 6;
            for (var index = 0; index < num1; ++index)
            {
                whichArea += direction;
                if (whichArea < 0)
                {
                    whichArea += num1;
                }
                if (whichArea >= num1)
                {
                    whichArea -= num1;
                }
                if (communityCenter.shouldNoteAppearInArea(whichArea))
                {
                    var num2 = -1;
                    if (currentlySnappedComponent != null && (currentlySnappedComponent.myID >= REGION_BUNDLE_MODIFIER || currentlySnappedComponent.myID == REGION_AREA_NEXT_BUTTON || currentlySnappedComponent.myID == REGION_AREA_BACK_BUTTON))
                    {
                        num2 = currentlySnappedComponent.myID;
                    }
                    var junimoNoteMenu = new JunimoNoteMenuRemake(true, whichArea, true)
                    {
                        GameMenuTabToReturnTo = GameMenuTabToReturnTo
                    };
                    Game1.activeClickableMenu = junimoNoteMenu;
                    if (num2 >= 0)
                    {
                        junimoNoteMenu.currentlySnappedComponent = junimoNoteMenu.getComponentWithID(currentlySnappedComponent.myID);
                        junimoNoteMenu.snapCursorToCurrentSnappedComponent();
                    }
                    if (junimoNoteMenu.getComponentWithID(AreaNextButton.leftNeighborID) != null)
                    {
                        junimoNoteMenu.AreaNextButton.leftNeighborID = AreaNextButton.leftNeighborID;
                    }
                    else
                    {
                        junimoNoteMenu.AreaNextButton.leftNeighborID = junimoNoteMenu.AreaBackButton.myID;
                    }
                    junimoNoteMenu.AreaNextButton.rightNeighborID = AreaNextButton.rightNeighborID;
                    junimoNoteMenu.AreaNextButton.upNeighborID = AreaNextButton.upNeighborID;
                    junimoNoteMenu.AreaNextButton.downNeighborID = AreaNextButton.downNeighborID;
                    if (junimoNoteMenu.getComponentWithID(AreaBackButton.rightNeighborID) != null)
                    {
                        junimoNoteMenu.AreaBackButton.leftNeighborID = AreaBackButton.leftNeighborID;
                    }
                    else
                    {
                        junimoNoteMenu.AreaBackButton.leftNeighborID = junimoNoteMenu.AreaNextButton.myID;
                    }
                    junimoNoteMenu.AreaBackButton.rightNeighborID = AreaBackButton.rightNeighborID;
                    junimoNoteMenu.AreaBackButton.upNeighborID = AreaBackButton.upNeighborID;
                    junimoNoteMenu.AreaBackButton.downNeighborID = AreaBackButton.downNeighborID;
                    break;
                }
            }
        }

        /// <inheritdoc />
        public override void receiveKeyPress(Keys key)
        {
            if (GameMenuTabToReturnTo != -1)
            {
                closeSound = "shwip";
            }
            base.receiveKeyPress(key);
            if (key == Keys.Delete && HeldItem != null && HeldItem.canBeTrashed())
            {
                Utility.trashItem(HeldItem);
                HeldItem = null;
            }
            if (!Game1.options.doesInputListContain(Game1.options.menuButton, key) || !IsReadyToCloseMenuOrBundle())
            {
                return;
            }
            if (_singleBundleMenu)
            {
                exitThisMenu(GameMenuTabToReturnTo == -1);
            }
            CloseBundlePage();
        }

        /// <inheritdoc />
        protected override void cleanupBeforeExit()
        {
            base.cleanupBeforeExit();
            if (GameMenuTabToReturnTo != -1)
            {
                Game1.activeClickableMenu = new GameMenu(GameMenuTabToReturnTo, playOpeningSound: false);
            }
            else
            {
                if (MenuToReturnTo == null)
                {
                    return;
                }
                Game1.activeClickableMenu = MenuToReturnTo;
            }
        }

        private void CloseBundlePage()
        {
            if (PartialDonationItem != null)
            {
                ReturnPartialDonations(false);
            }
            else
            {
                if (!SpecificBundlePage)
                {
                    return;
                }
                HoveredItem = null;
                Inventory.descriptionText = "";
                if (HeldItem == null)
                {
                    TakeDownBundleSpecificPage();
                    Game1.playSound("shwip");
                }
                else
                {
                    HeldItem = Inventory.tryToAddItem(HeldItem);
                }
            }
        }

        protected virtual void ReOpenThisMenu()
        {
            var num = SpecificBundlePage ? 1 : 0;
            var junimoNoteMenu = CreateJunimoNoteMenu();
            if (num != 0)
            {
                foreach (var bundle in junimoNoteMenu.Bundles)
                {
                    if (bundle.BundleIndex == CurrentPageBundle.BundleIndex)
                    {
                        junimoNoteMenu.SetUpBundleSpecificPage(bundle);
                        break;
                    }
                }
            }
            Game1.activeClickableMenu = junimoNoteMenu;
        }

        protected virtual JunimoNoteMenuRemake CreateJunimoNoteMenu()
        {
            if (FromGameMenu || FromThisMenu)
            {
                return new JunimoNoteMenuRemake(FromGameMenu, WhichArea, FromThisMenu)
                {
                    GameMenuTabToReturnTo = GameMenuTabToReturnTo,
                    MenuToReturnTo = MenuToReturnTo,
                };
            }
            else
            {
                return new JunimoNoteMenuRemake(WhichArea, Game1.RequireLocation<CommunityCenter>("CommunityCenter").bundlesDict())
                {
                    GameMenuTabToReturnTo = GameMenuTabToReturnTo,
                    MenuToReturnTo = MenuToReturnTo,
                };
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

        /// <summary>Get the qualified item ID to draw in the bundle UI for an ingredient.</summary>
        /// <param name="ingredient">The ingredient to represent.</param>
        public static string GetRepresentativeItemId(BundleIngredientDescription ingredient)
        {
            if (!ingredient.category.HasValue)
            {
                return ingredient.id;
            }
            foreach (var parsedItemData in ItemRegistry.GetObjectTypeDefinition().GetAllData())
            {
                var category1 = parsedItemData.Category;
                var category2 = ingredient.category;
                var valueOrDefault = category2.GetValueOrDefault();
                if (category1 == valueOrDefault & category2.HasValue)
                {
                    return parsedItemData.QualifiedItemId;
                }
            }
            return "0";
        }

        public static void GetBundleRewards(int area, List<Item> rewards)
        {
            var communityCenter = Game1.RequireLocation<CommunityCenter>("CommunityCenter");
            var bundleData = Game1.netWorldState.Value.BundleData;
            foreach (var key in bundleData.Keys)
            {
                if (key.Contains(CommunityCenter.getAreaNameFromNumber(area)))
                {
                    var int32 = Convert.ToInt32(key.Split('/')[1]);
                    if (communityCenter.bundleRewards[int32])
                    {
                        var standardTextDescription = Utility.getItemFromStandardTextDescription(bundleData[key].Split('/')[1], Game1.player);
                        standardTextDescription.SpecialVariable = int32;
                        rewards.Add(standardTextDescription);
                    }
                }
            }
        }

        private void OpenRewardsMenu()
        {
            Game1.playSound("smallSelect");
            var objList = new List<Item>();
            GetBundleRewards(WhichArea, objList);
            Game1.activeClickableMenu = new ItemGrabMenu(objList, false, true, null, null, null, RewardGrabbed, canBeExitedWithKey: true, context: this);
            Game1.activeClickableMenu.exitFunction = exitFunction != null ? exitFunction : ReOpenThisMenu;
        }

        private void RewardGrabbed(Item item, Farmer who)
        {
            Game1.RequireLocation<CommunityCenter>("CommunityCenter").bundleRewards[item.SpecialVariable] = false;
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

        protected void restoreaAreaOnExit_AbandonedJojaMart()
        {
            Game1.RequireLocation<AbandonedJojaMart>("AbandonedJojaMart").restoreAreaCutscene();
        }

        protected void restoreAreaOnExit()
        {
            if (FromGameMenu)
            {
                return;
            }
            Game1.RequireLocation<CommunityCenter>("CommunityCenter").restoreAreaCutscene(WhichArea);
        }

        public virtual void CheckForRewards()
        {
            var bundleData = Game1.netWorldState.Value.BundleData;
            foreach (var key in bundleData.Keys)
            {
                if (key.Contains(CommunityCenter.getAreaNameFromNumber(WhichArea)) && bundleData[key].Split('/')[1].Length > 1)
                {
                    var int32 = Convert.ToInt32(key.Split('/')[1]);
                    if (Game1.RequireLocation<CommunityCenter>("CommunityCenter").bundleRewards[int32])
                    {
                        PresentButton = new ClickableAnimatedComponent(new Rectangle(xPositionOnScreen + 592, yPositionOnScreen + 512, 72, 72), "", Game1.content.LoadString("Strings\\StringsFromCSFiles:JunimoNoteMenu.cs.10783"), new TemporaryAnimatedSprite(NOTE_TEXTURE_NAME, new Rectangle(548, 262, 18, 20), 70f, 4, 99999, new Vector2(-64f, -64f), false, false, 0.5f, 0.0f, Color.White, 4f, 0.0f, 0.0f, 0.0f, true));
                        break;
                    }
                }
            }
        }

        /// <inheritdoc />
        public override void receiveRightClick(int x, int y, bool playSound = true)
        {
            if (!CanClick)
            {
                return;
            }
            if (SpecificBundlePage)
            {
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
            if (SpecificBundlePage || !IsReadyToCloseMenuOrBundle())
            {
                return;
            }
            exitThisMenu(GameMenuTabToReturnTo == -1);
        }

        /// <inheritdoc />
        public override void update(GameTime time)
        {
            if (SpecificBundlePage && CurrentPageBundle != null && CurrentPageBundle.CompletionTimer <= 0 && IsReadyToCloseMenuOrBundle() && CurrentPageBundle.Complete)
            {
                TakeDownBundleSpecificPage();
            }
            foreach (var bundle in Bundles)
            {
                bundle.Update(time);
            }
            TempSprites.RemoveWhere(sprite => sprite.update(time));
            PresentButton?.update(time);
            if (ScreenSwipe != null)
            {
                CanClick = false;
                if (ScreenSwipe.update(time))
                {
                    ScreenSwipe = null;
                    CanClick = true;
                    var screenSwipeFinished = OnScreenSwipeFinished;
                    if (screenSwipeFinished != null)
                    {
                        screenSwipeFinished(this);
                    }
                }
            }
            if (!BundlesChanged || !FromGameMenu)
            {
                return;
            }
            ReOpenThisMenu();
        }

        /// <inheritdoc />
        public override void performHoverAction(int x, int y)
        {
            base.performHoverAction(x, y);
            if (ScrambledText)
            {
                return;
            }
            HoverText = "";
            if (SpecificBundlePage)
            {
                PerformHoverActionSpecificBundlePage(x, y);
            }
            else
            {
                PerformHoverActionBundleRoomPage(x, y);
            }
        }

        private void PerformHoverActionSpecificBundlePage(int x, int y)
        {
            BackButton?.tryHover(x, y);
            HoveredItem = CurrentPageBundle.Complete || CurrentPageBundle.CompletionTimer > 0 ? null : Inventory.hover(x, y, HeldItem);
            PerformHoverIngredients(x, y);
            PerformHoverIngredientSlots(x, y);
            TryHoverButtons(x, y);
        }

        private void PerformHoverIngredients(int x, int y)
        {
            foreach (var ingredient in IngredientList)
            {
                if (ingredient.bounds.Contains(x, y))
                {
                    HoverText = ingredient.hoverText;
                    break;
                }
            }
        }

        private void PerformHoverIngredientSlots(int x, int y)
        {
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

        protected virtual void TryHoverButtons(int x, int y)
        {
            PurchaseButton?.tryHover(x, y);
        }

        private void PerformHoverActionBundleRoomPage(int x, int y)
        {
            if (PresentButton != null)
            {
                HoverText = PresentButton.tryHover(x, y);
            }
            foreach (var bundle in Bundles)
            {
                bundle.TryHoverAction(x, y);
            }
            if (!FromGameMenu)
            {
                return;
            }
            AreaNextButton.tryHover(x, y);
            AreaBackButton.tryHover(x, y);
        }

        /// <inheritdoc />
        public override void draw(SpriteBatch b)
        {
            if (Game1.options.showMenuBackground)
            {
                drawBackground(b);
            }
            else if (!Game1.options.showClearBackgrounds)
            {
                b.Draw(Game1.fadeToBlackRect, new Rectangle(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height), Color.Black * 0.5f);
            }
            if (!SpecificBundlePage)
            {
                b.Draw(NoteTexture, new Vector2(xPositionOnScreen, yPositionOnScreen), new Rectangle(0, 0, BASE_WIDTH, BASE_HEIGHT), Color.White, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, 0.1f);
                SpriteText.drawStringHorizontallyCenteredAt(b, ScrambledText ? CommunityCenter.getAreaEnglishDisplayNameFromNumber(WhichArea) : CommunityCenter.getAreaDisplayNameFromNumber(WhichArea), xPositionOnScreen + width / 2 + 16, yPositionOnScreen + 12, height: 99999, alpha: 0.88f, junimoText: ScrambledText);
                if (ScrambledText)
                {
                    SpriteText.drawString(b, LocalizedContentManager.CurrentLanguageLatin ? Game1.content.LoadString("Strings\\StringsFromCSFiles:JunimoNoteMenu.cs.10786") : Game1.content.LoadBaseString("Strings\\StringsFromCSFiles:JunimoNoteMenu.cs.10786"), xPositionOnScreen + 96, yPositionOnScreen + 96, width: width - 192, height: 99999, alpha: 0.88f, junimoText: true);
                    base.draw(b);
                    if (Game1.options.SnappyMenus || !CanClick)
                    {
                        return;
                    }
                    drawMouse(b);
                    return;
                }
                foreach (var bundle in Bundles)
                {
                    bundle.Draw(b);
                }
                PresentButton?.draw(b);
                foreach (var tempSprite in TempSprites)
                {
                    tempSprite.draw(b, true);
                }
                if (FromGameMenu)
                {
                    if (AreaNextButton.visible)
                    {
                        AreaNextButton.draw(b);
                    }
                    if (AreaBackButton.visible)
                    {
                        AreaBackButton.draw(b);
                    }
                }
            }
            else
            {
                DrawSpecificBundle(b);
            }
            if (GetRewardNameForArea(WhichArea) != "")
            {
                SpriteText.drawStringWithScrollCenteredAt(b, GetRewardNameForArea(WhichArea), xPositionOnScreen + width / 2, Math.Min(yPositionOnScreen + height + 20, Game1.uiViewport.Height - 64 - 8));
            }
            base.draw(b);
            Game1.mouseCursorTransparency = 1f;
            if (CanClick)
            {
                drawMouse(b);
            }
            HeldItem?.drawInMenu(b, new Vector2(Game1.getOldMouseX() + 16, Game1.getOldMouseY() + 16), 1f);
            if (Inventory.descriptionText.Length > 0)
            {
                if (HoveredItem != null)
                {
                    drawToolTip(b, HoveredItem.getDescription(), HoveredItem.DisplayName, HoveredItem);
                }
            }
            else
            {
                drawHoverText(b, _singleBundleMenu || Game1.player.hasOrWillReceiveMail("canReadJunimoText") || HoverText.Length <= 0 ? HoverText : "???", Game1.dialogueFont);
            }
            ScreenSwipe?.draw(b);
        }

        private void DrawSpecificBundle(SpriteBatch b)
        {
            b.Draw(NoteTexture, new Vector2(xPositionOnScreen, yPositionOnScreen), new Rectangle(BASE_WIDTH, 0, BASE_WIDTH, BASE_HEIGHT), Color.White, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, 0.1f);
            
            DrawCurrentPageBundle(b);
            DrawButtons(b);
            DrawTemporarySprites(b);
            DrawIngredientSlots(b);
            DrawIngredients(b);
            Inventory.draw(b);
        }

        protected virtual void DrawButtons(SpriteBatch b)
        {
            DrawBackButton(b);
            DrawPurchaseButton(b);
        }

        private void DrawBackButton(SpriteBatch b)
        {
            if (BackButton != null)
            {
                BackButton.draw(b);
            }
        }

        private void DrawPurchaseButton(SpriteBatch b)
        {
            if (PurchaseButton != null)
            {
                PurchaseButton.draw(b);
                Game1.dayTimeMoneyBox.drawMoneyBox(b);
            }
        }

        private void DrawTemporarySprites(SpriteBatch b)
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

        private void DrawCurrentPageBundle(SpriteBatch b)
        {
            if (CurrentPageBundle == null)
            {
                return;
            }
            var num1 = CurrentPageBundle.BundleIndex;
            var texture = NoteTexture;
            var num2 = BASE_HEIGHT;
            if (CurrentPageBundle.BundleTextureIndexOverride >= 0)
            {
                num1 = CurrentPageBundle.BundleTextureIndexOverride;
            }
            if (CurrentPageBundle.BundleTextureOverride != null)
            {
                texture = CurrentPageBundle.BundleTextureOverride;
                num2 = 0;
            }
            b.Draw(texture, new Vector2(xPositionOnScreen + 872, yPositionOnScreen + 88), new Rectangle(num1 * 16 * 2 % texture.Width, num2 + 32 * (num1 * 16 * 2 / texture.Width), 32, 32), Color.White, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, 0.15f);
            if (CurrentPageBundle.label != null)
            {
                var x = Game1.dialogueFont.MeasureString(!Game1.player.hasOrWillReceiveMail("canReadJunimoText") ? "???" : Game1.content.LoadString("Strings\\UI:JunimoNote_BundleName", CurrentPageBundle.label)).X;
                b.Draw(NoteTexture, new Vector2(xPositionOnScreen + 936 - (int)x / 2 - 16, yPositionOnScreen + 228), new Rectangle(517, 266, 4, 17), Color.White, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, 0.1f);
                b.Draw(NoteTexture, new Rectangle(xPositionOnScreen + 936 - (int)x / 2, yPositionOnScreen + 228, (int)x, 68), new Rectangle(520, 266, 1, 17), Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, 0.1f);
                b.Draw(NoteTexture, new Vector2(xPositionOnScreen + 936 + (int)x / 2, yPositionOnScreen + 228), new Rectangle(524, 266, 4, 17), Color.White, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, 0.1f);
                b.DrawString(Game1.dialogueFont, !Game1.player.hasOrWillReceiveMail("canReadJunimoText") ? "???" : Game1.content.LoadString("Strings\\UI:JunimoNote_BundleName", CurrentPageBundle.label), new Vector2(xPositionOnScreen + 936 - x / 2f, yPositionOnScreen + 236) + new Vector2(2f, 2f), Game1.textShadowColor);
                b.DrawString(Game1.dialogueFont, !Game1.player.hasOrWillReceiveMail("canReadJunimoText") ? "???" : Game1.content.LoadString("Strings\\UI:JunimoNote_BundleName", CurrentPageBundle.label), new Vector2(xPositionOnScreen + 936 - x / 2f, yPositionOnScreen + 236) + new Vector2(0.0f, 2f), Game1.textShadowColor);
                b.DrawString(Game1.dialogueFont, !Game1.player.hasOrWillReceiveMail("canReadJunimoText") ? "???" : Game1.content.LoadString("Strings\\UI:JunimoNote_BundleName", CurrentPageBundle.label), new Vector2(xPositionOnScreen + 936 - x / 2f, yPositionOnScreen + 236) + new Vector2(2f, 0.0f), Game1.textShadowColor);
                b.DrawString(Game1.dialogueFont, !Game1.player.hasOrWillReceiveMail("canReadJunimoText") ? "???" : Game1.content.LoadString("Strings\\UI:JunimoNote_BundleName", CurrentPageBundle.label), new Vector2(xPositionOnScreen + 936 - x / 2f, yPositionOnScreen + 236), Game1.textColor * 0.9f);
            }
        }

        public virtual string GetRewardNameForArea(int whichArea)
        {
            switch (whichArea)
            {
                case -1:
                    return "";
                case 0:
                    return Game1.content.LoadString("Strings\\UI:JunimoNote_RewardPantry");
                case 1:
                    return Game1.content.LoadString("Strings\\UI:JunimoNote_RewardCrafts");
                case 2:
                    return Game1.content.LoadString("Strings\\UI:JunimoNote_RewardFishTank");
                case 3:
                    return Game1.content.LoadString("Strings\\UI:JunimoNote_RewardBoiler");
                case 4:
                    return Game1.content.LoadString("Strings\\UI:JunimoNote_RewardVault");
                case 5:
                    return Game1.content.LoadString("Strings\\UI:JunimoNote_RewardBulletin");
                default:
                    return "???";
            }
        }

        /// <inheritdoc />
        public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
        {
            base.gameWindowSizeChanged(oldBounds, newBounds);
            TempSprites.Clear();
            xPositionOnScreen = Game1.uiViewport.Width / 2 - 640;
            yPositionOnScreen = Game1.uiViewport.Height / 2 - 360;
            BackButton = new ClickableTextureComponent("Back", new Rectangle(xPositionOnScreen + borderWidth * 2 + 8, yPositionOnScreen + borderWidth * 2 + 4, 64, 64), null, null, Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44), 1f);
            if (FromGameMenu)
            {
                var textureComponent1 = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + width - 128, yPositionOnScreen, 48, 44), Game1.mouseCursors, new Rectangle(365, 495, 12, 11), 4f);
                textureComponent1.visible = false;
                AreaNextButton = textureComponent1;
                var textureComponent2 = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + 64, yPositionOnScreen, 48, 44), Game1.mouseCursors, new Rectangle(352, 495, 12, 11), 4f);
                textureComponent2.visible = false;
                AreaBackButton = textureComponent2;
            }
            Inventory = new InventoryMenu(xPositionOnScreen + 128, yPositionOnScreen + 140, true, highlightMethod: HighlightObjects, capacity: Game1.player.maxItems.Value, rows: 6, horizontalGap: 8, verticalGap: 8, drawSlots: false);
            for (var index = 0; index < Inventory.inventory.Count; ++index)
            {
                if (index >= Inventory.actualInventory.Count)
                {
                    Inventory.inventory[index].visible = false;
                }
            }
            for (var index = 0; index < Bundles.Count; ++index)
            {
                var locationFromNumber = GetBundleLocationFromNumber(index);
                Bundles[index].bounds.X = locationFromNumber.X;
                Bundles[index].bounds.Y = locationFromNumber.Y;
                Bundles[index].Sprite.position = new Vector2(locationFromNumber.X, locationFromNumber.Y);
            }
            if (!SpecificBundlePage)
            {
                return;
            }
            var ofIngredientSlots = CurrentPageBundle.NumberOfIngredientSlots;
            var toAddTo1 = new List<Rectangle>();
            AddRectangleRowsToList(toAddTo1, ofIngredientSlots, 932, 540);
            IngredientSlots.Clear();
            for (var index = 0; index < toAddTo1.Count; ++index)
                IngredientSlots.Add(new ClickableTextureComponent(toAddTo1[index], NoteTexture, new Rectangle(512, 244, 18, 18), 4f));
            var toAddTo2 = new List<Rectangle>();
            IngredientList.Clear();
            AddRectangleRowsToList(toAddTo2, CurrentPageBundle.Ingredients.Count, 932, 364);
            for (var index = 0; index < toAddTo2.Count; ++index)
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
                    var textureComponent = new ClickableTextureComponent("", toAddTo2[index], "", obj.DisplayName, texture, sourceRect, 4f);
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

        private void SetUpBundleSpecificPage(ArchipelagoBundle b)
        {
            TempSprites.Clear();
            CurrentPageBundle = b;
            SpecificBundlePage = true;
            if (IsBundleCurrencyBased())
            {
                SetUpCurrencyButtons();
            }
            else
            {
                SetUpIngredientButtons(b);
            }
        }

        protected virtual void SetUpCurrencyButtons()
        {
            SetUpPurchaseButton();
        }

        protected virtual void SetUpPurchaseButton()
        {
            if (FromGameMenu)
            {
                return;
            }
            var textureComponent = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + 800, yPositionOnScreen + 504, 260, 72), NoteTexture, new Rectangle(517, 286, 65, 20), 4f);
            textureComponent.myID = 797;
            textureComponent.leftNeighborID = REGION_BACK_BUTTON;
            PurchaseButton = textureComponent;
            if (!Game1.options.SnappyMenus)
            {
                return;
            }
            currentlySnappedComponent = PurchaseButton;
            snapCursorToCurrentSnappedComponent();
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

        public bool IsBundleCurrencyBased()
        {
            var ingredient = CurrentPageBundle.Ingredients.Last();
            var ingredientId = ingredient.id;
            return CurrencyBundle.CurrencyIds.ContainsValue(ingredientId);
        }

        public override bool IsAutomaticSnapValid(
          int direction,
          ClickableComponent a,
          ClickableComponent b)
        {
            return (CurrentPartialIngredientDescriptionIndex < 0 || (!IngredientSlots.Contains(b) || b.item == PartialDonationItem) && (!IngredientList.Contains(b) || IngredientList.IndexOf(b as ClickableTextureComponent) == CurrentPartialIngredientDescriptionIndex)) && (a.myID >= REGION_BUNDLE_MODIFIER || a.myID == REGION_AREA_NEXT_BUTTON ? 1 : a.myID == REGION_AREA_BACK_BUTTON ? 1 : 0) == (b.myID >= REGION_BUNDLE_MODIFIER || b.myID == REGION_AREA_NEXT_BUTTON ? 1 : b.myID == REGION_AREA_BACK_BUTTON ? 1 : 0);
        }

        private void AddRectangleRowsToList(
          List<Rectangle> toAddTo,
          int numberOfItems,
          int centerX,
          int centerY)
        {
            switch (numberOfItems)
            {
                case 1:
                    toAddTo.AddRange(CreateRowOfBoxesCenteredAt(xPositionOnScreen + centerX, yPositionOnScreen + centerY, 1, 72, 72, 12));
                    break;
                case 2:
                    toAddTo.AddRange(CreateRowOfBoxesCenteredAt(xPositionOnScreen + centerX, yPositionOnScreen + centerY, 2, 72, 72, 12));
                    break;
                case 3:
                    toAddTo.AddRange(CreateRowOfBoxesCenteredAt(xPositionOnScreen + centerX, yPositionOnScreen + centerY, 3, 72, 72, 12));
                    break;
                case 4:
                    toAddTo.AddRange(CreateRowOfBoxesCenteredAt(xPositionOnScreen + centerX, yPositionOnScreen + centerY, 4, 72, 72, 12));
                    break;
                case 5:
                    toAddTo.AddRange(CreateRowOfBoxesCenteredAt(xPositionOnScreen + centerX, yPositionOnScreen + centerY - 36, 3, 72, 72, 12));
                    toAddTo.AddRange(CreateRowOfBoxesCenteredAt(xPositionOnScreen + centerX, yPositionOnScreen + centerY + 40, 2, 72, 72, 12));
                    break;
                case 6:
                    toAddTo.AddRange(CreateRowOfBoxesCenteredAt(xPositionOnScreen + centerX, yPositionOnScreen + centerY - 36, 3, 72, 72, 12));
                    toAddTo.AddRange(CreateRowOfBoxesCenteredAt(xPositionOnScreen + centerX, yPositionOnScreen + centerY + 40, 3, 72, 72, 12));
                    break;
                case 7:
                    toAddTo.AddRange(CreateRowOfBoxesCenteredAt(xPositionOnScreen + centerX, yPositionOnScreen + centerY - 36, 4, 72, 72, 12));
                    toAddTo.AddRange(CreateRowOfBoxesCenteredAt(xPositionOnScreen + centerX, yPositionOnScreen + centerY + 40, 3, 72, 72, 12));
                    break;
                case 8:
                    toAddTo.AddRange(CreateRowOfBoxesCenteredAt(xPositionOnScreen + centerX, yPositionOnScreen + centerY - 36, 4, 72, 72, 12));
                    toAddTo.AddRange(CreateRowOfBoxesCenteredAt(xPositionOnScreen + centerX, yPositionOnScreen + centerY + 40, 4, 72, 72, 12));
                    break;
                case 9:
                    toAddTo.AddRange(CreateRowOfBoxesCenteredAt(xPositionOnScreen + centerX, yPositionOnScreen + centerY - 36, 5, 72, 72, 12));
                    toAddTo.AddRange(CreateRowOfBoxesCenteredAt(xPositionOnScreen + centerX, yPositionOnScreen + centerY + 40, 4, 72, 72, 12));
                    break;
                case 10:
                    toAddTo.AddRange(CreateRowOfBoxesCenteredAt(xPositionOnScreen + centerX, yPositionOnScreen + centerY - 36, 5, 72, 72, 12));
                    toAddTo.AddRange(CreateRowOfBoxesCenteredAt(xPositionOnScreen + centerX, yPositionOnScreen + centerY + 40, 5, 72, 72, 12));
                    break;
                case 11:
                    toAddTo.AddRange(CreateRowOfBoxesCenteredAt(xPositionOnScreen + centerX, yPositionOnScreen + centerY - 36, 6, 72, 72, 12));
                    toAddTo.AddRange(CreateRowOfBoxesCenteredAt(xPositionOnScreen + centerX, yPositionOnScreen + centerY + 40, 5, 72, 72, 12));
                    break;
                case 12:
                    toAddTo.AddRange(CreateRowOfBoxesCenteredAt(xPositionOnScreen + centerX, yPositionOnScreen + centerY - 36, 6, 72, 72, 12));
                    toAddTo.AddRange(CreateRowOfBoxesCenteredAt(xPositionOnScreen + centerX, yPositionOnScreen + centerY + 40, 6, 72, 72, 12));
                    break;
            }
        }

        private List<Rectangle> CreateRowOfBoxesCenteredAt(
          int xStart,
          int yStart,
          int numBoxes,
          int boxWidth,
          int boxHeight,
          int horizontalGap)
        {
            var ofBoxesCenteredAt = new List<Rectangle>();
            var num = xStart - numBoxes * (boxWidth + horizontalGap) / 2;
            var y = yStart - boxHeight / 2;
            for (var index = 0; index < numBoxes; ++index)
                ofBoxesCenteredAt.Add(new Rectangle(num + index * (boxWidth + horizontalGap), y, boxWidth, boxHeight));
            return ofBoxesCenteredAt;
        }

        public virtual void TakeDownBundleSpecificPage()
        {
            if (!IsReadyToCloseMenuOrBundle())
            {
                return;
            }
            ReturnPartialDonations(false);
            HoveredItem = null;
            if (!SpecificBundlePage)
            {
                return;
            }
            TakeDownSpecificBundleComponents();
        }

        protected virtual void TakeDownSpecificBundleComponents()
        {
            SpecificBundlePage = false;
            IngredientSlots.Clear();
            IngredientList.Clear();
            TempSprites.Clear();
            PurchaseButton = null;
            if (!Game1.options.SnappyMenus)
            {
                return;
            }
            if (CurrentPageBundle != null)
            {
                currentlySnappedComponent = CurrentPageBundle;
                snapCursorToCurrentSnappedComponent();
            }
            else
            {
                snapToDefaultClickableComponent();
            }
        }

        protected Point GetBundleLocationFromNumber(int whichBundle)
        {
            var locationFromNumber = new Point(xPositionOnScreen, yPositionOnScreen);
            switch (whichBundle)
            {
                case 0:
                    locationFromNumber.X += 592;
                    locationFromNumber.Y += 136;
                    break;
                case 1:
                    locationFromNumber.X += 392;
                    locationFromNumber.Y += 384;
                    break;
                case 2:
                    locationFromNumber.X += 784;
                    locationFromNumber.Y += 388;
                    break;
                case 3:
                    locationFromNumber.X += 304;
                    locationFromNumber.Y += 252;
                    break;
                case 4:
                    locationFromNumber.X += 892;
                    locationFromNumber.Y += 252;
                    break;
                case 5:
                    locationFromNumber.X += 588;
                    locationFromNumber.Y += 276;
                    break;
                case 6:
                    locationFromNumber.X += 588;
                    locationFromNumber.Y += 380;
                    break;
                case 7:
                    locationFromNumber.X += 440;
                    locationFromNumber.Y += 164;
                    break;
                case 8:
                    locationFromNumber.X += 776;
                    locationFromNumber.Y += 164;
                    break;
            }
            return locationFromNumber;
        }
    }
}

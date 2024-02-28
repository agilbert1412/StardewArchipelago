using System;
using System.Collections.Generic;
using System.Linq;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Items.Unlocks;
using StardewArchipelago.Locations.CodeInjections.Vanilla.MonsterSlayer;
using StardewArchipelago.Stardew;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Objects;
using StardewValley.Tools;
using Object = StardewValley.Object;

namespace StardewArchipelago.Locations.CodeInjections.Vanilla
{
    public static class AdventurerGuildInjections
    {
        private static IMonitor _monitor;
        private static IModHelper _modHelper;
        private static ArchipelagoClient _archipelago;
        private static LocationChecker _locationChecker;
        private static WeaponsManager _weaponsManager;

        public static void Initialize(IMonitor monitor, IModHelper modHelper, ArchipelagoClient archipelago, LocationChecker locationChecker, WeaponsManager weaponsManager)
        {
            _monitor = monitor;
            _modHelper = modHelper;
            _archipelago = archipelago;
            _locationChecker = locationChecker;
            _weaponsManager = weaponsManager;
        }

        // public static bool TryOpenShopMenu(string shopId, string ownerName, bool playOpenSound = true)
        public static bool TryOpenShopMenu_AddReceivedEquipmentsToGuildRecovery_Prefix(string shopId, string ownerName, bool playOpenSound, bool __result)
        {
            try
            {
                if (shopId != "AdventureGuildRecovery" || ownerName != "Marlon")
                {
                    return true; // run original logic
                }

                var farmer = Game1.player;

                var equipmentsToRecover = new[]
                {
                    VanillaUnlockManager.PROGRESSIVE_WEAPON, VanillaUnlockManager.PROGRESSIVE_SWORD,
                    VanillaUnlockManager.PROGRESSIVE_CLUB, VanillaUnlockManager.PROGRESSIVE_DAGGER,
                    VanillaUnlockManager.PROGRESSIVE_BOOTS,
                };

                var seed = (int)(Game1.uniqueIDForThisGame + Game1.stats.DaysPlayed);
                var random = new Random(seed);
                foreach (var equipmentApItem in equipmentsToRecover)
                {
                    var received = Math.Min(6, _archipelago.GetReceivedItemCount(equipmentApItem));
                    if (received <= 0)
                    {
                        continue;
                    }

                    var candidates = GetRecoveryCandidates(equipmentApItem, received);

                    if (!candidates.Any())
                    {
                        continue;
                    }

                    var chosenIndex = random.Next(0, candidates.Count);
                    var chosenEquipment = candidates[chosenIndex];

                    var equipmentToRecover = chosenEquipment.PrepareForRecovery();

                    if (farmer.itemsLostLastDeath.Any(x => x.Name == equipmentToRecover.Name))
                    {
                        continue;
                    }

                    equipmentToRecover.isLostItem = true;
                    farmer.itemsLostLastDeath.Add(equipmentToRecover);
                }

                return true; // run original logic
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(TryOpenShopMenu_AddReceivedEquipmentsToGuildRecovery_Prefix)}:\n{ex}",
                    LogLevel.Error);
                return true; // run original logic
            }
        }

        private static List<StardewItem> GetRecoveryCandidates(string equipmentApItem, int tier)
        {
            var candidates = equipmentApItem switch
            {
                VanillaUnlockManager.PROGRESSIVE_WEAPON => GetRecoveryCandidates(_weaponsManager.WeaponsByTier, tier),
                VanillaUnlockManager.PROGRESSIVE_SWORD => GetRecoveryCandidates(_weaponsManager.WeaponsByCategoryByTier[WeaponsManager.TYPE_SWORD], tier),
                VanillaUnlockManager.PROGRESSIVE_CLUB => GetRecoveryCandidates(_weaponsManager.WeaponsByCategoryByTier[WeaponsManager.TYPE_CLUB], tier),
                VanillaUnlockManager.PROGRESSIVE_DAGGER => GetRecoveryCandidates(_weaponsManager.WeaponsByCategoryByTier[WeaponsManager.TYPE_DAGGER], tier),
                VanillaUnlockManager.PROGRESSIVE_BOOTS => GetRecoveryCandidates(_weaponsManager.BootsByTier, tier),
                _ => new List<StardewItem>(),
            };
            return candidates;
        }

        private static List<StardewItem> GetRecoveryCandidates(Dictionary<int, List<StardewItem>> itemsByTier, int tier)
        {
            if (tier <= 0)
            {
                return new List<StardewItem>();
            }

            if (!itemsByTier.ContainsKey(tier) || !itemsByTier[tier].Any())
            {
                return GetRecoveryCandidates(itemsByTier, tier - 1);
            }

            return itemsByTier[tier];
        }

        public static void RemoveExtraItemsFromItemsLostLastDeath()
        {
            foreach (var lostItem in Game1.player.itemsLostLastDeath.ToArray())
            {
                if (lostItem is MeleeWeaponToRecover || lostItem is BootsToRecover)
                {
                    Game1.player.itemsLostLastDeath.Remove(lostItem);
                }
            }
        }

        public static bool GetAdventureShopStock_ShopBasedOnReceivedItems_Prefix(ref Dictionary<ISalable, int[]> __result)
        {
            try
            {
                var adventureShopStock = new Dictionary<ISalable, int[]>();
                AddWeapons(adventureShopStock);
                AddShoes(adventureShopStock);
                AddRings(adventureShopStock);
                AddSlingshots(adventureShopStock);
                AddAmmo(adventureShopStock);
                AddGilRewards(adventureShopStock);

                __result = adventureShopStock;
                return false; // don't run original logic
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(GetAdventureShopStock_ShopBasedOnReceivedItems_Prefix)}:\n{ex}",
                    LogLevel.Error);
                return true; // run original logic
            }
        }

        private static void AddWeapons(Dictionary<ISalable, int[]> adventureShopStock)
        {
            var weapons = new[]
            {
                WoodenBlade, IronDirk, WindSpire, Femur, SteelSmallsword, WoodClub, ElfBlade, SilverSaber, PirateSword,
                CrystalDagger, Cutlass, IronEdge, BurglarsShank, WoodMallet, Claymore, TemplarsBlade, Kudgel,
                ShadowDagger, ObsidianEdge, TemperedBroadsword, WickedKris, BoneSword, OssifiedBlade, SteelFalchion,
                TheSlammer, LavaKatana, // No Galaxy Weapons here
            };
            AddItemsToShop(adventureShopStock, weapons);
            AddGalaxyWeaponsToShopIfReceivedAny(adventureShopStock);
        }

        private static void AddShoes(Dictionary<ISalable, int[]> adventureShopStock)
        {
            var shoes = new[]
            {
                Sneakers, LeatherBoots, WorkBoots, TundraBoots, ThermalBoots, CombatBoots, FirewalkerBoots, DarkBoots,
                SpaceBoots, CrystalShoes,
            };
            AddItemsToShop(adventureShopStock, shoes);
        }

        private static void AddRings(Dictionary<ISalable, int[]> adventureShopStock)
        {
            adventureShopStock.Add(AmethystRing.Key, AmethystRing.Value);
            adventureShopStock.Add(TopazRing.Key, TopazRing.Value);
            if (MineShaft.lowestLevelReached >= 40)
            {
                adventureShopStock.Add(AquamarineRing.Key, AquamarineRing.Value);
                adventureShopStock.Add(JadeRing.Key, JadeRing.Value);
            }

            if (MineShaft.lowestLevelReached >= 80)
            {
                adventureShopStock.Add(EmeraldRing.Key, EmeraldRing.Value);
                adventureShopStock.Add(RubyRing.Key, RubyRing.Value);
            }
        }

        private static void AddSlingshots(Dictionary<ISalable, int[]> adventureShopStock)
        {
            var slingshots = new[] { Slingshot, MasterSlingshot };
            AddItemsToShop(adventureShopStock, slingshots);
        }

        private static void AddAmmo(Dictionary<ISalable, int[]> adventureShopStock)
        {
            if (Game1.player.craftingRecipes.ContainsKey("Explosive Ammo"))
            {
                adventureShopStock.Add(ExplosiveAmmo.Key, ExplosiveAmmo.Value);
            }
        }

        private static void AddGilRewards(Dictionary<ISalable, int[]> adventureShopStock)
        {
            if (Game1.player.mailReceived.Contains("Gil_Slime Charmer Ring"))
            {
                adventureShopStock.Add(SlimeCharmerRing.Key, SlimeCharmerRing.Value);
            }

            if (Game1.player.mailReceived.Contains("Gil_Savage Ring"))
            {
                adventureShopStock.Add(SavageRing.Key, SavageRing.Value);
            }

            if (Game1.player.mailReceived.Contains("Gil_Burglar's Ring"))
            {
                adventureShopStock.Add(BurglarsRing.Key, BurglarsRing.Value);
            }

            if (Game1.player.mailReceived.Contains("Gil_Vampire Ring"))
            {
                adventureShopStock.Add(VampireRing.Key, VampireRing.Value);
            }

            if (Game1.player.mailReceived.Contains("Gil_Crabshell Ring"))
            {
                adventureShopStock.Add(CrabshellRing.Key, CrabshellRing.Value);
            }

            if (Game1.player.mailReceived.Contains("Gil_Napalm Ring"))
            {
                adventureShopStock.Add(NapalmRing.Key, NapalmRing.Value);
            }

            if (Game1.player.mailReceived.Contains("Gil_Skeleton Mask"))
            {
                adventureShopStock.Add(SkeletonMask.Key, SkeletonMask.Value);
            }

            if (Game1.player.mailReceived.Contains("Gil_Hard Hat"))
            {
                adventureShopStock.Add(HardHat.Key, HardHat.Value);
            }

            if (Game1.player.mailReceived.Contains("Gil_Arcane Hat"))
            {
                adventureShopStock.Add(ArcaneHat.Key, ArcaneHat.Value);
            }

            if (Game1.player.mailReceived.Contains("Gil_Knight's Helmet"))
            {
                adventureShopStock.Add(KnightsHelmet.Key, KnightsHelmet.Value);
            }

            if (Game1.player.mailReceived.Contains("Gil_Insect Head"))
            {
                adventureShopStock.Add(InsectHead.Key, InsectHead.Value);
            }
        }

        private static void AddItemsToShop(Dictionary<ISalable, int[]> adventureShopStock, KeyValuePair<ISalable, int[]>[] items)
        {
            foreach (var (item, price) in items)
            {
                AddItemToStockIfReceived(adventureShopStock, item, price);
            }
        }

        private static void AddItemToStockIfReceived(Dictionary<ISalable, int[]> adventureShopStock, ISalable item, int[] price)
        {
            if (!_archipelago.HasReceivedItem(item.Name))
            {
                return;
            }

            adventureShopStock.Add(item, price);
        }

        private static KeyValuePair<ISalable, int[]> CreateWithPrice(ISalable item, int price)
        {
            return new(item, new[] { price, int.MaxValue });
        }

        private static KeyValuePair<ISalable, int[]> CreateWeapon(int weaponId, int price)
        {
            return CreateWithPrice(new MeleeWeapon(weaponId.ToString()), price);
        }

        private static KeyValuePair<ISalable, int[]> CreateBoots(int bootsId, int price)
        {
            return CreateWithPrice(new Boots(bootsId.ToString()), price);
        }

        private static KeyValuePair<ISalable, int[]> CreateRing(int ringId, int price)
        {
            return CreateWithPrice(new Ring(ringId.ToString()), price);
        }

        private static KeyValuePair<ISalable, int[]> CreateSlingshot(int slingshotId, int price)
        {
            return CreateWithPrice(new Slingshot(slingshotId.ToString()), price);
        }

        private static KeyValuePair<ISalable, int[]> CreateHat(int hatId, int price)
        {
            return CreateWithPrice(new Hat(hatId.ToString()), price);
        }

        private static void AddGalaxyWeaponsToShopIfReceivedAny(Dictionary<ISalable, int[]> adventureShopStock)
        {
            var galaxyWeapons = new[] { GalaxySword, GalaxyDagger, GalaxyHammer };
            if (!galaxyWeapons.Any(weapon => _archipelago.HasReceivedItem(weapon.Key.Name)))
            {
                return;
            }

            foreach (var (item, price) in galaxyWeapons)
            {
                adventureShopStock.Add(item, price);
            }
        }

        private static readonly KeyValuePair<ISalable, int[]> WoodenBlade = CreateWeapon(12, 250);
        private static readonly KeyValuePair<ISalable, int[]> IronDirk = CreateWeapon(17, 500);
        private static readonly KeyValuePair<ISalable, int[]> WindSpire = CreateWeapon(22, 500);
        private static readonly KeyValuePair<ISalable, int[]> Femur = CreateWeapon(31, 500);
        private static readonly KeyValuePair<ISalable, int[]> SteelSmallsword = CreateWeapon(11, 600);
        private static readonly KeyValuePair<ISalable, int[]> WoodClub = CreateWeapon(24, 600);
        private static readonly KeyValuePair<ISalable, int[]> ElfBlade = CreateWeapon(20, 600);
        private static readonly KeyValuePair<ISalable, int[]> SilverSaber = CreateWeapon(1, 750);
        private static readonly KeyValuePair<ISalable, int[]> PirateSword = CreateWeapon(43, 850);
        private static readonly KeyValuePair<ISalable, int[]> CrystalDagger = CreateWeapon(21, 1500);
        private static readonly KeyValuePair<ISalable, int[]> Cutlass = CreateWeapon(44, 1500);
        private static readonly KeyValuePair<ISalable, int[]> IronEdge = CreateWeapon(6, 1500);
        private static readonly KeyValuePair<ISalable, int[]> BurglarsShank = CreateWeapon(18, 1500);
        private static readonly KeyValuePair<ISalable, int[]> WoodMallet = CreateWeapon(27, 2000);
        private static readonly KeyValuePair<ISalable, int[]> Claymore = CreateWeapon(10, 2000);
        private static readonly KeyValuePair<ISalable, int[]> TemplarsBlade = CreateWeapon(7, 4000);
        private static readonly KeyValuePair<ISalable, int[]> Kudgel = CreateWeapon(46, 4000);
        private static readonly KeyValuePair<ISalable, int[]> ShadowDagger = CreateWeapon(19, 2000);
        private static readonly KeyValuePair<ISalable, int[]> ObsidianEdge = CreateWeapon(8, 6000);
        private static readonly KeyValuePair<ISalable, int[]> TemperedBroadsword = CreateWeapon(52, 6000);
        private static readonly KeyValuePair<ISalable, int[]> WickedKris = CreateWeapon(45, 6000);
        private static readonly KeyValuePair<ISalable, int[]> BoneSword = CreateWeapon(5, 6000);
        private static readonly KeyValuePair<ISalable, int[]> OssifiedBlade = CreateWeapon(60, 6000);
        private static readonly KeyValuePair<ISalable, int[]> SteelFalchion = CreateWeapon(50, 9000);
        private static readonly KeyValuePair<ISalable, int[]> TheSlammer = CreateWeapon(28, 9000);
        private static readonly KeyValuePair<ISalable, int[]> LavaKatana = CreateWeapon(9, 25000);
        private static readonly KeyValuePair<ISalable, int[]> GalaxySword = CreateWeapon(4, 50000);
        private static readonly KeyValuePair<ISalable, int[]> GalaxyDagger = CreateWeapon(23, 35000);
        private static readonly KeyValuePair<ISalable, int[]> GalaxyHammer = CreateWeapon(29, 75000);
        private static readonly KeyValuePair<ISalable, int[]> Sneakers = CreateBoots(504, 500);
        private static readonly KeyValuePair<ISalable, int[]> LeatherBoots = CreateBoots(506, 500);
        private static readonly KeyValuePair<ISalable, int[]> WorkBoots = CreateBoots(507, 500);
        private static readonly KeyValuePair<ISalable, int[]> TundraBoots = CreateBoots(509, 750);
        private static readonly KeyValuePair<ISalable, int[]> ThermalBoots = CreateBoots(510, 1000);
        private static readonly KeyValuePair<ISalable, int[]> CombatBoots = CreateBoots(508, 1250);
        private static readonly KeyValuePair<ISalable, int[]> FirewalkerBoots = CreateBoots(512, 2000);
        private static readonly KeyValuePair<ISalable, int[]> DarkBoots = CreateBoots(511, 2500);
        private static readonly KeyValuePair<ISalable, int[]> SpaceBoots = CreateBoots(514, 5000);
        private static readonly KeyValuePair<ISalable, int[]> CrystalShoes = CreateBoots(878, 5000);
        private static readonly KeyValuePair<ISalable, int[]> AmethystRing = CreateRing(529, 1000);
        private static readonly KeyValuePair<ISalable, int[]> TopazRing = CreateRing(530, 1000);
        private static readonly KeyValuePair<ISalable, int[]> AquamarineRing = CreateRing(531, 2500);
        private static readonly KeyValuePair<ISalable, int[]> JadeRing = CreateRing(532, 2500);
        private static readonly KeyValuePair<ISalable, int[]> EmeraldRing = CreateRing(533, 5000);
        private static readonly KeyValuePair<ISalable, int[]> RubyRing = CreateRing(534, 5000);
        private static readonly KeyValuePair<ISalable, int[]> Slingshot = CreateSlingshot(32, 500);
        private static readonly KeyValuePair<ISalable, int[]> MasterSlingshot = CreateSlingshot(33, 1000);
        private static readonly KeyValuePair<ISalable, int[]> ExplosiveAmmo = CreateWithPrice(new Object("441", int.MaxValue), 300);
        private static readonly KeyValuePair<ISalable, int[]> SlimeCharmerRing = CreateRing(520, 25000);
        private static readonly KeyValuePair<ISalable, int[]> SavageRing = CreateRing(523, 25000);
        private static readonly KeyValuePair<ISalable, int[]> BurglarsRing = CreateRing(526, 20000);
        private static readonly KeyValuePair<ISalable, int[]> VampireRing = CreateRing(522, 15000);
        private static readonly KeyValuePair<ISalable, int[]> CrabshellRing = CreateRing(810, 15000);
        private static readonly KeyValuePair<ISalable, int[]> NapalmRing = CreateRing(811, 30000);
        private static readonly KeyValuePair<ISalable, int[]> SkeletonMask = CreateHat(8, 20000);
        private static readonly KeyValuePair<ISalable, int[]> HardHat = CreateHat(27, 20000);
        private static readonly KeyValuePair<ISalable, int[]> ArcaneHat = CreateHat(60, 20000);
        private static readonly KeyValuePair<ISalable, int[]> KnightsHelmet = CreateHat(50, 20000);
        private static readonly KeyValuePair<ISalable, int[]> InsectHead = CreateWeapon(13, 10000);
    }
}

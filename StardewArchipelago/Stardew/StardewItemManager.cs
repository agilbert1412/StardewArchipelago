using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewValley;

namespace StardewArchipelago.Stardew
{
    public class StardewItemManager
    {
        private Dictionary<int, StardewObject> _objectsById;
        private Dictionary<string, StardewObject> _objectsByName;
        private Dictionary<int, BigCraftable> _bigCraftablesById;
        private Dictionary<string, BigCraftable> _bigCraftablesByName;
        private Dictionary<int, Boots> _bootsById;
        private Dictionary<string, Boots> _bootsByName;
        private Dictionary<int, Weapon> _weaponsById;
        private Dictionary<string, Weapon> _weaponsByName;

        public StardewItemManager()
        {
            InitializeData();
        }

        public bool ItemExists(string itemName)
        {
            return _objectsByName.ContainsKey(itemName) || 
                   _bigCraftablesByName.ContainsKey(itemName) ||
                   _bootsByName.ContainsKey(itemName) ||
                   _weaponsByName.ContainsKey(itemName);
        }

        public StardewItem GetItemByName(string itemName)
        {
            if (_objectsByName.ContainsKey(itemName))
            {
                return _objectsByName[itemName];
            }

            if (_bigCraftablesByName.ContainsKey(itemName))
            {
                return _bigCraftablesByName[itemName];
            }

            if (_bootsByName.ContainsKey(itemName))
            {
                return _bootsByName[itemName];
            }

            if (_weaponsByName.ContainsKey(itemName))
            {
                return _weaponsByName[itemName];
            }

            throw new ArgumentException($"Item not found: {itemName}");
        }

        private void InitializeData()
        {
            InitializeObjects();
            InitializeBigCraftables();
            InitializeBoots();
            // InitializeClothing(); var allClothingInformation = Game1.clothingInformation;
            // InitializeFurniture();
            // InitializeHats();
            // InitializeTools();
            InitializeWeapons();
        }

        private void InitializeObjects()
        {
            _objectsById = new Dictionary<int, StardewObject>();
            _objectsByName = new Dictionary<string, StardewObject>();
            var allObjectData = Game1.objectInformation;
            foreach (var (id, objectInfo) in allObjectData)
            {
                var stardewItem = ParseStardewObjectData(id, objectInfo);

                if (_objectsById.ContainsKey(id) || _objectsByName.ContainsKey(stardewItem.Name))
                {
                    continue;
                }

                _objectsById.Add(id, stardewItem);
                _objectsByName.Add(stardewItem.Name, stardewItem);
            }
        }

        private void InitializeBigCraftables()
        {
            _bigCraftablesById = new Dictionary<int, BigCraftable>();
            _bigCraftablesByName = new Dictionary<string, BigCraftable>();
            var allBigCraftablesInformation = Game1.bigCraftablesInformation;
            foreach (var (id, bigCraftableInfo) in allBigCraftablesInformation)
            {
                var bigCraftable = ParseStardewBigCraftableData(id, bigCraftableInfo);

                if (_bigCraftablesById.ContainsKey(id) || _bigCraftablesByName.ContainsKey(bigCraftable.Name))
                {
                    continue;
                }

                _bigCraftablesById.Add(id, bigCraftable);
                _bigCraftablesByName.Add(bigCraftable.Name, bigCraftable);
            }
        }

        private void InitializeBoots()
        {
            _bootsById = new Dictionary<int, Boots>();
            _bootsByName = new Dictionary<string, Boots>();
            var allBootsInformation = (IDictionary<int, string>)Game1.content.Load<Dictionary<int, string>>("Data\\Boots");
            foreach (var (id, bootsInfo) in allBootsInformation)
            {
                var boots = ParseStardewBootsData(id, bootsInfo);

                if (_bootsById.ContainsKey(id) || _bootsByName.ContainsKey(boots.Name))
                {
                    continue;
                }

                _bootsById.Add(id, boots);
                _bootsByName.Add(boots.Name, boots);
            }
        }

        private void InitializeWeapons()
        {
            _weaponsById = new Dictionary<int, Weapon>();
            _weaponsByName = new Dictionary<string, Weapon>();
            var allWeaponsInformation = (IDictionary<int, string>)Game1.content.Load<Dictionary<int, string>>("Data\\Weapons");
            foreach (var (id, weaponsInfo) in allWeaponsInformation)
            {
                var weapon = ParseStardewWeaponData(id, weaponsInfo);

                if (_weaponsById.ContainsKey(id) || _weaponsByName.ContainsKey(weapon.Name))
                {
                    continue;
                }

                _weaponsById.Add(id, weapon);
                _weaponsByName.Add(weapon.Name, weapon);
            }
        }

        private static StardewObject ParseStardewObjectData(int id, string objectInfo)
        {
            var fields = objectInfo.Split("/");
            var name = fields[0];
            var sellPrice = int.Parse(fields[1]);
            var edibility = int.Parse(fields[2]);
            var typeAndCategory = fields[3].Split(" ");
            var objectType = typeAndCategory[0];
            var category = typeAndCategory.Length > 1 ? typeAndCategory[1] : "";
            var displayName = fields[4];
            var description = fields[5];

            var stardewItem = new StardewObject(id, name, sellPrice, edibility, objectType, category, displayName,
                description);
            return stardewItem;
        }

        private static BigCraftable ParseStardewBigCraftableData(int id, string objectInfo)
        {
            var fields = objectInfo.Split("/");
            var name = fields[0];
            var sellPrice = int.Parse(fields[1]);
            var edibility = int.Parse(fields[2]);
            var typeAndCategory = fields[3].Split(" ");
            var objectType = typeAndCategory[0];
            var category = typeAndCategory.Length > 1 ? typeAndCategory[1] : "";
            var description = fields[4];
            var outdoors = bool.Parse(fields[5]);
            var indoors = bool.Parse(fields[6]);
            var fragility = int.Parse(fields[7]);
            var displayName = fields.Last();

            var bigCraftable = new BigCraftable(id, name, sellPrice, edibility, objectType, category, description, outdoors, indoors, fragility, displayName);
            return bigCraftable;
        }

        private static Boots ParseStardewBootsData(int id, string objectInfo)
        {
            var fields = objectInfo.Split("/");
            var name = fields[0];
            var description = fields[1];
            var sellPrice = int.Parse(fields[2]);
            var addedDefense = int.Parse(fields[3]);
            var addedImmunity = int.Parse(fields[4]);
            var colorIndex = int.Parse(fields[5]);
            var displayName = fields.Length > 6 ? fields[6] : name;

            var bigCraftable = new Boots(id, name, sellPrice, description, addedDefense,
                addedImmunity, colorIndex, displayName);
            return bigCraftable;
        }

        private static Weapon ParseStardewWeaponData(int id, string objectInfo)
        {
            var fields = objectInfo.Split("/");
            var name = fields[0];
            var description = fields[1];
            var minDamage = int.Parse(fields[2]);
            var maxDamage = int.Parse(fields[3]);
            var knockBack = double.Parse(fields[4]);
            var speed = double.Parse(fields[5]);
            var addedPrecision = double.Parse(fields[6]);
            var addedDefence = double.Parse(fields[7]);
            var type = int.Parse(fields[8]);
            var baseMineLevel = int.Parse(fields[9]);
            var minMineLevel = int.Parse(fields[10]);
            var addedAoe = double.Parse(fields[11]);
            var criticalChance = double.Parse(fields[12]);
            var criticalDamage = double.Parse(fields[13]);
            var displayName = fields.Length > 14 ? fields[14] : name;

            var bigCraftable = new Weapon(id, name, description, minDamage, maxDamage, knockBack, speed,
                addedPrecision, addedDefence, type, baseMineLevel, minMineLevel, addedAoe, criticalChance,
                criticalDamage, displayName);
            return bigCraftable;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewValley;

namespace StardewArchipelago
{
    public class StardewItemManager
    {
        private Dictionary<int, StardewItem> _itemsById;
        private Dictionary<string, StardewItem> _itemsByName;

        public StardewItemManager()
        {
            InitializeData();
        }

        public void InitializeData()
        {
            _itemsById = new Dictionary<int, StardewItem>();
            _itemsByName = new Dictionary<string, StardewItem>();

            var allObjectData = Game1.objectInformation;
            foreach (var (id, objectInfo) in allObjectData)
            {
                var stardewItem = ParseStardewObjectData(id, objectInfo);

                if (_itemsById.ContainsKey(id) || _itemsByName.ContainsKey(stardewItem.Name))
                {
                    continue;
                }

                _itemsById.Add(id, stardewItem);
                _itemsByName.Add(stardewItem.Name, stardewItem);
            }
        }

        public StardewItem GetItemByName(string itemName)
        {
            return _itemsByName[itemName];
        }

        private static StardewItem ParseStardewObjectData(int id, string objectInfo)
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

            var stardewItem = new StardewItem(id, name, sellPrice, edibility, objectType, category, displayName,
                description);
            return stardewItem;
        }
    }
}

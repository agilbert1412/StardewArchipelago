using System.Collections.Generic;
using System.Globalization;
using StardewValley;
using Object = StardewValley.Object;

namespace StardewArchipelago.Stardew.NameMapping
{
    public class NameSimplifier
    {
        public NameSimplifier()
        {

        }

        public string GetSimplifiedName(Item item)
        {
            var name = item.Name;
            if (_renamedItems.ContainsKey(item.ParentSheetIndex))
            {
                name = _renamedItems[item.ParentSheetIndex];
            }

            if (name.Contains("moonslime.excavation."))
            {
                TextInfo ti = CultureInfo.CurrentCulture.TextInfo;
                var displayName = ti.ToTitleCase(item.DisplayName.Replace("Woooden", "Wooden"));
                if (name.Contains("strange_doll_green"))
                {
                    displayName += " (Green)";
                }
                name = displayName;
            }

            if (item is not Object shippedObject)
            {
                return name;
            }

            foreach (var simplifiedName in _simplifiedNames)
            {
                if (name.Contains(simplifiedName))
                {
                    return simplifiedName;
                }
            }

            if (shippedObject.preserve.Value.HasValue)
            {
                switch (shippedObject.preserve.Value.GetValueOrDefault())
                {
                    case Object.PreserveType.Wine:
                        return "Wine";
                    case Object.PreserveType.Jelly:
                        return "Jelly";
                    case Object.PreserveType.Pickle:
                        return "Pickles";
                    case Object.PreserveType.Juice:
                        return "Juice";
                    case Object.PreserveType.Roe:
                        return "Roe";
                    case Object.PreserveType.AgedRoe:
                        return "Aged Roe";
                }
            }

            return name;
        }

        private static readonly Dictionary<int, string> _renamedItems = new()
        {
            { 180, "Egg (Brown)" },
            { 182, "Large Egg (Brown)" },
            { 438, "Large Goat Milk" },
            { 223, "Cookies" },
        };

        private static readonly List<string> _simplifiedNames = new()
        {
            "Honey",
            "Secret Note",
        };
    }
}

using System;
using System.Linq;
using System.Collections.Generic;
using StardewArchipelago.Constants;
using StardewArchipelago.Stardew;
using StardewValley;

namespace StardewArchipelago.GameModifications
{
    public static class ModArchaeologyNameFixer
    {
        public static string NametoEnglishDisplayName(string recipeName)
        {
            if (!recipeName.Contains("moonslime.excavation."))
            {
                return recipeName;
            }
            return ArchaeologyCraftNames.CraftNames[recipeName];
            
        }

        public static string EnglishDisplayNametoName(string recipeName)
        {
            var displayNameList = ArchaeologyCraftNames.CraftNames.Values.ToList();
            if (!displayNameList.Contains(recipeName))
            {
                return recipeName;
            }
            var recipeEntry = ArchaeologyCraftNames.CraftNames.FirstOrDefault(x => x.Value == recipeName);
            return recipeEntry.Key;
            

        }
    }
}
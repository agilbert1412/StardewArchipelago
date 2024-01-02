using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using StardewArchipelago.Constants;
using StardewArchipelago.Goals;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Characters;

namespace StardewArchipelago.Items
{
    public class BabyBirther
    {
        public void SpawnNewBaby()
        {
            var seed = (int)(Game1.uniqueIDForThisGame + Game1.stats.DaysPlayed);
            var random = new Random(seed);
            var babyGender = random.NextDouble() < 0.5;
            var babyColor = random.NextDouble() < 0.5;
            var babyName = ChooseBabyName(random);

            var baby = new Child(babyName, babyGender, babyColor, Game1.player)
            {
                Age = 0,
                Position = new Vector2(16f, 4f) * 64f + new Vector2(0.0f, -24f)
            };
            Utility.getHomeOfFarmer(Game1.player).characters.Add(baby);
            Game1.playSound("smallSelect");
            var spouse = Game1.player.getSpouse();
            if (Game1.player.getChildrenCount() >= 2)
            {
                GoalCodeInjection.CheckFullHouseGoalCompletion();
            }

            if (spouse != null)
            {
                spouse.shouldSayMarriageDialogue.Value = true;
                spouse.currentMarriageDialogue.Insert(0, new MarriageDialogueReference("Data\\ExtraDialogue", "NewChild_Adoption", true, babyName));
            }
        }

        private string ChooseBabyName(Random random)
        {
            var npcNames = Game1.content.Load<Dictionary<string, string>>("Data\\NPCDispositions").Keys.ToHashSet();
            foreach (var npc in Utility.getAllCharacters())
            {
                npcNames.Add(npc.Name);
            }
            string babyName;
            var maxAttempts = Community.AllNames.Length * 10;
            var attempt = 0;
            do
            {
                attempt++;
                babyName = Community.AllNames[random.Next(0, Community.AllNames.Length)];
                if (attempt >= maxAttempts)
                {
                    while (npcNames.Contains(babyName))
                    {
                        babyName += " ";
                    }
                }
            }
            while (npcNames.Contains(babyName));

            return babyName;
        }
    }
}

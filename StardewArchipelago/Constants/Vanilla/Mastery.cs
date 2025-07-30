using StardewArchipelago.Locations.CodeInjections.Vanilla;

namespace StardewArchipelago.Constants.Vanilla
{
    public static class Mastery
    {
        public static readonly string FARMING = SkillMastery(nameof(Skill.Farming));
        public static readonly string MINING = SkillMastery(nameof(Skill.Mining));
        public static readonly string FORAGING = SkillMastery(nameof(Skill.Foraging));
        public static readonly string FISHING = SkillMastery(nameof(Skill.Fishing));
        public static readonly string COMBAT = SkillMastery(nameof(Skill.Combat));

        private static string SkillMastery(string skill)
        {
            return $"{skill} Mastery";
        }
    }
}

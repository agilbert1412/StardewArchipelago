using StardewArchipelago.Locations.CodeInjections.Vanilla;
using StardewValley;

namespace StardewArchipelago.GameModifications.MultiSleep
{
    public class MultiSleepUntilBehavior : MultiSleepBehavior
    {
        public const string RAIN = "Rain";
        public const string STORM = "Storm";
        public const string FESTIVAL = "Festival";
        public const string BIRTHDAY = "Birthday";
        public const string TRAVELING_CART = "Traveling Cart";
        public const string BOOKSELLER = "Bookseller";
        public const string END_OF_MONTH = "End of Month";

        private string _untilWhat;

        public MultiSleepUntilBehavior(string untilWhat)
        {
            _untilWhat = untilWhat;
        }

        public override bool ShouldKeepSleeping()
        {
            switch (_untilWhat)
            {
                case RAIN:
                    return !Game1.isRaining && !Game1.isLightning && !Game1.isGreenRain;
                case STORM:
                    return !Game1.isLightning;
                case FESTIVAL:
                    return !IsFestivalDay();
                case BIRTHDAY:
                    NPC birthdayPerson = null;
                    Utility.ForEachVillager(n =>
                    {
                        if (n.Birthday_Season == Game1.currentSeason && n.Birthday_Day == Game1.dayOfMonth)
                        {
                            birthdayPerson = n;
                        }
                        return birthdayPerson == null;
                    });
                    return birthdayPerson == null;
                case TRAVELING_CART:
                    return !TravelingMerchantInjections.IsTravelingMerchantDay(Game1.dayOfMonth);
                case BOOKSELLER:
                    return !Utility.getDaysOfBooksellerThisSeason().Contains(Game1.dayOfMonth);

                default:
                    return false;
            }
        }

        private static bool IsFestivalDay()
        {
            if (Utility.isFestivalDay(Game1.dayOfMonth, Game1.season))
            {
                return true;
            }

            foreach (var passiveFestivalData in DataLoader.PassiveFestivals(Game1.content).Values)
            {
                if (Game1.dayOfMonth < passiveFestivalData.StartDay || Game1.dayOfMonth > passiveFestivalData.EndDay || Game1.season != passiveFestivalData.Season || !GameStateQuery.CheckConditions(passiveFestivalData.Condition))
                {
                    continue;
                }

                return true;
            }

            return false;
        }
    }
}

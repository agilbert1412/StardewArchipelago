using StardewArchipelago.Archipelago.SlotData.SlotEnums;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;

namespace StardewArchipelago.Archipelago.SlotData
{
    public class JojapocalypseSlotData
    {
        public JojapocalypseSetting Jojapocalypse { get; set; }
        public int StartPrice { get; set; }
        public int EndPrice { get; set; }
        public JojapocalypsePricingPattern PricingPattern { get; set; }
        public int PurchasesBeforeMembership { get; set; }

        public JojapocalypseSlotData(ILogger logger, SlotDataReader slotDataReader)
        {
            Jojapocalypse = slotDataReader.GetSlotSetting(SlotDataKeys.JOJAPOCALYPSE, JojapocalypseSetting.Disabled);
            StartPrice = slotDataReader.GetSlotSetting(SlotDataKeys.JOJA_START_PRICE, 100);
            EndPrice = slotDataReader.GetSlotSetting(SlotDataKeys.JOJA_END_PRICE, 50000);
            PricingPattern = slotDataReader.GetSlotSetting(SlotDataKeys.JOJA_PRICING_PATTERN, JojapocalypsePricingPattern.Exponential);
            PurchasesBeforeMembership = slotDataReader.GetSlotSetting(SlotDataKeys.JOJA_PURCHASES_FOR_MEMBERSHIP, 0);
        }
    }

}

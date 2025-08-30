using Newtonsoft.Json;
using StardewArchipelago.Archipelago.SlotData.SlotEnums;
using System;
using System.Collections.Generic;
using System.Linq;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;

namespace StardewArchipelago.Archipelago.SlotData
{
    public class SlotDataReader
    {
        private ILogger _logger;
        private Dictionary<string, object> _slotDataFields;

        public SlotDataReader(ILogger logger, Dictionary<string, object> slotDataFields)
        {
            _logger = logger;
            _slotDataFields = slotDataFields;
        }

        public T GetSlotSetting<T>(string key, T defaultValue, params string[] alternateKeys) where T : struct, Enum, IConvertible
        {
            if (_slotDataFields.ContainsKey(key))
            {
                if (Enum.TryParse<T>(_slotDataFields[key].ToString(), true, out var parsedValue))
                {
                    return parsedValue;
                }
            }

            foreach (var alternateKey in alternateKeys)
            {
                if (_slotDataFields.ContainsKey(alternateKey))
                {
                    if (Enum.TryParse<T>(_slotDataFields[alternateKey].ToString(), true, out var parsedValue))
                    {
                        return parsedValue;
                    }
                }
            }

            return GetSlotDefaultValue(key, defaultValue);
        }

        public string GetSlotSetting(string key, string defaultValue)
        {
            return _slotDataFields.ContainsKey(key) ? _slotDataFields[key].ToString() : GetSlotDefaultValue(key, defaultValue);
        }

        public int GetSlotSetting(string key, int defaultValue)
        {
            return _slotDataFields.ContainsKey(key) ? (int)(long)_slotDataFields[key] : GetSlotDefaultValue(key, defaultValue);
        }

        public bool GetSlotSetting(string key, bool defaultValue)
        {
            if (!_slotDataFields.ContainsKey(key) || _slotDataFields[key] == null)
            {
                return GetSlotDefaultValue(key, defaultValue);
            }

            if (_slotDataFields[key] is bool boolValue)
            {
                return boolValue;
            }
            if (_slotDataFields[key] is string strValue && bool.TryParse(strValue, out var parsedValue))
            {
                return parsedValue;
            }
            if (_slotDataFields[key] is int intValue)
            {
                return intValue != 0;
            }
            if (_slotDataFields[key] is long longValue)
            {
                return longValue != 0;
            }
            if (_slotDataFields[key] is short shortValue)
            {
                return shortValue != 0;
            }

            return GetSlotDefaultValue(key, defaultValue);
        }

        public T GetSlotDefaultValue<T>(string key, T defaultValue)
        {
            _logger.LogWarning($"SlotData did not contain expected key: \"{key}\"");
            return defaultValue;
        }

        public StartWithout GetSlotStartWithoutSetting()
        {
            return GetSlotOptionSetSetting<StartWithout>(SlotDataKeys.START_WITHOUT);
        }

        public Chefsanity GetSlotChefsanitySetting()
        {
            return GetSlotOptionSetSetting<Chefsanity>(SlotDataKeys.CHEFSANITY);
        }

        public Eatsanity GetSlotEatsanitySetting()
        {
            return GetSlotOptionSetSetting<Eatsanity>(SlotDataKeys.EATSANITY);
        }

        public Walnutsanity GetSlotWalnutsanitySetting()
        {
            return GetSlotOptionSetSetting<Walnutsanity>(SlotDataKeys.WALNUTSANITY);
        }

        public Secretsanity GetSlotSecretsanitySetting()
        {
            return GetSlotOptionSetSetting<Secretsanity>(SlotDataKeys.SECRETSANITY);
        }

        public TEnum GetSlotOptionSetSetting<TEnum>(string key) where TEnum : struct, Enum
        {
            var enabledValues = 0;
            var slotJson = GetSlotSetting(key, "");
            if (string.IsNullOrWhiteSpace(slotJson))
            {
                return (TEnum)(object)enabledValues;
            }
            var slotItems = JsonConvert.DeserializeObject<List<string>>(slotJson);
            if (slotItems == null)
            {
                return (TEnum)(object)enabledValues;
            }

            slotItems = slotItems.Select(x => x.Replace(" ", "")).ToList();
            foreach (var enumValue in Enum.GetValues<TEnum>())
            {
                if (slotItems.Any(x => x.Equals(enumValue.ToString(), StringComparison.InvariantCultureIgnoreCase)))
                {
                    enabledValues |= (int)(object)enumValue;
                }
            }

            return (TEnum)(object)enabledValues;
        }
    }
}

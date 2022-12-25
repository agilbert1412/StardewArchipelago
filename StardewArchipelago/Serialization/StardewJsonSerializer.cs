using System.IO;
using Newtonsoft.Json;

namespace StardewArchipelago.Serialization
{
    public class StardewJsonSerializer
    {
        private JsonSerializer _serializer = new JsonSerializer()
        {
            Formatting = Formatting.Indented,
            //PreserveReferencesHandling = PreserveReferencesHandling.All,
            //ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
            //Converters = { new ArrayReferencePreservingConverter()},
            //TypeNameHandling = TypeNameHandling.Auto,
            //MaxDepth = int.MaxValue
        };

        public void SerializeToFile(ArchipelagoStateDto dto, string filePath)
        {
            using StreamWriter sw = new StreamWriter(filePath);
            using JsonWriter writer = new JsonTextWriter(sw);
            _serializer.Serialize(writer, dto);
        }

        public ArchipelagoStateDto DeserializeFromFile(string filePath)
        {
            using StreamReader sw = new StreamReader(filePath);
            using JsonReader reader = new JsonTextReader(sw);

            var dto = _serializer.Deserialize<ArchipelagoStateDto>(reader);

            return dto;
        }
    }
}
using System;
using Newtonsoft.Json;

namespace StardewArchipelago.ViewerEventsModule
{
    public class ViewerEventsConfig
    {
        public string DiscordToken { get; set; } = null;
        public string TwitchToken { get; set; } = null;


        public ViewerEventsConfig()
        {
        }

        public static bool TryReadConfig(string path, out ViewerEventsConfig config)
        {
            config = null;
            try
            {
                if (string.IsNullOrEmpty(path) || !File.Exists(path))
                {
                    return false;
                }

                var content = File.ReadAllText(path);
                if (string.IsNullOrWhiteSpace(content))
                {
                    return false;
                }

                config = JsonConvert.DeserializeObject<ViewerEventsConfig>(content);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}

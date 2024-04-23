using System;
using System.Threading.Tasks;
using StardewModdingAPI;

namespace StardewArchipelago.Extensions
{
    public static class TaskExtensions
    {
        private static IMonitor _log;

        public static void Initialize(IMonitor log)
        {
            _log = log;
        }

        public static async void FireAndForget(this Task task)
        {
            try
            {
                await task;
            }
            catch (Exception ex)
            {
                _log.Log($"Exception occurred in FireAndForget task: {ex}", LogLevel.Error);
            }
        }
    }
}

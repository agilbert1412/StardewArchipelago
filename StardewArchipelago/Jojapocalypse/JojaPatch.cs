using System.Reflection;
using HarmonyLib;

namespace StardewArchipelago.Jojapocalypse
{
    public class JojaPatch
    {
        public MethodBase Original { get; set; }
        public HarmonyMethod Prefix { get; set; }
        public HarmonyMethod Postfix { get; set; }
    }
}

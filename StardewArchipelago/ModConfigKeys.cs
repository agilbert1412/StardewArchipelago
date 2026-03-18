using System.Runtime.Serialization;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace StardewArchipelago;
/// <summary>A set of parsed key bindings.</summary>
public class ModConfigKeys
{
    /// <summary>The keys which open the next mail item.</summary>
    public KeybindList OpenMail { get; set; } = new(SButton.N);


    /// <summary>Normalize the model after it's deserialized.</summary>
    /// <param name="context">The deserialization context.</param>
    [OnDeserialized]
    public void OnDeserialized(StreamingContext context)
    {
        this.OpenMail ??= new KeybindList();
    }
}

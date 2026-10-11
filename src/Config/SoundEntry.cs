using System.Text.Json.Serialization;

namespace SankSounds;

public class SoundsConfig
{
    [JsonPropertyName("Sounds")]
    public Dictionary<string, SoundEntry> Sounds { get; set; } = new();
}

public class SoundEntry
{
    [JsonPropertyName("Name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("SoundEventName")]
    public string SoundEventName { get; set; } = "";

    [JsonPropertyName("Command")]
    public string Command { get; set; } = "";

    [JsonPropertyName("Cooldown")]
    public float Cooldown { get; set; } = -1;

    [JsonIgnore]
    public bool IsValid => !string.IsNullOrWhiteSpace(SoundEventName) && !string.IsNullOrWhiteSpace(Command);
}

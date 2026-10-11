using System.Text.Json.Serialization;

namespace SankSounds;

public class PluginConfig
{
    [JsonPropertyName("SoundEventFile")]
    public string SoundEventFile { get; set; } = "soundevents/soundevents_addon.vsndevts";

    [JsonPropertyName("DefaultCooldown")]
    public float DefaultCooldown { get; set; } = 30;

    [JsonPropertyName("PlayPermission")]
    public string PlayPermission { get; set; } = "sanksounds.play";

    [JsonPropertyName("AdminPermission")]
    public string AdminPermission { get; set; } = "general.admin";

    [JsonPropertyName("StopSoundsCommand")]
    public string StopSoundsCommand { get; set; } = "stopsounds";

    [JsonPropertyName("SoundListCommand")]
    public string SoundListCommand { get; set; } = "sounds";

    [JsonPropertyName("ReloadCommand")]
    public string ReloadCommand { get; set; } = "reloadsounds";
}

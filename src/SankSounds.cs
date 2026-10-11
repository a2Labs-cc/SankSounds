#nullable disable
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SwiftlyS2.Shared;
using SwiftlyS2.Shared.Commands;
using SwiftlyS2.Shared.Events;
using SwiftlyS2.Shared.Players;
using SwiftlyS2.Shared.Sounds;
using SwiftlyS2.Shared.Plugins;
using SwiftlyS2.Shared.Menus;
using SwiftlyS2.Core.Menus.OptionsBase;
using Volume.Contract;

namespace SankSounds;

[PluginMetadata(
    Id          = "SankSounds",
    Version     = "1.0.0",
    Name        = "Sank Sounds",
    Author      = "aga",
    Description = "Sank-style sound plugin for CS2 - play sounds via chat commands"
)]
public partial class SankSounds : BasePlugin
{
    private IOptionsMonitor<PluginConfig> _configMonitor;
    private PluginConfig Config => _configMonitor?.CurrentValue ?? new PluginConfig();

    private IOptionsMonitor<SoundsConfig> _soundsConfigMonitor;
    private SoundsConfig SoundsConfig => _soundsConfigMonitor?.CurrentValue ?? new SoundsConfig();

    // playerID -> cooldown expiry
    private readonly Dictionary<int, DateTime> _playerCooldowns = new();
    // playerID -> true if player has blocked sounds
    private readonly Dictionary<int, bool> _playerBlockedSounds = new();

    private readonly List<Guid> _commandIds = new();
    // command text (lowercase) -> sound key
    private readonly Dictionary<string, string> _commandToSoundKey = new();

    private const string VolumeFeatureKey  = "SankSounds";
    private const string VolumeFeatureName = "Sank Sounds";
    private IPlayerVolumeAPI _volumeApi;

    public SankSounds(ISwiftlyCore core) : base(core) { }

    public override void Load(bool hotReload)
    {
        Core.Configuration
            .InitializeJsonWithModel<PluginConfig>("config.jsonc", "Config")
            .InitializeJsonWithModel<SoundsConfig>("sounds.jsonc", "Sounds")
            .Configure(builder =>
            {
                builder.AddJsonFile(
                    Core.Configuration.GetConfigPath("config.jsonc"),
                    optional: false, reloadOnChange: true);
                builder.AddJsonFile(
                    Core.Configuration.GetConfigPath("sounds.jsonc"),
                    optional: true, reloadOnChange: true);
            });

        var services = new ServiceCollection();
        services.AddSwiftly(Core);
        services.AddSingleton<IConfiguration>(Core.Configuration.Manager);
        services.AddOptionsWithValidateOnStart<PluginConfig>().BindConfiguration("Config");
        services.AddOptionsWithValidateOnStart<SoundsConfig>().BindConfiguration("Sounds");

        var provider = services.BuildServiceProvider();
        _configMonitor = provider.GetRequiredService<IOptionsMonitor<PluginConfig>>();
        _soundsConfigMonitor = provider.GetRequiredService<IOptionsMonitor<SoundsConfig>>();

        Core.Logger.LogInformation("[SankSounds] Loading...");

        RegisterSoundCommands();
        RegisterUtilityCommands();

        Core.Event.OnClientDisconnected += OnClientDisconnected;
        Core.Event.OnPrecacheResource += OnPrecacheResource;

        LogSoundInventory();
    }

    public override void Unload()
    {
        foreach (var id in _commandIds)
            Core.Command.UnregisterCommand(id);
        _commandIds.Clear();
        _commandToSoundKey.Clear();

        Core.Event.OnClientDisconnected -= OnClientDisconnected;
        Core.Event.OnPrecacheResource -= OnPrecacheResource;

        _playerCooldowns.Clear();
        _playerBlockedSounds.Clear();

        if (_volumeApi != null)
        {
            try { _volumeApi.UnregisterFeature(VolumeFeatureKey); }
            catch (Exception ex) { Core.Logger.LogError(ex, "[SankSounds] Could not unregister volume feature"); }
        }
    }

    public override void UseSharedInterface(IInterfaceManager interfaceManager)
    {
        _volumeApi = null;
        try
        {
            if (!interfaceManager.HasSharedInterface("Volume.Player.v1")) return;
            _volumeApi = interfaceManager.GetSharedInterface<IPlayerVolumeAPI>("Volume.Player.v1");
            _volumeApi.RegisterFeature(VolumeFeatureKey, VolumeFeatureName);
            Core.Logger.LogInformation("[SankSounds] Connected to Volume.API ('{Key}').", VolumeFeatureKey);
        }
        catch (Exception ex)
        {
            Core.Logger.LogError(ex, "[SankSounds] Could not connect to Volume.API");
            _volumeApi = null;
        }
    }

    private void OnPrecacheResource(IOnPrecacheResourceEvent @event)
    {
        var eventFile = Config.SoundEventFile;
        if (!string.IsNullOrWhiteSpace(eventFile))
        {
            @event.AddItem(eventFile);
            Core.Logger.LogDebug("[SankSounds] Precaching sound event file: {File}", eventFile);
        }
    }

    private void OnClientDisconnected(IOnClientDisconnectedEvent @event)
    {
        int id = @event.PlayerId;
        _playerCooldowns.Remove(id);
        _playerBlockedSounds.Remove(id);
    }
}

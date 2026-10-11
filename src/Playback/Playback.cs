#nullable disable
using Microsoft.Extensions.Logging;
using SwiftlyS2.Shared.Players;
using SwiftlyS2.Shared.Sounds;

namespace SankSounds;

public partial class SankSounds
{
    private void PlaySound(IPlayer player, string soundKey, SoundEntry sound)
    {
        var cooldown = sound.Cooldown >= 0 ? sound.Cooldown : Config.DefaultCooldown;

        if (_playerCooldowns.TryGetValue(player.PlayerID, out var cdEnd) && DateTime.Now < cdEnd)
        {
            var remaining = (int)(cdEnd - DateTime.Now).TotalSeconds;
            SendLocalizedChat(player, "cooldown_active", remaining);
            return;
        }

        _playerCooldowns[player.PlayerID] = DateTime.Now.AddSeconds(Math.Max(0f, cooldown));

        var displayName = string.IsNullOrWhiteSpace(sound.Name) ? soundKey : sound.Name;
        var soundEventName = sound.SoundEventName;

        Core.Scheduler.NextTick(() =>
        {
            foreach (var target in Core.PlayerManager.GetAllValidPlayers())
            {
                if (target.IsFakeClient || !target.IsValid) continue;
                if (_playerBlockedSounds.TryGetValue(target.PlayerID, out var blk) && blk) continue;

                EmitSoundToPlayer(target, soundEventName);
            }
        });

        Core.Scheduler.NextTick(() =>
            SendLocalizedChatToAll("sound_played", player.Name, displayName));
    }

    private void EmitSoundToPlayer(IPlayer target, string soundEventName)
    {
        try
        {
            float volume = GetPlayerVolume(target);

            using var se = new SoundEvent(soundEventName, Math.Clamp(volume, 0f, 1f));
            se.Recipients.AddRecipient(target.Slot);
            se.Emit();
        }
        catch (Exception ex)
        {
            Core.Logger.LogError(ex, "[SankSounds] Failed to emit sound to {Name}", target.Name);
        }
    }

    private float GetPlayerVolume(IPlayer player)
    {
        if (_volumeApi == null) return 1.0f;
        try
        {
            return _volumeApi.GetEffectiveVolume((long)player.SteamID, VolumeFeatureKey);
        }
        catch (Exception ex)
        {
            Core.Logger.LogError(ex, "[SankSounds] Could not read volume for {Name}", player.Name);
            return 1.0f;
        }
    }
}

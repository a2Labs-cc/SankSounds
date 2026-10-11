#nullable disable
using Microsoft.Extensions.Logging;
using SwiftlyS2.Shared.Commands;

namespace SankSounds;

public partial class SankSounds
{
    private void RegisterSoundCommands()
    {
        foreach (var (key, sound) in SoundsConfig.Sounds)
        {
            if (!sound.IsValid)
            {
                Core.Logger.LogWarning("[SankSounds] Skipping invalid sound '{Key}': missing SoundEventName or Command", key);
                continue;
            }

            var cmdName = sound.Command.TrimStart('!').Trim();
            if (string.IsNullOrEmpty(cmdName)) continue;

            _commandToSoundKey[cmdName.ToLowerInvariant()] = key;

            var soundKey = key;
            var id = Core.Command.RegisterCommand(
                cmdName,
                (ctx) => OnSoundCommand(ctx, soundKey),
                registerRaw: true,
                permission: Config.PlayPermission,
                helpText: $"Play sound: {(string.IsNullOrWhiteSpace(sound.Name) ? key : sound.Name)}"
            );
            _commandIds.Add(id);
            Core.Logger.LogDebug("[SankSounds] Registered command '!{Cmd}' -> '{Key}'", cmdName, key);
        }
    }

    private void RegisterUtilityCommands()
    {
        if (!string.IsNullOrWhiteSpace(Config.StopSoundsCommand))
        {
            foreach (var name in Config.StopSoundsCommand.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                var id = Core.Command.RegisterCommand(name, OnStopSoundsCommand, registerRaw: true, permission: Config.AdminPermission, helpText: "Toggle blocking sounds for yourself");
                _commandIds.Add(id);
            }
        }

        if (!string.IsNullOrWhiteSpace(Config.SoundListCommand))
        {
            foreach (var name in Config.SoundListCommand.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                var id = Core.Command.RegisterCommand(name, OnSoundListCommand, registerRaw: true, helpText: "List all available sounds");
                _commandIds.Add(id);
            }
        }

        if (!string.IsNullOrWhiteSpace(Config.ReloadCommand))
        {
            foreach (var name in Config.ReloadCommand.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                var id = Core.Command.RegisterCommand(name, OnReloadCommand, registerRaw: true, permission: Config.AdminPermission, helpText: "Reload sounds config");
                _commandIds.Add(id);
            }
        }
    }

    private void OnSoundCommand(ICommandContext ctx, string soundKey)
    {
        var player = ctx.Sender;
        if (player == null || !player.IsValid || player.IsFakeClient) return;

        if (!HasPlayPermission(player))
        {
            SendLocalizedChat(player, "no_permission");
            return;
        }

        if (!SoundsConfig.Sounds.TryGetValue(soundKey, out var sound) || !sound.IsValid)
        {
            SendLocalizedChat(player, "sound_not_found", soundKey);
            return;
        }

        PlaySound(player, soundKey, sound);
    }

    private void OnStopSoundsCommand(ICommandContext ctx)
    {
        var player = ctx.Sender;
        if (player == null || !player.IsValid || player.IsFakeClient) return;

        bool blocked = _playerBlockedSounds.TryGetValue(player.PlayerID, out var b) && b;
        _playerBlockedSounds[player.PlayerID] = !blocked;

        SendLocalizedChat(player, blocked ? "sounds_unblocked" : "sounds_blocked");
    }

    private void OnSoundListCommand(ICommandContext ctx)
    {
        var player = ctx.Sender;
        if (player == null || !player.IsValid || player.IsFakeClient) return;

        if (SoundsConfig.Sounds.Count == 0)
        {
            SendLocalizedChat(player, "no_sounds");
            return;
        }

        OpenSoundsMenu(player);
    }

    private void OnReloadCommand(ICommandContext ctx)
    {
        var player = ctx.Sender;
        if (player != null && !player.IsFakeClient)
        {
            if (!string.IsNullOrEmpty(Config.AdminPermission) && !Core.Permission.PlayerHasPermission(player.SteamID, Config.AdminPermission))
            {
                SendLocalizedChat(player, "no_permission");
                return;
            }
        }

        LogSoundInventory();
        SendLocalizedReply(ctx, "sounds_reloaded", SoundsConfig.Sounds.Count);
    }
}

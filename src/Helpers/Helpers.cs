#nullable disable
using Microsoft.Extensions.Logging;
using SwiftlyS2.Shared.Commands;
using SwiftlyS2.Shared.Players;

namespace SankSounds;

public partial class SankSounds
{
    private bool HasPlayPermission(IPlayer player)
    {
        if (string.IsNullOrEmpty(Config.PlayPermission)) return true;
        return Core.Permission.PlayerHasPermission(player.SteamID, Config.PlayPermission);
    }

    private void SendLocalizedChat(IPlayer player, string key, params object[] args)
    {
        if (player.IsFakeClient) return;
        var loc = Core.Translation.GetPlayerLocalizer(player);
        player.SendChat(loc[key, args]);
    }

    private void SendLocalizedReply(ICommandContext ctx, string key, params object[] args)
    {
        if (ctx.Sender == null || ctx.Sender.IsFakeClient) return;
        var loc = Core.Translation.GetPlayerLocalizer(ctx.Sender);
        ctx.Reply(loc[key, args]);
    }

    private void SendLocalizedChatToAll(string key, params object[] args)
    {
        var firstPlayer = Core.PlayerManager.GetAllValidPlayers().FirstOrDefault(p => !p.IsFakeClient && p.IsValid);
        if (firstPlayer == null) return;

        var loc = Core.Translation.GetPlayerLocalizer(firstPlayer);
        string message = loc[key, args];
        Core.PlayerManager.SendChat(message);
    }

    private void LogSoundInventory()
    {
        int count = SoundsConfig.Sounds.Count;
        Core.Logger.LogInformation("[SankSounds] Loaded {Count} sound(s).", count);

        if (count == 0)
        {
            Core.Logger.LogWarning("[SankSounds] No sounds configured. Edit sounds.jsonc in the plugin config folder.");
            return;
        }

        foreach (var (key, sound) in SoundsConfig.Sounds)
        {
            if (!sound.IsValid)
                Core.Logger.LogWarning("[SankSounds] Sound '{Key}' is invalid (missing SoundEventName or Command).", key);
            else
                Core.Logger.LogDebug("[SankSounds] '{Key}' -> !{Cmd} (event: {Event})", key, sound.Command, sound.SoundEventName);
        }
    }
}

#nullable disable
using Microsoft.Extensions.Logging;
using SwiftlyS2.Shared.Players;
using SwiftlyS2.Core.Menus.OptionsBase;

namespace SankSounds;

public partial class SankSounds
{
    private void OpenSoundsMenu(IPlayer player)
    {
        try
        {
            var loc = Core.Translation.GetPlayerLocalizer(player);
            var title = loc["menu_title"];
            bool canPlay = HasPlayPermission(player);

            var builder = Core.MenusAPI.CreateBuilder();
            builder.EnableExit();

            foreach (var (soundKey, soundValue) in SoundsConfig.Sounds)
            {
                if (!soundValue.IsValid) continue;

                var key = soundKey;
                var val = soundValue;
                var displayName = string.IsNullOrWhiteSpace(val.Name) ? key : val.Name;
                var cmd = val.Command.StartsWith("!") ? val.Command : $"!{val.Command}";

                var btn = new ButtonMenuOption($"{displayName} ({cmd})") { CloseAfterClick = true };

                if (canPlay)
                {
                    btn.Click += async (sender, args) =>
                    {
                        Core.Scheduler.NextTick(() => PlaySound(args.Player, key, val));
                        await ValueTask.CompletedTask;
                    };
                }
                else
                {
                    btn.Click += async (sender, args) =>
                    {
                        Core.Scheduler.NextTick(() => SendLocalizedChat(args.Player, "no_permission"));
                        await ValueTask.CompletedTask;
                    };
                }

                builder.AddOption(btn);
            }

            var menu = builder.Build();
            menu.Configuration.Title = title;
            Core.MenusAPI.OpenMenuForPlayer(player, menu);
        }
        catch (Exception ex)
        {
            Core.Logger.LogError(ex, "[SankSounds] Failed to open menu for {Name}", player.Name);
            SendLocalizedChat(player, "menu_failed");
        }
    }
}

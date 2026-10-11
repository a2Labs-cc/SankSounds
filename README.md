<div align="center">

# [SwiftlyS2] SankSounds

<a href="https://github.com/a2Labs-cc/SankSounds/releases/latest">
  <img src="https://img.shields.io/github/v/release/a2Labs-cc/SankSounds?label=release&color=07f223&style=for-the-badge">
</a>
<a href="https://github.com/a2Labs-cc/SankSounds/issues">
  <img src="https://img.shields.io/github/issues/a2Labs-cc/SankSounds?label=issues&color=E63946&style=for-the-badge">
</a>
<a href="https://github.com/a2Labs-cc/SankSounds/releases">
  <img src="https://img.shields.io/github/downloads/a2Labs-cc/SankSounds/total?label=downloads&color=3A86FF&style=for-the-badge">
</a>
<a href="https://github.com/a2Labs-cc/SankSounds/stargazers">
  <img src="https://img.shields.io/github/stars/a2Labs-cc/SankSounds?label=stars&color=e3d322&style=for-the-badge">
</a>

<br/>
<sub>Made by <a href="https://github.com/agasking1337" target="_blank" rel="noopener noreferrer">aga</a></sub>

</div>


## Overview

**SwiftlyS2-SankSounds** is a SwiftlyS2 plugin inspired by the classic Sank Sounds plugin. Players trigger sounds via chat commands (e.g. `!bruh`, `!nani`), and each sound has its own configurable cooldown.

Sounds are played through addon sound events (`.vsndevts`), so the sound event file is configured once in the main config and shared by all sounds.

For shared volume controls (`!volume` / `!vol`), install [Volume.API](https://github.com/a2Labs-cc/Volume.API) and its exported contract.

## Features

- **Chat command per sound**: Each sound gets its own `!command` trigger
- **Per-sound cooldown**: Configurable per sound, or fall back to the global default
- **Sound menu**: `!sounds` opens a menu with every available sound
- **Volume.API integration**: Volume is controlled through the shared Volume.API (`!volume`)
- **Player sound blocking**: `!stopsounds` toggles blocking sounds for yourself
- **Hot-reloadable config**: Config changes are picked up automatically
- **Admin reload command**: `!reloadsounds`
- **Translation support**: All messages are translatable

## Support

Need help or have questions? Join our Discord server:

<p align="center">
  <a href="https://discord.gg/d853jMW2gh" target="_blank">
    <img src="https://img.shields.io/badge/Join%20Discord-5865F2?logo=discord&logoColor=white&style=for-the-badge" alt="Discord">
  </a>
</p>


## Download Shortcuts
<ul>
  <li>
    <code>📦</code>
    <strong>&nbsp;Download Latest Plugin Version</strong> &rarr;
    <a href="https://github.com/a2Labs-cc/SankSounds/releases/latest" target="_blank" rel="noopener noreferrer">Click Here</a>
  </li>
  <li>
    <code>⚙️</code>
    <strong>&nbsp;Download Volume.API for shared volume controls</strong> &rarr;
    <a href="https://github.com/a2Labs-cc/Volume.API" target="_blank" rel="noopener noreferrer">Click Here</a>
  </li>
  <li>
    <code>⚙️</code>
    <strong>&nbsp;Download Latest Addons Manager</strong> &rarr;
    <a href="https://github.com/SwiftlyS2-Plugins/AddonsManager/releases/latest" target="_blank" rel="noopener noreferrer">Click Here</a>
  </li>
  <li>
    <code>⚙️</code>
    <strong>&nbsp;Download Latest SwiftlyS2 Version</strong> &rarr;
    <a href="https://github.com/swiftly-solution/swiftlys2/releases/latest" target="_blank" rel="noopener noreferrer">Click Here</a>
  </li>
</ul>

## Installation

1. Download/build the plugin (`dotnet publish -c Release`; output lands in `build/publish/SankSounds/`).
2. Copy the published plugin folder to your server:

```
.../game/csgo/addons/swiftlys2/plugins/SankSounds/
```
3. Ensure the `resources/` folder (translations, templates) is alongside the DLL.
4. Make sure the workshop addon containing your sound events (`.vsndevts`) is loaded on the server (e.g. via Addons Manager) and that `SoundEventFile` in `config.jsonc` points to it.
5. Install [Volume.API](https://github.com/a2Labs-cc/Volume.API) for `!volume` / `!vol`, with its `resources/exports/Volume.Contract.dll`, and install the Cookies plugin to persist player settings. Keep the Volume.API plugin folder intact: SwiftlyS2 loads its shared contract from `resources/exports/`.
6. Start/restart the server.

## Configuration

The plugin uses SwiftlyS2's JSON config system.

- **Files**: `config.jsonc` and `sounds.jsonc`
- **Location**: `game/csgo/addons/swiftlys2/configs/plugins/SankSounds/`

On first run the configs are created automatically.

### config.jsonc

```jsonc
{
  "Config": {
    // .vsndevts file to precache (shared across all sounds)
    "SoundEventFile": "soundevents/soundevents_addon.vsndevts",
    // Default cooldown in seconds between sounds per player
    "DefaultCooldown": 30,
    // Permission required to play sounds (empty = everyone)
    "PlayPermission": "sanksounds.play",
    // Permission required for admin commands
    "AdminPermission": "general.admin",
    // Command(s) to toggle sound blocking for yourself (comma separated)
    "StopSoundsCommand": "stopsounds",
    // Command(s) to open the sound list menu (comma separated)
    "SoundListCommand": "sounds",
    // Command(s) to reload sounds config (comma separated)
    "ReloadCommand": "reloadsounds"
  }
}
```

### sounds.jsonc

Each sound entry has:

- `Name`: Display name shown in chat and the menu
- `SoundEventName`: The sound event name from the `.vsndevts` file
- `Command`: Chat command that triggers the sound (with or without `!`)
- `Cooldown`: Per-sound cooldown in seconds (`-1` = use `DefaultCooldown`)

```jsonc
{
  "Sounds": {
    "Sounds": {
      "bruh": {
        "Name": "Bruh",
        "SoundEventName": "soundchat.bruh",
        "Command": "bruh",
        "Cooldown": -1
      },
      "nani": {
        "Name": "Nani",
        "SoundEventName": "soundchat.nani",
        "Command": "nani",
        "Cooldown": 15
      }
    }
  }
}
```

### Commands

| Command | Description | Permission |
|---------|-------------|------------|
| `!<sound_command>` | Play a specific sound | `PlayPermission` |
| `!sounds` | Open the sound list menu | None |
| `!stopsounds` | Toggle sound blocking for yourself | `AdminPermission` |
| `!reloadsounds` | Reload sounds configuration | `AdminPermission` |

The shared Volume plugin owns `!volume` and `!vol`: use either command without arguments to open the volume slider menu, `!volume <0-10>` for all sound features, or `!volume SankSounds <0-10>` for SankSounds only. If the shared interface is unavailable, sounds play at full volume.

## Building

```bash
dotnet publish -c Release
```

The output will be in `build/publish/SankSounds/`.

## Credits
- Original plugin based on [SyntX34](https://github.com/SyntX34/swiftlys2-AdminSounds)
- Readme template by [criskkky](https://github.com/criskkky)

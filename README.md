[![dotnet package](https://github.com/k0x1a4/cs2-votemap/actions/workflows/release.yml/badge.svg)](https://github.com/k0x1a4/cs2-votemap/actions/workflows/release.yml)

# VoteMap - Rock The Vote for CS2

A classic Rock The Vote plugin for Counter-Strike 2 using CounterStrikeSharp. Allows players to vote for map changes mid-game and nominate maps for upcoming votes.

## Features

- **Rock The Vote** - Players can vote to change the map when enough players agree
- **Map Nominations** - Players can nominate maps with partial name matching
- **Configurable Menu** - Choose between HTML (center screen) or Chat-based voting menu
- **End of Map Voting** - Automatic vote when the match ends
- **Cooldown System** - Prevent RTV spam with round-based cooldowns

## Requirements

- [CounterStrikeSharp](https://github.com/roflmuffin/CounterStrikeSharp) v1.0.364 or later
- .NET 8.0 Runtime

## Installation

1. Build the plugin or download the release
2. Copy the `VoteMap` folder to `addons/counterstrikesharp/plugins/`
3. Restart the server or load the plugin with `css_plugins load VoteMap`
4. Config file will be generated at `addons/counterstrikesharp/configs/plugins/VoteMap/VoteMap.json`

## Commands

| Command | Description | Usage |
|---------|-------------|-------|
| `!rtv` | Vote to rock the vote | `!rtv` |
| `!rockthevote` | Alias for !rtv | `!rockthevote` |
| `!nominate` | Nominate a map for the vote | `!nominate <mapname>` |
| `!nextmap` | Display the next map if decided | `!nextmap` |
| `!currentmap` | Display the current map | `!currentmap` |
| `!listmaps` | List all available maps | `!listmaps` |
| `!maps` | Alias for !listmaps | `!maps` |

## Permissions

This plugin does not require any special permissions. All commands are available to all players by default.

For server administrators wanting to restrict commands, you can use CounterStrikeSharp's permission system by modifying the command registrations in the source code with the `[RequiresPermissions]` attribute.

## Configuration

The configuration file is automatically generated at:
```
addons/counterstrikesharp/configs/plugins/VoteMap/VoteMap.json
```

### Config Options

```json
{
  "RtvPercentage": 0.6,
  "VoteDuration": 30,
  "MapsInVote": 5,
  "EnableNominations": true,
  "MinPlayersForRtv": 2,
  "RtvCooldownRounds": 3,
  "ChangeMapDelay": 5.0,
  "MenuType": "Html",
  "EnableEndOfMapVote": true,
  "ChatPrefix": "[RTV]",
  "MapList": [
    "de_dust2",
    "de_mirage",
    "de_inferno",
    "de_nuke",
    "de_overpass",
    "de_ancient",
    "de_anubis",
    "de_vertigo"
  ],
  "ConfigVersion": 1
}
```

### Config Reference

| Option | Type | Default | Description |
|--------|------|---------|-------------|
| `RtvPercentage` | float | `0.6` | Percentage of players needed to trigger a vote (0.0 - 1.0) |
| `VoteDuration` | int | `30` | Duration of the vote in seconds |
| `MapsInVote` | int | `5` | Number of maps to show in the vote menu |
| `EnableNominations` | bool | `true` | Allow players to nominate maps |
| `MinPlayersForRtv` | int | `2` | Minimum players required to start RTV |
| `RtvCooldownRounds` | int | `3` | Rounds after map start before RTV is available |
| `ChangeMapDelay` | float | `5.0` | Seconds to wait before changing map after vote ends |
| `MenuType` | string | `Html` | Vote menu type: `Html` (center screen) or `Chat` |
| `EnableEndOfMapVote` | bool | `true` | Automatically start a vote when the match ends |
| `ChatPrefix` | string | `[RTV]` | Prefix for chat messages |
| `MapList` | array | *see above* | List of map names available for voting |

### Workshop Maps

**Using a Workshop Collection (Recommended)**

If you're hosting a workshop collection using `HOST_WORKSHOP_COLLECTION`, just add the map names directly to the config:

```json
{
  "MapList": [
    "de_dust2",
    "de_thera",
    "aim_botz",
    "my_custom_map"
  ]
}
```

**Example server launch options with workshop collection:**
```
+host_workshop_collection 3700001633
```

With this setup, you can use the actual map names (like `de_thera`) instead of workshop IDs.

**Not using a collection?**

If you're not hosting a workshop collection, you'll need to load maps by their workshop ID. In this case, add the workshop ID as the map name:

```json
{
  "MapList": [
    "de_dust2",
    "3121217565"
  ]
}
```

The plugin will use `ds_workshop_changelevel` for maps that aren't found locally.

## License

MIT License

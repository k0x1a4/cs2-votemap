using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;

namespace VoteMap;

public class VoteMapPlugin : BasePlugin, IPluginConfig<VoteMapConfig> {
    public override string ModuleName => "VoteMap";
    public override string ModuleVersion => "1.0.0";
    public override string ModuleAuthor => "k0x1a4";
    public override string ModuleDescription => "Rock The Vote - Vote for a new map";

    public VoteMapConfig Config { get; set; } = new();

    private MapManager _mapManager = null!;
    private VoteManager _voteManager = null!;

    public void OnConfigParsed(VoteMapConfig config) {
        Config = config;
    }

    public override void Load(bool hotReload) {
        _mapManager = new MapManager(Config);
        _voteManager = new VoteManager(Config, _mapManager, BroadcastMessage);

        RegisterListener<Listeners.OnMapStart>(OnMapStart);
        RegisterListener<Listeners.OnClientDisconnect>(OnClientDisconnect);

        RegisterEventHandler<EventRoundStart>(OnRoundStart);
        RegisterEventHandler<EventCsWinPanelMatch>(OnMatchEnd);

        AddCommand("css_rtv", "Rock the vote to change the map", CommandRtv);
        AddCommand("css_rockthevote", "Rock the vote to change the map", CommandRtv);
        AddCommand("css_nominate", "Nominate a map for the vote", CommandNominate);
        AddCommand("css_nextmap", "Show the next map", CommandNextMap);
        AddCommand("css_currentmap", "Show the current map", CommandCurrentMap);
        AddCommand("css_listmaps", "List all available maps", CommandListMaps);
        AddCommand("css_maps", "List all available maps", CommandListMaps);

        Console.WriteLine("VoteMap plugin loaded!");

        if (hotReload) {
            _mapManager.SetCurrentMap(Server.MapName);
        }
    }

    public override void Unload(bool hotReload) {
        Console.WriteLine("VoteMap plugin unloaded!");
    }

    private void OnMapStart(string mapName) {
        _mapManager.SetCurrentMap(mapName);
        _voteManager.Reset();
    }

    private void OnClientDisconnect(int playerSlot) {
        _mapManager.RemoveNomination(playerSlot);
        _voteManager.RemovePlayer(playerSlot);
    }

    private HookResult OnRoundStart(EventRoundStart @event, GameEventInfo info) {
        _voteManager.OnRoundStart();
        return HookResult.Continue;
    }

    private HookResult OnMatchEnd(EventCsWinPanelMatch @event, GameEventInfo info) {
        _voteManager.ForceEndOfMapVote(this);
        return HookResult.Continue;
    }

    [CommandHelper(whoCanExecute: CommandUsage.CLIENT_ONLY)]
    private void CommandRtv(CCSPlayerController? player, CommandInfo command) {
        if (player == null)
            return;

        var (success, message) = _voteManager.TryRtv(player);

        if (!success) {
            player.PrintToChatPrefixed(Config.ChatPrefix, message);
            return;
        }

        if (!string.IsNullOrEmpty(message)) {
            BroadcastMessage(message);
        }

        if (_voteManager.ShouldStartVote()) {
            _voteManager.StartVote(this);
        }
    }

    [CommandHelper(minArgs: 0, usage: "[mapname]", whoCanExecute: CommandUsage.CLIENT_ONLY)]
    private void CommandNominate(CCSPlayerController? player, CommandInfo command) {
        if (player == null)
            return;

        if (!Config.EnableNominations) {
            player.PrintToChatPrefixed(Config.ChatPrefix, "Nominations are disabled.");
            return;
        }

        if (_voteManager.VoteInProgress) {
            player.PrintToChatPrefixed(Config.ChatPrefix, "Cannot nominate during a vote.");
            return;
        }

        if (_voteManager.VoteCompleted) {
            player.PrintToChatPrefixed(Config.ChatPrefix, $"Vote already completed. Next map: {_voteManager.NextMap}");
            return;
        }

        var mapArg = command.ArgByIndex(1);

        if (string.IsNullOrWhiteSpace(mapArg)) {
            var currentNom = _mapManager.GetNomination(player);
            if (currentNom != null) {
                player.PrintToChatPrefixed(Config.ChatPrefix,
                    $"Your current nomination: {ChatColors.LightBlue}{currentNom}");
            }
            else {
                player.PrintToChatPrefixed(Config.ChatPrefix, "Usage: !nominate <mapname>");
            }

            return;
        }

        var matches = _mapManager.FindMaps(mapArg);

        if (matches.Count == 0) {
            player.PrintToChatPrefixed(Config.ChatPrefix, $"No maps found matching '{mapArg}'");
            return;
        }

        if (matches.Count > 1) {
            var mapNames = string.Join(", ", matches.Take(5));
            player.PrintToChatPrefixed(Config.ChatPrefix, $"Multiple maps found: {mapNames}");
            return;
        }

        var map = matches[0];

        if (map.Equals(_mapManager.CurrentMap, StringComparison.OrdinalIgnoreCase)) {
            player.PrintToChatPrefixed(Config.ChatPrefix, "Cannot nominate the current map.");
            return;
        }

        if (_mapManager.Nominate(player, mapArg)) {
            BroadcastMessage($"{player.PlayerName} nominated {ChatColors.LightBlue}{map}");
        }
    }

    [CommandHelper(whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    private void CommandNextMap(CCSPlayerController? player, CommandInfo command) {
        var nextMap = _voteManager.NextMap;
        var message = nextMap != null
            ? $"Next map: {ChatColors.LightBlue}{nextMap}"
            : "Next map has not been decided yet.";

        if (player != null) {
            player.PrintToChatPrefixed(Config.ChatPrefix, message);
        }
        else {
            Console.WriteLine($"[VoteMap] {message}");
        }
    }

    [CommandHelper(whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    private void CommandCurrentMap(CCSPlayerController? player, CommandInfo command) {
        var message = $"Current map: {ChatColors.LightBlue}{_mapManager.CurrentMap}";

        if (player != null) {
            player.PrintToChatPrefixed(Config.ChatPrefix, message);
        }
        else {
            Console.WriteLine($"[VoteMap] Current map: {_mapManager.CurrentMap}");
        }
    }

    [CommandHelper(whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    private void CommandListMaps(CCSPlayerController? player, CommandInfo command) {
        var maps = Config.MapList;

        if (player != null) {
            player.PrintToChatPrefixed(Config.ChatPrefix, $"Available maps ({maps.Count}):");
            foreach (var map in maps) {
                var isCurrent = map.Equals(_mapManager.CurrentMap, StringComparison.OrdinalIgnoreCase);
                var prefix = isCurrent ? $"{ChatColors.Green}> " : "  ";
                player.PrintToChat(
                    $" {prefix}{ChatColors.LightBlue}{map}{ChatColors.Default}{(isCurrent ? " (current)" : "")}");
            }
        }
        else {
            Console.WriteLine($"[VoteMap] Available maps ({maps.Count}):");
            foreach (var map in maps) {
                var isCurrent = map.Equals(_mapManager.CurrentMap, StringComparison.OrdinalIgnoreCase);
                Console.WriteLine($"  {map}{(isCurrent ? " (current)" : "")}");
            }
        }
    }

    private void BroadcastMessage(string message) {
        Extensions.PrintToChatAll(Config.ChatPrefix, message);
    }
}
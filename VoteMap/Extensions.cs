using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

namespace VoteMap;

public static class Extensions {
    public static void PrintToChatAll(string prefix, string message) {
        foreach (var player in Utilities.GetPlayers().Where(p => p.IsValid && !p.IsBot && !p.IsHLTV)) {
            player.PrintToChat($" {ChatColors.Green}{prefix}{ChatColors.Default} {message}");
        }
    }

    public static void PrintToChatPrefixed(this CCSPlayerController player, string prefix, string message) {
        player.PrintToChat($" {ChatColors.Green}{prefix}{ChatColors.Default} {message}");
    }

    public static List<CCSPlayerController> GetValidPlayers() {
        return Utilities.GetPlayers()
            .Where(p => p.IsValid && !p.IsBot && !p.IsHLTV)
            .ToList();
    }
}
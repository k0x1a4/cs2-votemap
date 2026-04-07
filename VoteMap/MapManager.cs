using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;

namespace VoteMap;

public class MapManager {
    private readonly VoteMapConfig _config;
    private readonly Dictionary<int, string> _nominations = new();
    private string _currentMap = string.Empty;

    public MapManager(VoteMapConfig config) {
        _config = config;
    }

    public void SetCurrentMap(string mapName) {
        _currentMap = mapName;
        _nominations.Clear();
    }

    public string CurrentMap => _currentMap;

    public bool Nominate(CCSPlayerController player, string mapName) {
        var matchedMap = FindMap(mapName);
        if (matchedMap == null)
            return false;

        _nominations[player.Slot] = matchedMap;
        return true;
    }

    public string? GetNomination(CCSPlayerController player) {
        return _nominations.TryGetValue(player.Slot, out var map) ? map : null;
    }

    public void RemoveNomination(int playerSlot) {
        _nominations.Remove(playerSlot);
    }

    public string? FindMap(string partialName) {
        var lowerPartial = partialName.ToLower();

        var exactMatch = _config.MapList.FirstOrDefault(m =>
            m.Equals(partialName, StringComparison.OrdinalIgnoreCase));
        if (exactMatch != null)
            return exactMatch;

        var matches = _config.MapList
            .Where(m => m.ToLower().Contains(lowerPartial))
            .ToList();

        return matches.Count == 1 ? matches[0] : null;
    }

    public List<string> FindMaps(string partialName) {
        var lowerPartial = partialName.ToLower();
        return _config.MapList
            .Where(m => m.ToLower().Contains(lowerPartial))
            .ToList();
    }

    public List<string> GetMapsForVote(int count) {
        var maps = new List<string>();
        var nominations = _nominations.Values.Distinct().ToList();

        foreach (var nom in nominations) {
            if (!IsCurrentMap(nom) && maps.Count < count)
                maps.Add(nom);
        }

        var availableMaps = _config.MapList
            .Where(m => !IsCurrentMap(m) && !maps.Contains(m))
            .ToList();

        var random = new Random();
        while (maps.Count < count && availableMaps.Count > 0) {
            var index = random.Next(availableMaps.Count);
            maps.Add(availableMaps[index]);
            availableMaps.RemoveAt(index);
        }

        return maps;
    }

    private bool IsCurrentMap(string map) {
        return map.Equals(_currentMap, StringComparison.OrdinalIgnoreCase);
    }

    public IReadOnlyDictionary<int, string> Nominations => _nominations;

    public bool IsValidMap(string mapName) {
        return _config.MapList.Contains(mapName, StringComparer.OrdinalIgnoreCase);
    }

    public void ChangeMap(string mapName) {
        if (Server.IsMapValid(mapName)) {
            Server.ExecuteCommand($"changelevel {mapName}");
        }
        else {
            Server.ExecuteCommand($"ds_workshop_changelevel {mapName}");
        }
    }
}
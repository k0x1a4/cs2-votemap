using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Menu;
using Timer = CounterStrikeSharp.API.Modules.Timers.Timer;

namespace VoteMap;

public class VoteManager {
    private readonly VoteMapConfig _config;
    private readonly MapManager _mapManager;
    private readonly Action<string> _broadcastMessage;

    private readonly HashSet<int> _rtvVoters = new();
    private readonly Dictionary<int, string> _mapVotes = new();
    private List<string> _currentVoteMaps = new();

    private bool _voteInProgress;
    private bool _voteCompleted;
    private string? _nextMap;
    private int _roundsSinceMapStart;
    private Timer? _voteTimer;

    public VoteManager(VoteMapConfig config, MapManager mapManager, Action<string> broadcastMessage) {
        _config = config;
        _mapManager = mapManager;
        _broadcastMessage = broadcastMessage;
    }

    public void Reset() {
        _rtvVoters.Clear();
        _mapVotes.Clear();
        _currentVoteMaps.Clear();
        _voteInProgress = false;
        _voteCompleted = false;
        _nextMap = null;
        _roundsSinceMapStart = 0;
        _voteTimer?.Kill();
        _voteTimer = null;
    }

    public void OnRoundStart() {
        _roundsSinceMapStart++;
    }

    public void RemovePlayer(int playerSlot) {
        _rtvVoters.Remove(playerSlot);
        _mapVotes.Remove(playerSlot);
    }

    public bool VoteInProgress => _voteInProgress;
    public bool VoteCompleted => _voteCompleted;
    public string? NextMap => _nextMap;

    public (bool success, string message) TryRtv(CCSPlayerController player) {
        if (_voteCompleted)
            return (false, "Vote already completed. Next map: " + _nextMap);

        if (_voteInProgress)
            return (false, "A vote is already in progress!");

        if (_roundsSinceMapStart < _config.RtvCooldownRounds)
            return (false, $"RTV will be available in {_config.RtvCooldownRounds - _roundsSinceMapStart} rounds.");

        if (_rtvVoters.Contains(player.Slot))
            return (false, "You have already voted to rock the vote!");

        _rtvVoters.Add(player.Slot);

        var players = Utilities.GetPlayers()
            .Where(p => p.IsValid && !p.IsBot && !p.IsHLTV)
            .ToList();

        var needed = Math.Max(_config.MinPlayersForRtv, (int)Math.Ceiling(players.Count * _config.RtvPercentage));
        var current = _rtvVoters.Count;

        if (current >= needed) {
            return (true, string.Empty);
        }

        return (true, $"{player.PlayerName} wants to rock the vote! ({current}/{needed})");
    }

    public bool ShouldStartVote() {
        var players = Utilities.GetPlayers()
            .Where(p => p.IsValid && !p.IsBot && !p.IsHLTV)
            .ToList();

        if (players.Count < _config.MinPlayersForRtv)
            return false;

        var needed = Math.Max(_config.MinPlayersForRtv, (int)Math.Ceiling(players.Count * _config.RtvPercentage));
        return _rtvVoters.Count >= needed;
    }

    public void StartVote(BasePlugin plugin) {
        if (_voteInProgress || _voteCompleted)
            return;

        _voteInProgress = true;
        _mapVotes.Clear();
        _currentVoteMaps = _mapManager.GetMapsForVote(_config.MapsInVote);

        _broadcastMessage($"Map vote started! Vote now! ({_config.VoteDuration} seconds)");

        var menu = CreateVoteMenu();

        foreach (var player in Utilities.GetPlayers().Where(p => p.IsValid && !p.IsBot && !p.IsHLTV)) {
            OpenMenuForPlayer(player, menu);
        }

        _voteTimer = plugin.AddTimer(_config.VoteDuration, () => EndVote(plugin));
    }

    private IMenu CreateVoteMenu() {
        if (_config.MenuType == MenuType.Html) {
            var menu = new CenterHtmlMenu("Vote for the next map");
            foreach (var map in _currentVoteMaps) {
                menu.AddMenuOption(map, (player, option) => HandleVote(player, map));
            }

            return menu;
        }
        else {
            var menu = new ChatMenu("Vote for the next map");
            foreach (var map in _currentVoteMaps) {
                menu.AddMenuOption(map, (player, option) => HandleVote(player, map));
            }

            return menu;
        }
    }

    private void OpenMenuForPlayer(CCSPlayerController player, IMenu menu) {
        if (_config.MenuType == MenuType.Html) {
            MenuManager.OpenCenterHtmlMenu(null!, player, (CenterHtmlMenu)menu);
        }
        else {
            MenuManager.OpenChatMenu(player, (ChatMenu)menu);
        }
    }

    private void HandleVote(CCSPlayerController player, string map) {
        if (!_voteInProgress)
            return;

        var previousVote = _mapVotes.TryGetValue(player.Slot, out var prev) ? prev : null;
        _mapVotes[player.Slot] = map;

        if (previousVote != null) {
            player.PrintToChat($"{_config.ChatPrefix} Changed vote to {map}");
        }
        else {
            player.PrintToChat($"{_config.ChatPrefix} Voted for {map}");
        }
    }

    private void EndVote(BasePlugin plugin) {
        _voteInProgress = false;
        _voteTimer = null;

        if (_mapVotes.Count == 0) {
            _broadcastMessage("No votes cast. Map will not change.");
            _voteCompleted = true;
            return;
        }

        var results = _mapVotes
            .GroupBy(v => v.Value)
            .Select(g => new { Map = g.Key, Votes = g.Count() })
            .OrderByDescending(r => r.Votes)
            .ToList();

        var winner = results.First();
        _nextMap = winner.Map;
        _voteCompleted = true;

        var resultMessage = string.Join(", ", results.Select(r => $"{r.Map}: {r.Votes}"));
        _broadcastMessage($"Vote ended! Results: {resultMessage}");
        _broadcastMessage($"Next map: {_nextMap} (changing in {_config.ChangeMapDelay} seconds)");

        plugin.AddTimer(_config.ChangeMapDelay, () => { _mapManager.ChangeMap(_nextMap); });
    }

    public void ForceEndOfMapVote(BasePlugin plugin) {
        if (!_config.EnableEndOfMapVote || _voteInProgress || _voteCompleted)
            return;

        StartVote(plugin);
    }
}
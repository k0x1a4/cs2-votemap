using System.Text.Json.Serialization;
using CounterStrikeSharp.API.Core;

namespace VoteMap;

public enum MenuType {
    Html,
    Chat
}

public class VoteMapConfig : BasePluginConfig {
    [JsonPropertyName("RtvPercentage")] public float RtvPercentage { get; set; } = 0.6f;

    [JsonPropertyName("VoteDuration")] public int VoteDuration { get; set; } = 30;

    [JsonPropertyName("MapsInVote")] public int MapsInVote { get; set; } = 5;

    [JsonPropertyName("EnableNominations")]
    public bool EnableNominations { get; set; } = true;

    [JsonPropertyName("MinPlayersForRtv")] public int MinPlayersForRtv { get; set; } = 2;

    [JsonPropertyName("RtvCooldownRounds")]
    public int RtvCooldownRounds { get; set; } = 3;

    [JsonPropertyName("ChangeMapDelay")] public float ChangeMapDelay { get; set; } = 5.0f;

    [JsonPropertyName("MenuType")] public MenuType MenuType { get; set; } = MenuType.Html;

    [JsonPropertyName("EnableEndOfMapVote")]
    public bool EnableEndOfMapVote { get; set; } = true;

    [JsonPropertyName("ChatPrefix")] public string ChatPrefix { get; set; } = "[RTV]";

    [JsonPropertyName("MapList")]
    public List<string> MapList { get; set; } = new() {
        "de_dust2",
        "de_mirage",
        "de_inferno",
        "de_nuke",
        "de_overpass",
        "de_ancient",
        "de_anubis",
        "de_vertigo"
    };
}
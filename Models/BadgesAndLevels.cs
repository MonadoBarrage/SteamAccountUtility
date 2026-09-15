using System.Collections.ObjectModel;
using System.Text.Json.Serialization;
namespace SteamAccountUtility.Models;

public class BadgesAndLevelsData
{
    [JsonPropertyName("response")]
    public BadgesAndLevelsResponse? Response { get; init; }
}

public class BadgesAndLevelsResponse
{
    [JsonPropertyName("badges")] public ObservableCollection<Badge>? Badges { get; init; } = [];

    [JsonPropertyName("player_xp")] public long? PlayerXp { get; init; } = 0;

    [JsonPropertyName("player_level")] public long? PlayerLevel { get; init; } = 0;

    [JsonPropertyName("player_xp_needed_to_level_up")]
    public long? NeededXpForLevelUp { get; init; } = 0;

    [JsonPropertyName("player_xp_needed_current_level")]
    public long? NeededXpForCurrentLevel { get; init; } = 0;

}

public class Badge
{
    [JsonPropertyName("badgeid")]
    public uint? BadgeId { get; init; }
    
    [JsonPropertyName("level")]
    public uint? Level { get; init; }
    
    [JsonPropertyName("completion_time")]
    public long? CompletionTime { get; init; }
    
    [JsonPropertyName("xp")]
    public long? xp { get; init; }
    
    [JsonPropertyName("scarcity")]
    public long? Scarcity { get; init; }
}
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;
namespace SteamAccountUtility.Models;

public class RecentlyPlayedGamesResponse
{
    [JsonPropertyName("response")]
    public RecentlyPlayedGamesData? Response { get; init; }
}

public class RecentlyPlayedGamesData
{
    [JsonPropertyName("total_count")]
    public long TotalCount { get; init; }
    
    [JsonPropertyName("games")]
    public ObservableCollection<GameData>? Games { get; init; }
}
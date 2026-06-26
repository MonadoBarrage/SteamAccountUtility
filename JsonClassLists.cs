using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SteamAccountUtility;

public class SteamGameHTTPRequest
{
    [JsonPropertyName("response")]
    public Response Response { get; set; }
}

public class Response
{
    [JsonPropertyName("game_count")]
    public int GameCount { get; set; }

    [JsonPropertyName("games")]
    public List<Game> Games { get; set; }
}

public class Game
{
    [JsonPropertyName("appid")]
    public int AppId { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("playtime_forever")]
    public int PlaytimeForever { get; set; }

    [JsonPropertyName("img_icon_url")]
    public string ImgIconUrl { get; set; }

    [JsonPropertyName("playtime_windows_forever")]
    public int PlaytimeWindowsForever { get; set; }

    [JsonPropertyName("playtime_mac_forever")]
    public int PlaytimeMacForever { get; set; }

    [JsonPropertyName("playtime_linux_forever")]
    public int PlaytimeLinuxForever { get; set; }

    [JsonPropertyName("playtime_deck_forever")]
    public int PlaytimeDeckForever { get; set; }

    [JsonPropertyName("rtime_last_played")]
    public long RtimeLastPlayed { get; set; }

    [JsonPropertyName("content_descriptorids")]
    public List<int> ContentDescriptorIds { get; set; }

    [JsonPropertyName("playtime_disconnected")]
    public int PlaytimeDisconnected { get; set; }
}

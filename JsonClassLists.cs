using System.Collections.Generic;
using System.Text.Json.Serialization;
using SteamKit2;

namespace SteamAccountUtility;

public class LoginDetails
{
    [JsonPropertyName("username")]
    public string Username { get; set; }
    
    [JsonPropertyName("password")]
    public string Password { get; set; }
    
    [JsonPropertyName("steamKey")]
    public string SteamKey { get; set; }
    
    [JsonPropertyName("guardData")]
    public string GuardData { get; set; }
    
    [JsonPropertyName("accessToken")]
    public string AccessToken { get; set; }
}

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

public class UserData
{
    public SteamID SteamID;
    public string ProfileName;
    public byte[]? AvatarHash;
    public string LastPlayed;
    
    public bool validateData()
    {
        if (!SteamID.Equals(new SteamID()) && 
            !string.IsNullOrEmpty(ProfileName)
           ) return true;
        return false;
    }

}

public class FriendData
{
    public SteamID SteamID;
    public string ProfileName;
    public byte[]? AvatarHash;

    public bool validateData()
    {
        if (!SteamID.Equals(new SteamID()) && 
            !string.IsNullOrEmpty(ProfileName)
            ) return true;
        return false;
    }
}
public class RefreshTokenJson
{
    
    [JsonPropertyName("iss")]
    public string iss { get; set; }
    
    [JsonPropertyName("sub")]
    public string sub { get; set; }

    [JsonPropertyName("aud")]
    public List<string> aud { get; set; }
    
    [JsonPropertyName("exp")]
    public long exp { get; set; }

    [JsonPropertyName("nbf")]
    public long nbf { get; set; }
    
    [JsonPropertyName("iat")]
    public long iat { get; set; }

    [JsonPropertyName("jti")]
    public string jti { get; set; }
    
    [JsonPropertyName("oat")]
    public long oat { get; set; }
    
    [JsonPropertyName("per")]
    public long per { get; set; }
    
    [JsonPropertyName("ip_subject")]
    public string ip_subject { get; set; }
    
    [JsonPropertyName("ip_confirmer")]
    public string ip_confirmer { get; set; }

}



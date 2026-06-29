using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;
using Avalonia.Media.Imaging;
using SteamAccountUtility.ViewModels;
using SteamKit2;

namespace SteamAccountUtility;

public class LoginDetails
{
    [JsonPropertyName("username")]
    public string? Username { get; init; }
    
    [JsonPropertyName("password")]
    public string? Password { get; init; }
    
    [JsonPropertyName("steamKey")]
    public string? SteamKey { get; init; }
    
    [JsonPropertyName("guardData")]
    public string? GuardData { get; init; }
    
    [JsonPropertyName("accessToken")]
    public string? AccessToken { get; init; }
}

public class SteamGameHttpRequest
{
    [JsonPropertyName("response")]
    public required Response Response { get; init; }
}

public class Response
{
    [JsonPropertyName("game_count")]
    public int GameCount { get; set; }

    [JsonPropertyName("games")]
    public required List<GameData> Games { get; set; }
}

public class GameData
{
    public GameData()
    {
        AppId = -1;
        Name = string.Empty;
        ImgIconUrl = string.Empty;
        PlaytimeForever = -1;
        PlaytimeWindowsForever = -1;
        PlaytimeMacForever = -1;
        PlaytimeLinuxForever = -1;
        PlaytimeDeckForever = -1;
        RtimeLastPlayed = -1;
        ContentDescriptorIds = [];
        PlaytimeDeckForever = -1;
    }
    
    [JsonPropertyName("appid")]
    public int AppId { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("img_icon_url")]
    public string ImgIconUrl { get; set; }
    
    [JsonPropertyName("playtime_forever")]
    public int PlaytimeForever { get; set; }
    
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

    public Bitmap? AppIcon { get; set; }
    
    public void PrintAll()
    {
        Console.WriteLine("appid: {0}",AppId);
        Console.WriteLine("name: {0}",Name);
        Console.WriteLine("img_icon_url: {0}",ImgIconUrl);
        Console.WriteLine("playtime_forever: {0}",PlaytimeForever);
        Console.WriteLine("playtime_windows_forever: {0}",PlaytimeWindowsForever);
        Console.WriteLine("playtime_mac_forever: {0}",PlaytimeMacForever);
        Console.WriteLine("playtime_linux_forever: {0}",PlaytimeLinuxForever);
        Console.WriteLine("playtime_deck_forever: {0}",PlaytimeDeckForever);
        Console.WriteLine("rtime_last_played: {0}",RtimeLastPlayed);
        Console.WriteLine("content_descriptorids: {0}",ContentDescriptorIds.Count);
        foreach (var item in ContentDescriptorIds)
        {
            Console.WriteLine(item);
        }
        Console.WriteLine("playtime_disconnected: {0}",PlaytimeDisconnected);

        
    }
}

public class UserData
{
    public SteamID? SteamID { get; set; }
    public string? ProfileName { get; set; }
    public byte[]? AvatarHash { get; set; }
    
    public Bitmap? AvatarIcon { get; set; }
    public string? LastPlayed { get; set; }
    
    public bool ValidateData()
    {
        return SteamID != null 
               && !SteamID.Equals(new SteamID()) 
               && !string.IsNullOrEmpty(ProfileName);
    }

    public void PrintAll()
    {
        Console.WriteLine("SteamID: {0}",SteamID);
        Console.WriteLine("ProfileName: {0}",ProfileName);
        Console.WriteLine("AvatarHash: {0}",AvatarHash);
        Console.WriteLine("LastPlayed: {0}",LastPlayed);
        
    }

}

public class FriendData
{
    public required SteamID SteamID { get; set; }
    public required string ProfileName { get; set; }
    public byte[]? AvatarHash { get; set; }
    
    public Bitmap? AvatarIcon { get; set; }

    public bool ValidateData()
    {
        if (!SteamID.Equals(new SteamID()) && 
            !string.IsNullOrEmpty(ProfileName)
            ) return true;
        return false;
    }
    public void PrintAll()
    {
        Console.WriteLine("SteamID: {0}",SteamID);
        Console.WriteLine("ProfileName: {0}",ProfileName);
        Console.WriteLine("AvatarHash: {0}",AvatarHash);
    }
}

public class RefreshTokenJson
{
    
    [JsonPropertyName("iss")]
    public string? Iss { get; init; }
    
    [JsonPropertyName("sub")]
    public string? Sub { get; init; }

    [JsonPropertyName("aud")]
    public List<string>? Aud { get; init; }
    
    [JsonPropertyName("exp")]
    public long? Expiration { get; init; }

    [JsonPropertyName("nbf")]
    public long? Nbf { get; init; }
    
    [JsonPropertyName("iat")]
    public long? Iat { get; init; }

    [JsonPropertyName("jti")]
    public string? Jti { get; init; }
    
    [JsonPropertyName("oat")]
    public long? Oat { get; init; }
    
    [JsonPropertyName("per")]
    public long? Per { get; init; }
    
    [JsonPropertyName("ip_subject")]
    public string? IpSubject { get; init; }
    
    [JsonPropertyName("ip_confirmer")]
    public string? IpConfirmer { get; init; }

}

public class AllSteamData
{
    public  required UserData CurrentUser;
    public  required ObservableCollection<FriendData> Friends;
    public  required ObservableCollection<GameData> Games;
    public  required ObservableCollection<AuxiliaryFriendViewModel> FriendVM;
    public  required ObservableCollection<AuxiliaryGameViewModel> GameVM;
}


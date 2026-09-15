using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;
using Avalonia.Media.Imaging;
using SteamAccountUtility.ViewModels.AuxiliaryViewModels;

namespace SteamAccountUtility.Models;

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
    
    [JsonPropertyName("playtime_2weeks")]
    public int Playtime2Weeks { get; set; }
    
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
    
    public string? AppURI { get; set; }
    public Bitmap? AppIcon { get; set; }
    
    public string? CapsuleURI  { get; set; }
    public Bitmap? CapsuleIcon { get; set; }
    
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
        Console.WriteLine("AppURI: {0}", AppURI);
        
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
    public required UserData CurrentUser;
    public required Dictionary<int, RenderedSteamGame> Games;
    public required ObservableCollection<FriendData> Friends;
    public required BadgesAndLevelsResponse BadgesAndLevels;
    public required ObservableCollection<RenderedSteamGame> RecentGames;
    
    public required ObservableCollection<AuxiliaryFriendViewModel>? FriendVm;
    public required ObservableCollection<AuxiliaryGameViewModel>? GameVm;
    
    
}



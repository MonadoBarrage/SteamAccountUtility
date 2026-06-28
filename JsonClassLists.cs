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
    public List<GameData> Games { get; set; }
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
        ContentDescriptorIds = new List<int>();
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
    
    public void printAll()
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
    public SteamID SteamID { get; set; }
    public string ProfileName { get; set; }
    public byte[]? AvatarHash { get; set; }
    
    public Bitmap? AvatarIcon { get; set; }
    public string LastPlayed { get; set; }
    
    public bool validateData()
    {
        if (!SteamID.Equals(new SteamID()) && 
            !string.IsNullOrEmpty(ProfileName)
           ) return true;
        return false;
    }

    public void printAll()
    {
        Console.WriteLine("SteamID: {0}",SteamID);
        Console.WriteLine("ProfileName: {0}",ProfileName);
        Console.WriteLine("AvatarHash: {0}",AvatarHash);
        Console.WriteLine("LastPlayed: {0}",LastPlayed);
        
    }

}

public class FriendData
{
    public SteamID SteamID { get; set; }
    public string ProfileName { get; set; }
    public byte[]? AvatarHash { get; set; }
    
    public Bitmap? AvatarIcon { get; set; }

    public bool validateData()
    {
        if (!SteamID.Equals(new SteamID()) && 
            !string.IsNullOrEmpty(ProfileName)
            ) return true;
        return false;
    }
    public void printAll()
    {
        Console.WriteLine("SteamID: {0}",SteamID);
        Console.WriteLine("ProfileName: {0}",ProfileName);
        Console.WriteLine("AvatarHash: {0}",AvatarHash);
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

public class AllSteamData
{
    public UserData currentUser;
    public ObservableCollection<FriendData> friends;
    public ObservableCollection<GameData> games;
    public ObservableCollection<AuxiliaryFriendViewModel> friendVM;
    public ObservableCollection<AuxiliaryGameViewModel> gameVM;
}


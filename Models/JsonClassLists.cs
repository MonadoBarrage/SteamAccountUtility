using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;
using Avalonia.Media.Imaging;
using SteamAccountUtility.ViewModels;
using SteamKit2;

namespace SteamAccountUtility;

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

public class UserData
{
    public SteamID? SteamID { get; set; }
    public string? ProfileName { get; set; }
    public byte[]? AvatarHash { get; set; }
    
    public string? AvatarURI { get; set; }
    
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
        Console.WriteLine("AvatarURI: {0}",AvatarURI);
        Console.WriteLine("LastPlayed: {0}",LastPlayed);
        
    }

}

public class FriendData
{
    public required SteamID SteamID { get; set; }
    public required string ProfileName { get; set; }
    public byte[]? AvatarHash { get; set; }
    
    public string? AvatarURI { get; set; }
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
        Console.WriteLine("AvatarURI: {0}",AvatarURI);
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
    public required UserData CurrentUser;
    public required Dictionary<int, RenderedSteamGame> Games;
    public required ObservableCollection<FriendData> Friends;
    public required BadgesAndLevelsResponse BadgesAndLevels;
    public required ObservableCollection<RenderedSteamGame> RecentGames;
    
    public required ObservableCollection<AuxiliaryFriendViewModel>? FriendVm;
    public required ObservableCollection<AuxiliaryGameViewModel>? GameVm;
    
    
}

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

public class SteamStoreItemsResponse
{
    [JsonPropertyName("response")]
    public SteamStoreItems StoreItems { get; set; }
}

public class SteamStoreItems
{
    [JsonPropertyName("store_items")]
    public SteamStoreGame[] StoreGames { get; set; }
}

public class SteamStoreGame
{
    [JsonPropertyName("id")] public int Id{ get; set; }
    [JsonPropertyName("appid")] public int AppId{ get; set; }
    [JsonPropertyName("name")] public string? Name{ get; set; }
    [JsonPropertyName("assets")] public SteamStoreItemAssets? ItemAssets{ get; set; }
}

public class SteamStoreItemAssets
{
    [JsonPropertyName("asset_url_format")] public string? AssetUrlFormat{ get; set; }
    [JsonPropertyName("small_capsule")] public string? SmallCapsule{ get; set; }
    [JsonPropertyName("library_capsule")] public string? LibraryCapsule{ get; set; }
    [JsonPropertyName("library_capsule_2x")] public string? LibraryCapsule2x{ get; set; }
    [JsonPropertyName("header")] public string? Header{ get; set; }
    [JsonPropertyName("library_hero")] public string? LibraryHero{ get; set; }
    [JsonPropertyName("community_icon")] public string? CommunityIcon{ get; set; }
    [JsonPropertyName("last_modified")] public long? LastModified{ get; set; }
}


public class RenderedSteamGame
{
    [JsonPropertyName("appid")] public int Appid{ get; set; }
    [JsonPropertyName("name")] public string? Name{ get; set; }
    [JsonPropertyName("library_image")] public Bitmap? LibraryImage{ get; set; }
    [JsonPropertyName("header")] public Bitmap? Header{ get; set; }
}

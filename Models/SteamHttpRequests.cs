using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.Messaging;
using SteamAccountUtility.Messages;
using SteamKit2;

namespace SteamAccountUtility.Models;

using System.Net.Http;
using System.Text.Json;



public class SteamHttpRequests
{
    private readonly HttpClient _httpClient = new HttpClient();

    
    private SteamInputJson steamInput = new SteamInputJson()
    {
        Ids = [],
        Context = new SteamContext()
        {
            Language = "english",
            CountryCode = "us"
        },
        DataRequest = new SteamDataRequest()
        {
            IncludeAssets =  true,
        }
    };

    public async Task<ObservableCollection<AppRenderedSteamGame>?> FetchUserGameLibrary(string steamKey, SteamID userId)
    {
        string requestLink = "https://api.steampowered.com/IPlayerService/GetOwnedGames/v1/";
        requestLink += "?key=" + steamKey;
        requestLink += "&steamid=" + userId.ConvertToUInt64();
        requestLink += "&include_appinfo=1";
        
        using var response = await _httpClient.GetAsync(requestLink);
        response.EnsureSuccessStatusCode();
        var jsonResponse = await response.Content.ReadAsStringAsync();

        var obj = JsonSerializer.Deserialize<SteamGameHttpRequest>(jsonResponse);
        if (obj == null)
        {
            return null;
        }
        var fetchedGames = obj.Response.Games;
        var gameLibrary = await FetchGameData(steamKey, fetchedGames);
        return gameLibrary;
        // fetchedGames.ForEach(game => Console.WriteLine($"{game.AppId}: {game.Name}"));

    }
    
    private async Task<ObservableCollection<AppRenderedSteamGame>?> FetchGameData(string steamKey, List<GameData> games)
    {
        var requestLink = "https://api.steampowered.com/IStoreBrowseService/GetItems/v1/";
        steamInput.Ids = games.Select(game => new SteamGameIds(){AppId = game.AppId}).ToArray();
        var json = JsonSerializer.Serialize(steamInput);
        
        requestLink += "?key=" + steamKey;
        requestLink += "&input_json=" + json;
        // Console.WriteLine(requestLink);
        
        using var response = await _httpClient.GetAsync(requestLink);
        response.EnsureSuccessStatusCode();
        
        var jsonResponse = await response.Content.ReadAsStringAsync();
        
        var storeItemsResponse = JsonSerializer.Deserialize<SteamStoreItemsResponse>(jsonResponse);

        if (storeItemsResponse == null) return null;
        // List<Task> gamesToFetchImages = new List<Task>();
        List<AppRenderedSteamGame> steamGames = new List<AppRenderedSteamGame>();
        foreach (var game in storeItemsResponse.StoreItems.StoreGames)
        {
            var (libraryCapsule, header) = await FetchGameImages(game);
            steamGames.Add(new AppRenderedSteamGame()
            {
                Appid = game.Id,
                Name =  game.Name,
                LibraryImage = libraryCapsule,
                Header = header
            });
        }
        steamGames.Sort((x, y) => x.Name.CompareTo(y.Name, StringComparison.OrdinalIgnoreCase));
        
        return new ObservableCollection<AppRenderedSteamGame>(steamGames);
    }

    private void CheckForAssetUpdates()
    {
        
    }

    private async Task<(Bitmap?,Bitmap?)> FetchGameImages(SteamStoreGame steamStoreGame)
    {
        if (steamStoreGame.ItemAssets == null
            || steamStoreGame.ItemAssets.AssetUrlFormat == null 
            || steamStoreGame.ItemAssets.Header == null 
            || steamStoreGame.ItemAssets.LibraryCapsule == null)
        {
            return (null, null);
        }
        
        var requestLink = "https://shared.fastly.steamstatic.com/store_item_assets/" 
            + steamStoreGame.ItemAssets.AssetUrlFormat;
        
        
        
        var libraryImage = requestLink.Replace("${FILENAME}",steamStoreGame.ItemAssets.LibraryCapsule); 
        var headerImage = requestLink.Replace("${FILENAME}",steamStoreGame.ItemAssets.Header);

        var libraryBitmap = await CreateBitmapImage(libraryImage);
        var headerBitmap = await CreateBitmapImage(headerImage);
        
        return (libraryBitmap, headerBitmap);
    }

    public async Task<BadgeResponse?> FetchUserBadgesAndLevels(string steamKey, SteamID userId)
    {
        var requestLink = "https://api.steampowered.com/IPlayerService/GetBadges/v1/";
        requestLink += "?key=" + steamKey;
        requestLink += "&steamid=" + userId.ConvertToUInt64();
        using var response = await _httpClient.GetAsync(requestLink);

        response.EnsureSuccessStatusCode();

        var jsonResponse = await response.Content.ReadAsStringAsync();

        var badgesAndLevels = JsonSerializer.Deserialize<BadgeResponse>(jsonResponse);
        return badgesAndLevels;
    }

    public async Task<RecentlyPlayedGamesResponse?> FetchRecentlyPlayedGames(string steamKey, SteamID userId)
    {
        var requestLink = "https://api.steampowered.com/IPlayerService/GetRecentlyPlayedGames/v1/?count=3";
        requestLink += "&key=" + steamKey; 
        requestLink += "&steamid=" + userId.ConvertToUInt64();
        using var response = await _httpClient.GetAsync(requestLink);
        
        response.EnsureSuccessStatusCode();

        var jsonResponse = await response.Content.ReadAsStringAsync();
        var recentlyPlayedGames = JsonSerializer.Deserialize<RecentlyPlayedGamesResponse>(jsonResponse);
        
        return recentlyPlayedGames;
    }
    
    public async Task<Bitmap?> FetchUserAvatar(byte[]? avatarHash)
    {
        var requestLink = "https://avatars.fastly.steamstatic.com/";
        requestLink += ConvertByteArrayToString(avatarHash);
        requestLink += "_full.jpg";
        
        var response = await CreateBitmapImage(requestLink);
        return response;

    }

    public async Task<Bitmap?> CreateBitmapImage(string requestLink)
    {
        
        try
        {
            var bytes = await _httpClient.GetByteArrayAsync(requestLink);
            using var stream = new MemoryStream(bytes);
            return new Bitmap(stream);
        }
        catch (Exception)
        {
            return null;
        }

    }
    
    private static string ConvertByteArrayToString(byte[]? hash)
    {
        return (hash == null ? "" : BitConverter.ToString(hash).Replace("-", "").ToLower());
    }
}
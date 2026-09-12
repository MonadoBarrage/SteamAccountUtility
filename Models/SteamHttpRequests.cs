using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using SteamKit2;
namespace SteamAccountUtility.Models;

using System.Net.Http;
using System.Text.Json;

public class SteamHttpRequests(string address)
{
    private readonly HttpClient _httpClient = new();

    private readonly string _serverUrl = "http://" + address;
    public async Task<Dictionary<int, AppRenderedSteamGame>?> FetchUserGameLibrary(SteamID userId)
    {
        var requestLink = _serverUrl + "/owned-games?id=" + userId.ConvertToUInt64();
        using var response = await _httpClient.GetAsync(requestLink);
        response.EnsureSuccessStatusCode();
        
        var jsonResponse = await response.Content.ReadAsStringAsync();
        var storeItemsResponse = JsonSerializer.Deserialize<SteamStoreItemsResponse>(jsonResponse);
        Dictionary<int, AppRenderedSteamGame> steamGames = new Dictionary<int, AppRenderedSteamGame>();
        if(storeItemsResponse != null)
            foreach (var game in storeItemsResponse.StoreItems.StoreGames)
            {
                var (libraryCapsule, header) = await FetchGameImages(game);
                steamGames.Add(game.Id, new AppRenderedSteamGame()
                {
                    Appid = game.Id,
                    Name =  game.Name,
                    LibraryImage = libraryCapsule,
                    Header = header
                });
            }
        
        return steamGames;

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

    public async Task<BadgeResponse?> FetchUserBadgesAndLevels(SteamID userId)
    {
        
        
        var requestLink = _serverUrl + "/badges-and-levels?id=" + userId.ConvertToUInt64();
        using var response = await _httpClient.GetAsync(requestLink);
        response.EnsureSuccessStatusCode();
        var jsonResponse = await response.Content.ReadAsStringAsync();
        var badgesAndLevels = JsonSerializer.Deserialize<BadgeResponse>(jsonResponse);
        
        return badgesAndLevels;
    }

    public async Task<RecentlyPlayedGamesResponse?> FetchRecentlyPlayedGames(SteamID userId)
    {
        var requestLink = _serverUrl + "/recently-played-games?id=" + userId.ConvertToUInt64();
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

    private async Task<Bitmap?> CreateBitmapImage(string requestLink)
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
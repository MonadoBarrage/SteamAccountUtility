using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using SteamAccountUtility.Models;
using SteamKit2;
namespace SteamAccountUtility.Services;

using System.Net.Http;
using System.Text.Json;

public class SteamHttpRequests(string address)
{
    private readonly HttpClient _httpClient = new();

    private readonly string _serverUrl = "http://" + address;
    
    
    public async Task<Dictionary<int, RenderedSteamGame>?> FetchUserGameLibrary(SteamID userId)
    {
        try
        {
            var requestLink = _serverUrl + "/owned-games?id=" + userId.ConvertToUInt64();
            using var response = await _httpClient.GetAsync(requestLink);
            response.EnsureSuccessStatusCode();

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var storeItemsResponse = JsonSerializer.Deserialize<SteamStoreItemsResponse>(jsonResponse);
            var steamGames = new Dictionary<int, RenderedSteamGame>();
            if (storeItemsResponse == null)
                return [];

            foreach (var game in storeItemsResponse.StoreItems.StoreGames)
            {
                var (libraryCapsule, header) = await FetchGameImages(game);
                steamGames.Add(game.Id, new RenderedSteamGame()
                {
                    Appid = game.Id,
                    Name = game.Name,
                    LibraryImage = libraryCapsule,
                    Header = header
                });
            }
            return steamGames;
        }
        catch (Exception e)
        {
            await Console.Error.WriteLineAsync(e.Message);
            return null;
        }
    }

    private async Task<(Bitmap?, Bitmap?)> FetchGameImages(SteamStoreGame steamStoreGame)
    {
        try
        {
            if (steamStoreGame.ItemAssets?.AssetUrlFormat == null
                || steamStoreGame.ItemAssets.Header == null
                || steamStoreGame.ItemAssets.LibraryCapsule == null)
            {
                return (null, null);
            }

            var requestLink = "https://shared.fastly.steamstatic.com/store_item_assets/"
                              + steamStoreGame.ItemAssets.AssetUrlFormat;


            var libraryImage = requestLink.Replace("${FILENAME}", steamStoreGame.ItemAssets.LibraryCapsule);
            var headerImage = requestLink.Replace("${FILENAME}", steamStoreGame.ItemAssets.Header);
            var libraryBitmap = await CreateBitmapImage(libraryImage);
            var headerBitmap = await CreateBitmapImage(headerImage);

            return (libraryBitmap, headerBitmap);
        }
        catch (Exception e)
        {
            await Console.Error.WriteLineAsync(e.Message);
            return (null,null);
        }
    }

    public async Task<BadgesAndLevelsResponse?> FetchUserBadgesAndLevels(SteamID userId)
    {
        
        try {
            var requestLink = _serverUrl + "/badges-and-levels?id=" + userId.ConvertToUInt64();
            using var response = await _httpClient.GetAsync(requestLink);
            response.EnsureSuccessStatusCode();
            var jsonResponse = await response.Content.ReadAsStringAsync();
            var badgesAndLevels  = JsonSerializer.Deserialize<BadgesAndLevelsData>(jsonResponse);
            
            return badgesAndLevels?.Response ?? new BadgesAndLevelsResponse();
        }
        catch (Exception e)
        {
            await Console.Error.WriteLineAsync(e.Message);
            return null;
        }
    }

    public async Task<RecentlyPlayedGamesResponse?> FetchRecentlyPlayedGames(SteamID userId)
    {
        try
        {
            var requestLink = _serverUrl + "/recently-played-games?id=" + userId.ConvertToUInt64();
            using var response = await _httpClient.GetAsync(requestLink);
            response.EnsureSuccessStatusCode();
            var jsonResponse = await response.Content.ReadAsStringAsync();
            var recentlyPlayedGames = JsonSerializer.Deserialize<RecentlyPlayedGamesResponse>(jsonResponse);

            return recentlyPlayedGames;
        }
        catch (Exception e)
        {
            await Console.Error.WriteLineAsync(e.Message);
            return null;
        }
    }
    
    public async Task<Bitmap?> FetchUserAvatar(byte[]? avatarHash)
    {
        try
        {
            var requestLink = "https://avatars.fastly.steamstatic.com/";
            requestLink += ConvertByteArrayToString(avatarHash);
            requestLink += "_full.jpg";

            var response = await CreateBitmapImage(requestLink);
            return response;
        }
        catch(Exception e)
        {
            await Console.Error.WriteLineAsync(e.Message);
            return null;
        }
    }

    private async Task<Bitmap?> CreateBitmapImage(string requestLink)
    {
        
        try
        {
            var bytes = await _httpClient.GetByteArrayAsync(requestLink);
            using var stream = new MemoryStream(bytes);
            return new Bitmap(stream);
        }
        catch (Exception e)
        {
            await Console.Error.WriteLineAsync(e.Message);
            return null;
        }

    }
    
    private static string ConvertByteArrayToString(byte[]? hash)
    {
        return hash == null ? "" : BitConverter.ToString(hash).Replace("-", "").ToLower();
    }
    

}
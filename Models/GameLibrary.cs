using System.Collections.ObjectModel;
using System.Text.Json.Serialization;
using Avalonia.Media.Imaging;

namespace SteamAccountUtility.Models;

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
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("appid")]
    public int AppId { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("assets")]
    public SteamStoreItemAssets? ItemAssets { get; set; }
}

public class SteamStoreItemAssets
{
    [JsonPropertyName("asset_url_format")]
    public string? AssetUrlFormat { get; set; }

    [JsonPropertyName("small_capsule")]
    public string? SmallCapsule { get; set; }

    [JsonPropertyName("library_capsule")]
    public string? LibraryCapsule { get; set; }

    [JsonPropertyName("library_capsule_2x")]
    public string? LibraryCapsule2x { get; set; }

    [JsonPropertyName("header")]
    public string? Header { get; set; }

    [JsonPropertyName("library_hero")]
    public string? LibraryHero { get; set; }

    [JsonPropertyName("community_icon")]
    public string? CommunityIcon { get; set; }

    [JsonPropertyName("last_modified")]
    public long? LastModified { get; set; }
}

public class RenderedSteamGame
{
    [JsonPropertyName("appid")]
    public int Appid { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("library_image")]
    public Bitmap? LibraryImage { get; set; }

    [JsonPropertyName("header")]
    public Bitmap? Header { get; set; }
}

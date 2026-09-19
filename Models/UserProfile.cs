using System;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;
using Avalonia.Media.Imaging;
using SteamKit2;

namespace SteamAccountUtility.Models;

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
        Console.WriteLine("SteamID: {0}", SteamID);
        Console.WriteLine("ProfileName: {0}", ProfileName);
        Console.WriteLine("AvatarHash: {0}", AvatarHash);
        Console.WriteLine("AvatarURI: {0}", AvatarURI);
        Console.WriteLine("LastPlayed: {0}", LastPlayed);
    }
}

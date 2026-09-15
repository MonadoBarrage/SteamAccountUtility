using System;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;
using Avalonia.Media.Imaging;
using SteamKit2;

namespace SteamAccountUtility.Models;

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
using System.Collections.Concurrent;
using System.Collections.Generic;
using SteamKit2;

namespace SteamAccountUtility.Messages;

public class GoToHomePage(AllSteamData steamData)
{
    public readonly AllSteamData UserSteamData = steamData;
}
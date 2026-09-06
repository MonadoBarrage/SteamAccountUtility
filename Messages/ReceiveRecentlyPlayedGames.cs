using System.Collections.Concurrent;
using System.Collections.Generic;

namespace SteamAccountUtility.Messages;

public class ReceiveRecentlyPlayedGames(List<int> g)
{
    public readonly List<int> RecentlyPlayedGames = g;
}
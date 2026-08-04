using System.Collections.Concurrent;
using System.Collections.Generic;

namespace SteamAccountUtility.Messages;

public class ReceiveRecentlyPlayedGames(ConcurrentBag<GameData>? g)
{
    public readonly ConcurrentBag<GameData>? RecentlyPlayedGames = g;
}
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace SteamAccountUtility.Messages;

public class ReceiveRecentlyPlayedGames(List<int> recentGames, bool fetchedSuccessfully)
{
    public readonly List<int> RecentlyPlayedGames = recentGames;
    public readonly bool FetchedSuccessfully = fetchedSuccessfully;
}
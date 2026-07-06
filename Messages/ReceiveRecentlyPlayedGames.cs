using System.Collections.Generic;

namespace SteamAccountUtility.Messages;

public class ReceiveRecentlyPlayedGames(RecentlyPlayedGamesResponse? g)
{
    public readonly RecentlyPlayedGamesResponse? RecentlyPlayedGames = g;
}
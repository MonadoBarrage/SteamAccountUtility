using System.Collections.Generic;
using SteamAccountUtility.Models;

namespace SteamAccountUtility.Messages;

public class ReceiveGameList(Dictionary<int, RenderedSteamGame> gamesList, bool fetchedSuccessfully)
{
    public readonly Dictionary<int, RenderedSteamGame> NewGameList = gamesList;
    public readonly bool FetchedSuccessfully = fetchedSuccessfully;

}
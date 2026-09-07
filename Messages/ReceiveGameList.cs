using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SteamAccountUtility.Messages;

public class ReceiveGameList(Dictionary<int, AppRenderedSteamGame>? g)
{
    public readonly Dictionary<int, AppRenderedSteamGame>? NewGameList = g;

}
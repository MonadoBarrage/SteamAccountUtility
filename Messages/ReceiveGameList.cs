using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SteamAccountUtility.Messages;

public class ReceiveGameList(ObservableCollection<AppRenderedSteamGame>? g)
{
    public readonly ObservableCollection<AppRenderedSteamGame>? NewGameList = g;

}
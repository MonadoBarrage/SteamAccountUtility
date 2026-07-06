using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SteamAccountUtility.Messages;

public class ReceiveGameList(ObservableCollection<GameData>? g)
{
    public readonly ObservableCollection<GameData>? NewGameList = g;

}
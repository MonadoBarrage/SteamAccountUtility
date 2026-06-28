using System.Collections.Generic;
namespace SteamAccountUtility.Messages;

public class ReceiveGameList(List<GameData> g)
{
    public readonly List<GameData> NewGameList = g;

}
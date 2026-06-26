using System.Collections.Generic;
using SteamAccountUtility;
namespace SteamAccountUtility.Messages;

public class ReceiveGameList(List<Game> g)
{
    public readonly List<Game> NewGameList = g;

}
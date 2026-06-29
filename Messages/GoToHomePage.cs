using System.Collections.Concurrent;
using System.Collections.Generic;
using SteamKit2;

namespace SteamAccountUtility.Messages;

public class GoToHomePage(bool checkIfUserCanEnter, UserData userData, List<GameData> userGameList, ConcurrentDictionary<SteamID, FriendData> userFriendList )
{
    public readonly bool CheckIfUserCanEnter = checkIfUserCanEnter;
    public readonly UserData User = userData;
    public readonly List<GameData> UserGameList = userGameList;
    public readonly ConcurrentDictionary<SteamID, FriendData> UserFriendList = userFriendList;
}
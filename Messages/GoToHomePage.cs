using System.Collections.Generic;
using CommunityToolkit.Mvvm.Messaging.Messages;
using SteamAccountUtility.ViewModels;
using SteamKit2;

namespace SteamAccountUtility.Messages;

public class GoToHomePage(bool checkIfUserCanEnter, UserData userData, List<Game> userGameList, Dictionary<SteamID, FriendData> userFriendList )
{
    public readonly bool CheckIfUserCanEnter = checkIfUserCanEnter;
    public readonly UserData User = userData;
    public readonly List<Game> UserGameList = userGameList;
    public readonly Dictionary<SteamID, FriendData> UserFriendList = userFriendList;
}
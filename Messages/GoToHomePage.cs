using System.Collections.Generic;
using CommunityToolkit.Mvvm.Messaging.Messages;
using SteamAccountUtility.ViewModels;

namespace SteamAccountUtility.Messages;

public class GoToHomePage(bool checkIfUserCanEnter, string profileName, List<Game> userGameList, Dictionary<string, string> userFriendList )
{
    public readonly bool CheckIfUserCanEnter = checkIfUserCanEnter;
    public readonly string ProfileName = profileName;
    public readonly List<Game> UserGameList = userGameList;
    public readonly Dictionary<string, string> UserFriendList = userFriendList;
}
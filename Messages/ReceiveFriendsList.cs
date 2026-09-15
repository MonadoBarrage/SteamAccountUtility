using System.Collections.ObjectModel;
using SteamAccountUtility.Models;

namespace SteamAccountUtility.Messages;

public class ReceiveFriendsList(ObservableCollection<FriendData> friendsList, bool fetchedSuccessfully)
{
    public readonly ObservableCollection<FriendData> NewFriendsList = friendsList;
    public readonly bool FetchedSuccessfully  = fetchedSuccessfully;
}
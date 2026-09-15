using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using SteamKit2;

namespace SteamAccountUtility.Messages;

public class ReceiveFriendsList(ObservableCollection<FriendData> friendsList, bool fetchedSuccessfully)
{
    public readonly ObservableCollection<FriendData> NewFriendsList = friendsList;
    public readonly bool FetchedSuccessfully  = fetchedSuccessfully;
}
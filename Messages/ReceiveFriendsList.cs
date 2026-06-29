using System.Collections.Concurrent;
using SteamKit2;

namespace SteamAccountUtility.Messages;

public class ReceiveFriendsList(ConcurrentDictionary<SteamID, FriendData> fl)
{
    public readonly ConcurrentDictionary<SteamID, FriendData> NewFriendsList = fl;
}
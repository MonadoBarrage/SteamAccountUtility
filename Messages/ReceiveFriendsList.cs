using System.Collections.Generic;
using SteamKit2;

namespace SteamAccountUtility.Messages;

public class ReceiveFriendsList(Dictionary<SteamID, FriendData> fl)
{
    public readonly Dictionary<SteamID, FriendData> NewFriendsList = fl;
}
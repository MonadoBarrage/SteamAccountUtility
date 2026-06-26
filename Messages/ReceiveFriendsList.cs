using System.Collections.Generic;

namespace SteamAccountUtility.Messages;

public class ReceiveFriendsList(Dictionary<string, string> fl)
{
    public readonly Dictionary<string, string> NewFriendsList = fl;
}
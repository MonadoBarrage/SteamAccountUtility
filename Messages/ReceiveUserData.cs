namespace SteamAccountUtility.Messages;
using SteamAccountUtility.Models;

public class ReceiveUserData(UserData ud, bool fetchedSuccessfully)
{
    public readonly UserData User = ud;
    public readonly bool FetchedSuccessfully = fetchedSuccessfully;
}
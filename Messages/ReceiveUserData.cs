namespace SteamAccountUtility.Messages;

public class ReceiveUserData(UserData ud, bool fetchedSuccessfully)
{
    public readonly UserData User = ud;
    public readonly bool FetchedSuccessfully = fetchedSuccessfully;
}
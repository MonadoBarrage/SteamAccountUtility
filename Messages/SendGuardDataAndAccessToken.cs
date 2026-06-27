namespace SteamAccountUtility.Messages;

public class SendGuardDataAndAccessToken(string gd, string at)
{
    public readonly string GuardData = gd;
    public readonly string AccessToken = at;
}
namespace SteamAccountUtility.Messages;

public class GoToHomePageMessage(AllSteamData steamData)
{
    public readonly AllSteamData UserSteamData = steamData;
}
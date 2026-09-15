namespace SteamAccountUtility.Messages;

public class ReceiveBadgesAndLevels(BadgesAndLevelsResponse response, bool fetchedSuccessfully)
{
    public readonly BadgesAndLevelsResponse FetchedBadges = response;
    public readonly bool FetchedSuccessfully = fetchedSuccessfully;
}
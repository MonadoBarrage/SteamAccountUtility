namespace SteamAccountUtility.Messages;

public class ReceiveBadgesAndLevels(BadgeResponse? response)
{
    public readonly BadgesAndLevelsData? FetchedBadges = response.Response;
}
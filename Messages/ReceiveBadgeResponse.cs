namespace SteamAccountUtility.Messages;

public class ReceiveBadgeResponse(BadgeResponse? response)
{
    public readonly BadgeResponse? FetchedBadges = response;
}
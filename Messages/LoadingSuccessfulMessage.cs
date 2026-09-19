using SteamAccountUtility.Models;

namespace SteamAccountUtility.Messages;

public class LoadingSuccessfulMessage(AllSteamData? asd)
{
    public readonly AllSteamData? steamData = asd;
}

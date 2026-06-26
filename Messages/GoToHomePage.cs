using CommunityToolkit.Mvvm.Messaging.Messages;
using SteamAccountUtility.ViewModels;

namespace SteamAccountUtility.Messages;

public class GoToHomePage(bool checkIfUserCanEnter)
{
    public readonly bool IsValid = checkIfUserCanEnter;
}
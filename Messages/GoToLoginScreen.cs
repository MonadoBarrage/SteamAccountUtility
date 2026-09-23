namespace SteamAccountUtility.Messages;

public class GoToLoginScreen(string? errorMessage = null)
{
    public string? ErrorMessage = errorMessage;
}

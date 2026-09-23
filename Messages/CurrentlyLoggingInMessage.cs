using SteamAccountUtility.Models;

namespace SteamAccountUtility.Messages;

public class CurrentlyLoggingInMessage(string? message = null)
{
    public string? Message = message;
}

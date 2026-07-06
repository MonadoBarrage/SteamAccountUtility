namespace SteamAccountUtility.Messages;

public class UpdateLoginMessage(string newMessage, bool isButtonEnabled)
{
    public readonly string NewMessage = newMessage;
    public readonly bool IsButtonEnabled = isButtonEnabled;
}
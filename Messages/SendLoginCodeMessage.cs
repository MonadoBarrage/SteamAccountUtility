namespace SteamAccountUtility.Messages;

public class SendLoginCodeMessage(string code)
{
    public string loginCode = code;
}

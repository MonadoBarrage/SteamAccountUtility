namespace SteamAccountUtility.Messages;

public class UpdateLoginMessage(string n, bool triggerButton = false)
{
    public string NewMessage = n;
    public bool TriggerButton = triggerButton;
}
using CommunityToolkit.Mvvm.Messaging.Messages;
using SteamAccountUtility.ViewModels;

namespace SteamAccountUtility.Messages;

public class LoginMessage: AsyncRequestMessage<LoginWindowViewModel?>;
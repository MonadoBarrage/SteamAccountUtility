using CommunityToolkit.Mvvm.ComponentModel;
using SteamAccountUtility.Models;

namespace SteamAccountUtility.ViewModels;

public partial class AuxiliaryFriendViewModel(FriendData fd) : ViewModelBase
{
    [ObservableProperty]
    private FriendData _friend = fd;
}

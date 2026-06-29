using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace SteamAccountUtility.ViewModels;

public partial class FriendListViewModel(ObservableCollection<AuxiliaryFriendViewModel> fl) : ViewModelBase
{
    [ObservableProperty] private ObservableCollection<AuxiliaryFriendViewModel> _friendlist = fl;
    
}
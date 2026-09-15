using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using SteamAccountUtility.ViewModels.AuxiliaryViewModels;

namespace SteamAccountUtility.ViewModels;

public partial class FriendListViewModel(ObservableCollection<AuxiliaryFriendViewModel> fl) : ViewModelBase
{
    [ObservableProperty] private ObservableCollection<AuxiliaryFriendViewModel> _friendlist = fl;
    
}
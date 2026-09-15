using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SteamAccountUtility.Models;

namespace SteamAccountUtility.ViewModels.AuxiliaryViewModels;

public partial class AuxiliaryFriendViewModel(FriendData fd): ViewModelBase
{
    
    
    
    [ObservableProperty]
    private FriendData _friend = fd;
    
    [RelayCommand]
    private void Yay()
    {
        Console.WriteLine(Friend.ProfileName);
    }
}
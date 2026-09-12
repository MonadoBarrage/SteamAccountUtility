using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
namespace SteamAccountUtility.ViewModels;

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
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SteamAccountUtility.Messages;
using SteamAccountUtility.Messages.Navigation;
using SteamKit2;

namespace SteamAccountUtility.ViewModels;

public partial class HomeWindowViewModel(AllSteamData allSteamData): ViewModelBase
{
    
    [ObservableProperty] private UserData currentUser = allSteamData.currentUser;
    [ObservableProperty] private ObservableCollection<AuxiliaryGameViewModel> gameList = allSteamData.gameVM;
    [ObservableProperty] private ObservableCollection<AuxiliaryFriendViewModel> friendList = allSteamData.friendVM;



    [RelayCommand]
    public void NavigateToFriendPage()
    {
        WeakReferenceMessenger.Default.Send(new SendToFriendPage());
    }
    
    [RelayCommand]
    public void NavigateToGamePage()
    {
        WeakReferenceMessenger.Default.Send(new SendToGamePage());
    }
}
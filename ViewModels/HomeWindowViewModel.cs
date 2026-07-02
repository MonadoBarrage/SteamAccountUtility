using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SteamAccountUtility.Messages.Navigation;

namespace SteamAccountUtility.ViewModels;

public partial class HomeWindowViewModel(AllSteamData allSteamData): ViewModelBase
{
    
    [ObservableProperty] private UserData _currentUser = allSteamData.CurrentUser;
    [ObservableProperty] private ObservableCollection<AuxiliaryGameViewModel> _gameList = allSteamData.GameVM;
    [ObservableProperty] private ObservableCollection<AuxiliaryFriendViewModel> _friendList = allSteamData.FriendVM;



    [RelayCommand]
    private void NavigateToFriendPage()
    {
        WeakReferenceMessenger.Default.Send(new SendToFriendPage());
    }
    
    [RelayCommand]
    private void NavigateToGamePage()
    {
        WeakReferenceMessenger.Default.Send(new SendToGamePage());
    }
    
    [RelayCommand]
    private void NavigateToGameRandomizerPage()
    {
        WeakReferenceMessenger.Default.Send(new SendToGameRandomizerPage());
    }
}
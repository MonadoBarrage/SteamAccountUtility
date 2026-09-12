using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SteamAccountUtility.Messages;

namespace SteamAccountUtility.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{

    [ObservableProperty] private ViewModelBase _currentPage;
    
    [ObservableProperty] private AllSteamData? _allSteamData;

    [ObservableProperty] private bool _isBarVisible;
    
    
    public MainWindowViewModel()
    {
        _isBarVisible = false;

        var fullServerAddress = Environment.GetEnvironmentVariable("SAH_SERVER_ADDRESS");
        if (string.IsNullOrEmpty(fullServerAddress))
        {
            CurrentPage = new ErrorWindowViewModel();
        }
        else
        {
            CurrentPage = new LoginWindowViewModel(fullServerAddress);
        }

        WeakReferenceMessenger.Default.Register<MainWindowViewModel, GoToHomePageMessage>
        (this, (mainWindow, receivedMessage) =>
        {
            mainWindow.AllSteamData = receivedMessage.UserSteamData;
            mainWindow.IsBarVisible = true;
            mainWindow.CurrentPage = new HomeWindowViewModel(mainWindow.AllSteamData);
        });
    }

    [RelayCommand]
    private void GoToHomePage()
    {
        if(AllSteamData != null)
            CurrentPage = new HomeWindowViewModel(AllSteamData);
    }
    [RelayCommand]
    private void GoToGameLibraryPage()
    {
        if(AllSteamData != null)
            CurrentPage = new GameLibraryViewModel(AllSteamData.GameVm);
    }
    [RelayCommand]
    private void GoToFriendsPage()
    {
        if(AllSteamData != null)
            CurrentPage = new FriendListViewModel(AllSteamData.FriendVm);
    }
    [RelayCommand]
    private void GoToGameRandomizerPage()
    {
        if(AllSteamData != null)
            CurrentPage = new GameRandomizerViewModel(AllSteamData.GameVm);
    }
    
    [RelayCommand]
    private void ChangeBool()
    {
        IsBarVisible = !IsBarVisible;
    }
    
    
}

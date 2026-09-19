using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SteamAccountUtility.Messages;
using SteamAccountUtility.Models;

namespace SteamAccountUtility.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty]
    private ViewModelBase _currentPage;

    [ObservableProperty]
    private AllSteamData? _allSteamData;

    [ObservableProperty]
    private bool _isBarVisible;

    [ObservableProperty]
    private Dictionary<string, ViewModelBase> _pageDictionary;

    public MainWindowViewModel()
    {
        _isBarVisible = false;

        var fullServerAddress = "banana-slamma.tyrannosaurus-mixolydian.ts.net:9116";
        if (string.IsNullOrEmpty(fullServerAddress))
        {
            CurrentPage = new ErrorWindowViewModel();
        }
        else
        {
            CurrentPage = new LoginWithDefaultViewModel(fullServerAddress);
        }

        WeakReferenceMessenger.Default.Register<MainWindowViewModel, CurrentlyLoggingInMessage>(
            this,
            (mainWindow, receivedMessage) =>
            {
                mainWindow.CurrentPage = new LoadingViewModel();
            }
        );

        WeakReferenceMessenger.Default.Register<MainWindowViewModel, LoadingSuccessfulMessage>(
            this,
            (mainWindow, receivedMessage) =>
            {
                _allSteamData = receivedMessage.steamData;
                if (_allSteamData == null)
                {
                    mainWindow.CurrentPage = new ErrorWindowViewModel();
                    return;
                }
                mainWindow.AllSteamData = _allSteamData;
                mainWindow.IsBarVisible = true;
                mainWindow.CurrentPage = new HomeWindowViewModel(mainWindow.AllSteamData);
            }
        );

        WeakReferenceMessenger.Default.Register<MainWindowViewModel, LoadingFailedMessage>(
            this,
            (mainWindow, _) =>
            {
                mainWindow.CurrentPage = new LoginWithDefaultViewModel(fullServerAddress);
            }
        );
    }

    [RelayCommand]
    private void GoToHomePage()
    {
        if (AllSteamData != null)
            CurrentPage = new HomeWindowViewModel(AllSteamData);
    }

    [RelayCommand]
    private void GoToGameLibraryPage()
    {
        if (AllSteamData != null)
            CurrentPage = new GameLibraryViewModel(AllSteamData.GameVm);
    }

    [RelayCommand]
    private void GoToFriendsPage()
    {
        if (AllSteamData != null)
            CurrentPage = new FriendListViewModel(AllSteamData.FriendVm);
    }

    [RelayCommand]
    private void GoToGameRandomizerPage()
    {
        if (AllSteamData != null)
            CurrentPage = new GameRandomizerViewModel(AllSteamData.GameVm);
    }

    [RelayCommand]
    private void ChangeBool()
    {
        IsBarVisible = !IsBarVisible;
    }
}

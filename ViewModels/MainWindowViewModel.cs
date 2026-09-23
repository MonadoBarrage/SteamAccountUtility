using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SteamAccountUtility.Messages;
using SteamAccountUtility.Models;
using SteamAccountUtility.Services;

namespace SteamAccountUtility.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty]
    private ViewModelBase _currentPage;

    [ObservableProperty]
    private AllSteamData? _allSteamData;

    [ObservableProperty]
    private bool _isBarVisible;

    private SteamLogin _steamLogin;
    
    private bool _isLoggedIn;
    
    public MainWindowViewModel()
    {
        _isBarVisible = false;

        var fullServerAddress = "banana-slamma.tyrannosaurus-mixolydian.ts.net:9116";
        if (string.IsNullOrEmpty(fullServerAddress))
        {
            CurrentPage = new ErrorWindowViewModel();
            throw new Exception();
        }

        CurrentPage = new LoadingViewModel();
        _steamLogin = new SteamLogin(fullServerAddress);

        WeakReferenceMessenger.Default.Register<MainWindowViewModel, CurrentlyLoggingInMessage>(
            this,
            (mainWindow, receivedMessage) =>
            {
                mainWindow.CurrentPage = new LoadingViewModel(receivedMessage.Message);
            }
        );

        WeakReferenceMessenger.Default.Register<MainWindowViewModel, LoadingSuccessfulMessage>(
            this,
            (mainWindow, receivedMessage) =>
            {
                Console.WriteLine(
                    "Successfully received fetched Steam Data. Attempting to move to Home Window . . ."
                );

                _allSteamData = receivedMessage.steamData;

                if (_allSteamData == null)
                {
                    mainWindow.CurrentPage = new ErrorWindowViewModel();
                    return;
                }
                _isLoggedIn = true;
                mainWindow.AllSteamData = _allSteamData;
                mainWindow.IsBarVisible = true;
                mainWindow.CurrentPage = new HomeWindowViewModel(mainWindow.AllSteamData);
            }
        );

        WeakReferenceMessenger.Default.Register<MainWindowViewModel, GoToLoginScreen>(
            this,
            (mainWindow, receivedMessage) =>
            {
                if (_isLoggedIn)
                    return;
                
                mainWindow.CurrentPage = new LoginWindowViewModel(
                    fullServerAddress,
                    receivedMessage.ErrorMessage
                );
            }
        );

        WeakReferenceMessenger.Default.Register<MainWindowViewModel, GoToSteamGuardCode>(
            this,
            (mainWindow, receivedMessage) =>
            {
                mainWindow.CurrentPage = new LoginSubmitCodeViewModel();
            }
        );

        _ = _steamLogin.LoginToSteam(SteamLoginType.RefreshToken);
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

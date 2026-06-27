using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SteamAccountUtility.Messages;
using SteamAccountUtility.Models;
using SteamKit2;

/**
 * Note: Functions for fetching credentials are NOT secured or encrypted.
 * This is for testing purposes only and will not be in the final app
 * 
 */


namespace SteamAccountUtility.ViewModels;

public partial class LoginWindowViewModel : ViewModelBase
{

    [ObservableProperty] private string username;
    [ObservableProperty] private string password;
    [ObservableProperty] private string steamKey;
    [ObservableProperty] private string loadingMessage;
    
    [ObservableProperty] private bool checkForSavedCredentials = true;

    private string guardData;
    private string accessToken;
    private SteamLogin _steamLogin;
    private AppDirectory _appDirectory;
    
    public LoginWindowViewModel()
    {
        _steamLogin = new SteamLogin();
        _appDirectory = new AppDirectory();
        
        _appDirectory.SetUpNewDirectory();

        try
        {
            var jsonDetails = _appDirectory.GetCredentials();
            Username = jsonDetails.Username;
            Password =  jsonDetails.Password;
            SteamKey = jsonDetails.SteamKey;
            guardData = jsonDetails.GuardData;
            accessToken = jsonDetails.AccessToken;
        }
        catch (Exception e)
        {
            Username = "";
            Password = "";
            SteamKey = "";
            guardData = "";
            accessToken = "";
        }
        
        
        WeakReferenceMessenger.Default.Register<LoginWindowViewModel, UpdateLoginMessage>
            (this, static (win, mang) =>
            {
                win.LoadingMessage = mang.newMessage;
            });
        
        WeakReferenceMessenger.Default.Register<LoginWindowViewModel, SendGuardDataAndAccessToken>
        (this, static (win, mang) =>
        {
            if (win.CheckForSavedCredentials)
            {
                win._appDirectory.SaveCredentials(new LoginDetails
                {
                    Username = win.Username, 
                    Password = win.Password, 
                    SteamKey = win.SteamKey,
                    GuardData = mang.GuardData,
                    AccessToken = mang.AccessToken
                });
            }
        });
        
        
    }

    [RelayCommand]
    public async Task LoginToSteamAsync()
    {
        
        if (string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(Password))
        {
            LoadingMessage = "Please enter a username and password";
            return;
        }
        LoadingMessage = "Logging in...";

        if (CheckForSavedCredentials)
        {
            _appDirectory.SaveCredentials(new LoginDetails
            {
                Username = Username, Password = Password, SteamKey = SteamKey, GuardData = guardData, AccessToken =  accessToken
            });
        }
        
        _steamLogin.GetCredentials(Username, Password, SteamKey, guardData, accessToken);
        
        await _steamLogin.InitializeClient();
        LoadingMessage = "Signing in";
        
        
    }
    
}
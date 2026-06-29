using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SteamAccountUtility.Messages;
using SteamAccountUtility.Models;
using SteamKit2;

/*
  Note: Functions for fetching credentials are NOT secured or encrypted.
  This is for testing purposes only and will not be in the final app
  
 */


namespace SteamAccountUtility.ViewModels;

public partial class LoginWindowViewModel : ViewModelBase
{

    [ObservableProperty] private string? _username;
    [ObservableProperty] private string? _password;
    [ObservableProperty] private string? _steamKey;
    [ObservableProperty] private string? _loadingMessage;
    
    [ObservableProperty] private bool _isLoginButtonEnabled;
    [ObservableProperty] private bool _checkForSavedCredentials;
    
    private ConcurrentDictionary<SteamID, FriendData> _friendList;
    private UserData _userData;
    private List<GameData> _gamesList;

    private readonly string? _guardData;
    private readonly string? _accessToken;
    private readonly SteamLogin _steamLogin;
    private readonly AppDirectory _appDirectory;
    
    public LoginWindowViewModel()
    {
        _steamLogin = new SteamLogin();
        _appDirectory = new AppDirectory();
        
        _friendList = new ConcurrentDictionary<SteamID, FriendData>();
        _userData = new UserData();
        _gamesList = new List<GameData>();
        
        _appDirectory.SetUpNewDirectory();
        _isLoginButtonEnabled = true;

        try
        {
            var jsonDetails = _appDirectory.GetCredentials();
            CheckForSavedCredentials = jsonDetails != new LoginDetails();
            Username = jsonDetails.Username;
            Password =  jsonDetails.Password;
            SteamKey = jsonDetails.SteamKey;
            _guardData = jsonDetails.GuardData;
            _accessToken = jsonDetails.AccessToken;
            
        }
        catch (Exception)
        {
            Username = "";
            Password = "";
            SteamKey = "";
            _guardData = "";
            _accessToken = "";
        }
        
        
        WeakReferenceMessenger.Default.Register<LoginWindowViewModel, UpdateLoginMessage>
            (this, static (win, mang) =>
            {
                win.LoadingMessage = mang.NewMessage;
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
        
        WeakReferenceMessenger.Default.Register<LoginWindowViewModel, ReceiveFriendsList>
        (this, static (win, mang) =>
        {
            win._friendList = mang.NewFriendsList;

#if DEBUG
            Console.WriteLine("Received FriendsList:");
            foreach (var keyPair in win._friendList)
            {   
                Console.WriteLine($"{keyPair.Key}: {keyPair.Value}");
            }
#endif
            
            win.CheckIfAllDataFetched();
            
            
        });
        
        
        
        WeakReferenceMessenger.Default.Register<LoginWindowViewModel, ReceiveUserData>
        (this, static (win, mang) =>
        {
            win._userData = mang.User;

#if DEBUG
            Console.WriteLine("Received ProfileName:");
            Console.WriteLine(mang.User.ProfileName);
#endif
            win.CheckIfAllDataFetched();
            
            
        });
        
        WeakReferenceMessenger.Default.Register<LoginWindowViewModel, ReceiveGameList>
        (this, static (win, mang) =>
        {
            win._gamesList = mang.NewGameList;
            
#if DEBUG
            Console.WriteLine("Received GameList:");
            Console.WriteLine("Received FriendsList:");
            foreach (var g in win._gamesList)
            {   
                Console.WriteLine(g.Name);
                Console.WriteLine(g.ImgIconUrl);
            }
#endif
            win.CheckIfAllDataFetched();
            
        });
        
        
    }
    
    private void CheckIfAllDataFetched()
    {
        if (_friendList.Count > 0 && _gamesList.Count > 0 && _userData.ValidateData())
        {
            WeakReferenceMessenger.Default.Send(new GoToHomePage(true, _userData, _gamesList, _friendList));
        }
    }

    [RelayCommand]
    private async Task LoginToSteamAsync()
    {
        
        if (string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(Password) || string.IsNullOrEmpty(SteamKey))
        {
            LoadingMessage = "Please enter a username, password, and steam key";
            return;
        }
        LoadingMessage = "Logging in...";

        if (CheckForSavedCredentials)
        {
            _appDirectory.SaveCredentials(new LoginDetails
            {
                Username = Username, Password = Password, SteamKey = SteamKey, GuardData = _guardData, AccessToken =  _accessToken
            });
        }
        
        _steamLogin.GetCredentials(Username, Password, SteamKey, _guardData, _accessToken);
        
        await _steamLogin.InitializeClient();
        LoadingMessage = "Signing in";
        
        
    }
    
}
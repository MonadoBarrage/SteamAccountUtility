using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json;
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
    
    private ObservableCollection<FriendData>? _friendList;
    private UserData _userData;
    private ObservableCollection<GameData>? _gamesList;
    private BadgeResponse? _badgeResponse;
    private RecentlyPlayedGamesResponse? _recentlyPlayedGamesResponse;

    private readonly string? _guardData;
    private readonly string? _accessToken;
    private readonly SteamLogin _steamLogin;
    private readonly AppDirectory _appDirectory;

    private bool fetchedGames;
    private bool fetchedFriends;
    private bool fetchedUser;
    private bool fetchedBadges;
    private bool fetchedRecentlyPlayedGames;
    
    public LoginWindowViewModel()
    {
        _steamLogin = new SteamLogin();
        _appDirectory = new AppDirectory();
        
        _friendList = new ObservableCollection<FriendData>();
        _userData = new UserData();
        _gamesList = new ObservableCollection<GameData>();
        
        fetchedGames = false;
        fetchedFriends = false;
        fetchedUser = false;
        
        _appDirectory.SetUpNewDirectory();
        _isLoginButtonEnabled = true;
        
        WeakReferenceMessenger.Default.Register<LoginWindowViewModel, UpdateLoginMessage>
            (this, static (loginWindow, receivedMessage) =>
            {
                loginWindow.LoadingMessage = receivedMessage.NewMessage;
                loginWindow.IsLoginButtonEnabled = receivedMessage.IsButtonEnabled;
            });
        
        WeakReferenceMessenger.Default.Register<LoginWindowViewModel, SaveForAutoLogin>
        (this, static (loginWindow, receivedMessage) =>
        {

                loginWindow._appDirectory.SaveCredentials(new LoginDetails
                {
                    Username = loginWindow.Username, 
                    SteamKey = loginWindow.SteamKey,
                    GuardData = receivedMessage.GuardData,
                    AccessToken = receivedMessage.AccessToken
                });
            
        });
        
        WeakReferenceMessenger.Default.Register<LoginWindowViewModel, ReceiveFriendsList>
        (this, static (loginWindow, receivedMessage) =>
        {
            Console.WriteLine("Fetched friends list");
            loginWindow.fetchedFriends = true;
            if(receivedMessage.NewFriendsList != null)
                loginWindow._friendList = new ObservableCollection<FriendData>(receivedMessage.NewFriendsList.Values);
            
            loginWindow.CheckIfAllDataFetched();
            
            
        });
        
        WeakReferenceMessenger.Default.Register<LoginWindowViewModel, ReceiveUserData>
        (this, static (loginWindow, receivedMessage) =>
        {
            Console.WriteLine("Fetched user data");
            loginWindow.fetchedUser = true;
            loginWindow._userData = receivedMessage.User;
            loginWindow.CheckIfAllDataFetched();
        });
        
        WeakReferenceMessenger.Default.Register<LoginWindowViewModel, ReceiveGameList>
        (this, static (loginWindow, receivedMessage) =>
        {
            Console.WriteLine("Fetched games list");
            loginWindow.fetchedGames = true;
            loginWindow._gamesList = receivedMessage.NewGameList;
            loginWindow.CheckIfAllDataFetched();
            
        });
        
        WeakReferenceMessenger.Default.Register<LoginWindowViewModel, ReceiveRecentlyPlayedGames>
        (this, static (loginWindow, receivedMessage) =>
        {
            Console.WriteLine("Fetched recently played games");
            loginWindow.fetchedRecentlyPlayedGames = true;
            loginWindow._recentlyPlayedGamesResponse = receivedMessage.RecentlyPlayedGames;
            loginWindow.CheckIfAllDataFetched();
            
        });
        
        WeakReferenceMessenger.Default.Register<LoginWindowViewModel, ReceiveBadgeResponse>
        (this, static (loginWindow, receivedMessage) =>
        {
            Console.WriteLine("Fetched badges");
            loginWindow.fetchedBadges = true;
            loginWindow._badgeResponse = receivedMessage.FetchedBadges;
            loginWindow.CheckIfAllDataFetched();
            
        });
        
        try
        {
            var jsonDetails = _appDirectory.GetCredentials();
            Username = jsonDetails.Username;
            SteamKey = jsonDetails.SteamKey;
            _guardData = jsonDetails.GuardData;
            _accessToken = jsonDetails.AccessToken;
            
            
            if (!string.IsNullOrEmpty(Username) 
                && !string.IsNullOrEmpty(SteamKey) 
                && !string.IsNullOrEmpty(_accessToken))
            {
                if (ParseRefreshToken(_accessToken))
                    Task.Run(async () =>
                    {
                        await LoginToSteamAsync(true);
                    });
                else LoadingMessage = "Session expired. Please log in again";
            }
            
        }
        catch (Exception)
        {
            Username = "";
            Password = "";
            SteamKey = "";
            _guardData = "";
            _accessToken = "";
        }
        
        
    }
    
    private void CheckIfAllDataFetched()
    {
        if (fetchedFriends && fetchedGames && fetchedUser && fetchedRecentlyPlayedGames && fetchedBadges)
        {
            var friendViewModels = new ObservableCollection<AuxiliaryFriendViewModel>();
            var gameViewModels = new ObservableCollection<AuxiliaryGameViewModel>();
            if(_friendList != null)
                foreach (var f in _friendList)
                {
                    friendViewModels.Add(new AuxiliaryFriendViewModel(f));
                }

            if(_gamesList != null)
                foreach (var g in _gamesList)
                {
                    gameViewModels.Add(new AuxiliaryGameViewModel(g));
                }
             
            var newSteamData = new AllSteamData()
            {
                CurrentUser =  _userData,
                Friends = _friendList,
                Games = _gamesList,
                FriendVM = friendViewModels,
                GameVM =  gameViewModels,
                FetchedBadgesResponse = _badgeResponse,
                FetchedRecentlyPlayedGamesResponse = _recentlyPlayedGamesResponse
                 
            };
            _appDirectory.SaveStuff(newSteamData);
            WeakReferenceMessenger.Default.Send(new GoToHomePage(newSteamData));
        }
    }

    [RelayCommand]
    private async Task LoginToSteamAsync(bool? useAutoLogin)
    {
        
        if (useAutoLogin == false && (string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(Password) || string.IsNullOrEmpty(SteamKey)))
        {
            LoadingMessage = "Please enter a username, password, and steam key";
            return;
        }
        
        IsLoginButtonEnabled = false;
        LoadingMessage = "Logging in...";
        
        
        _steamLogin.GetCredentials(Username, Password, SteamKey, _guardData, _accessToken);
        
        await _steamLogin.InitializeClient(useAutoLogin ?? false);
        
    }
    
    private static bool ParseRefreshToken(string token)
    {
        var tokenComponents = token.Split('.');

        var base64 = tokenComponents[1].Replace('-', '+').Replace('_', '/');

        if (base64.Length % 4 != 0) base64 += new string('=', 4 - base64.Length % 4);

        var payloadBytes = Convert.FromBase64String(base64);
        
        var newTokenJson = JsonSerializer.Deserialize<RefreshTokenJson>(payloadBytes);
        
        return (newTokenJson != null) && CheckUnixTimestamp(newTokenJson.Expiration);
    }
    private static bool CheckUnixTimestamp(long? expirationTimestamp)
    {
        if (expirationTimestamp == null) return false;
        return DateTimeOffset.UtcNow.ToUnixTimeSeconds() < (expirationTimestamp - 604800);
    }
    
}
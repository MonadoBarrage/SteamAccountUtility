using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SteamAccountUtility.Messages;
using SteamAccountUtility.Models;

namespace SteamAccountUtility.ViewModels;

public partial class LoginWindowViewModel: ViewModelBase
{

    [ObservableProperty] private string? _username;
    [ObservableProperty] private string? _password;
    [ObservableProperty] private string? _loadingMessage;
    
    [ObservableProperty] private bool _isLoginButtonEnabled;
    
    private ObservableCollection<FriendData>? _friendList;
    private UserData _userData;
    private Dictionary<int, AppRenderedSteamGame>? _gamesList;
    private BadgesAndLevelsData? _badgesAndLevels;
    private List<int>? _recentlyPlayedGamesResponse;

    private readonly string? _guardData;
    private readonly string? _accessToken;
    private readonly SteamLogin _steamLogin;

    private bool _fetchedGames;
    private bool _fetchedFriends;
    private bool _fetchedUser;
    private bool _fetchedBadges;
    private bool _fetchedRecentlyPlayedGames;
    
    public LoginWindowViewModel(string serverAddress)
    {
        _steamLogin = new SteamLogin(serverAddress);
        
        _friendList = [];
        _userData = new UserData();
        _gamesList = new Dictionary<int, AppRenderedSteamGame>();
        
        _fetchedGames = false;
        _fetchedFriends = false;
        _fetchedUser = false;
        
        _isLoginButtonEnabled = true;
        
        WeakReferenceMessenger.Default.Register<LoginWindowViewModel, UpdateLoginMessage>
            (this, static (loginWindow, receivedMessage) =>
            {
                loginWindow.LoadingMessage = receivedMessage.NewMessage;
                loginWindow.IsLoginButtonEnabled = receivedMessage.IsButtonEnabled;
            });
        
        WeakReferenceMessenger.Default.Register<LoginWindowViewModel, ReceiveFriendsList>
        (this, static (loginWindow, receivedMessage) =>
        {
            Console.WriteLine("Fetched friends list");
            loginWindow._fetchedFriends = true;
            if(receivedMessage.NewFriendsList != null)
                loginWindow._friendList = new ObservableCollection<FriendData>(receivedMessage.NewFriendsList.Values);
            
            loginWindow.CheckIfAllDataFetched();
            
            
        });
        
        WeakReferenceMessenger.Default.Register<LoginWindowViewModel, ReceiveUserData>
        (this, static (loginWindow, receivedMessage) =>
        {
            Console.WriteLine("Fetched user data");
            loginWindow._fetchedUser = true;
            loginWindow._userData = receivedMessage.User;
            loginWindow.CheckIfAllDataFetched();
        });
        
        WeakReferenceMessenger.Default.Register<LoginWindowViewModel, ReceiveGameList>
        (this, static (loginWindow, receivedMessage) =>
        {
            Console.WriteLine("Fetched games list");
            loginWindow._fetchedGames = true;
            loginWindow._gamesList = receivedMessage.NewGameList;
            loginWindow.CheckIfAllDataFetched();
            
        });
        
        WeakReferenceMessenger.Default.Register<LoginWindowViewModel, ReceiveRecentlyPlayedGames>
        (this, static (loginWindow, receivedMessage) =>
        {
            Console.WriteLine("Fetched recently played games");
            loginWindow._fetchedRecentlyPlayedGames = true;
            loginWindow._recentlyPlayedGamesResponse = new List<int>(receivedMessage.RecentlyPlayedGames);
            loginWindow.CheckIfAllDataFetched();
            
        });
        
        WeakReferenceMessenger.Default.Register<LoginWindowViewModel, ReceiveBadgesAndLevels>
        (this, static (loginWindow, receivedMessage) =>
        {
            Console.WriteLine("Fetched badges");
            loginWindow._fetchedBadges = true;
            loginWindow._badgesAndLevels = receivedMessage.FetchedBadges;
            loginWindow.CheckIfAllDataFetched();
            
        });
        
        try
        {
            if (!string.IsNullOrEmpty(Username) 
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
            _guardData = "";
            _accessToken = "";
        }
    }
    
    private void CheckIfAllDataFetched()
    {
        if (_fetchedFriends && _fetchedGames && _fetchedUser && _fetchedRecentlyPlayedGames && _fetchedBadges)
        {
            var friendViewModels = new ObservableCollection<AuxiliaryFriendViewModel>();
            var gameViewModels = new ObservableCollection<AuxiliaryGameViewModel>();
            var recentlyPlayedGamesViewModels = new ObservableCollection<AppRenderedSteamGame>();
            
            if(_friendList != null)
                foreach (var f in _friendList)
                {
                    friendViewModels.Add(new AuxiliaryFriendViewModel(f));
                }

            if(_gamesList != null)
                foreach (var g in _gamesList)
                {
                    gameViewModels.Add(new AuxiliaryGameViewModel(g.Value));
                }

            if (_recentlyPlayedGamesResponse != null && _gamesList != null)
            {
                foreach (var recentIds in _recentlyPlayedGamesResponse)
                {
                    var g = _gamesList[recentIds];
                    recentlyPlayedGamesViewModels.Add(g);
                }
            }
            
            var newSteamData = new AllSteamData()
            {
                CurrentUser =  _userData,
                Friends = _friendList,
                Games = _gamesList,
                FriendVm = friendViewModels,
                GameVm =  gameViewModels,
                BadgesAndLevels = _badgesAndLevels,
                RecentGames = recentlyPlayedGamesViewModels
                 
            };
            WeakReferenceMessenger.Default.Send(new GoToHomePageMessage(newSteamData));
        }
    }

    [RelayCommand]
    private async Task LoginToSteamAsync(bool? useAutoLogin)
    {
        
        if (useAutoLogin == false && (string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(Password)))
        {
            LoadingMessage = "Please enter a username and password";
            return;
        }
        
        IsLoginButtonEnabled = false;
        LoadingMessage = "Logging in...";
        
        
        _steamLogin.GetCredentials(Username, Password, _guardData, _accessToken);
        
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
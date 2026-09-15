using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SteamAccountUtility.Messages;
using SteamAccountUtility.Models;
using SteamAccountUtility.Services;
using SteamAccountUtility.ViewModels.AuxiliaryViewModels;
namespace SteamAccountUtility.ViewModels;

public partial class LoginWindowViewModel: ViewModelBase
{

    [ObservableProperty] private string? _username;
    [ObservableProperty] private string? _password;
    [ObservableProperty] private string? _loadingMessage;
    
    [ObservableProperty] private bool _isLoginButtonEnabled;
    
    private ObservableCollection<FriendData> _friendList;
    private UserData _userData;
    private Dictionary<int, RenderedSteamGame> _gamesList;
    private BadgesAndLevelsResponse _badgesAndLevels = new();
    private List<int> _recentlyPlayedGamesResponse = [];

    private readonly string _guardData = "";
    private readonly string _accessToken = "";
    private readonly SteamLogin _steamLogin;

    private int _remainingFetches = 5;

    private readonly TaskCompletionSource<bool> _loginTaskCompletionSource;
    
    public LoginWindowViewModel(string serverAddress)
    {
        _steamLogin = new SteamLogin(serverAddress);
        _loginTaskCompletionSource = new TaskCompletionSource<bool>();
        _friendList = [];
        _userData = new UserData();
        _gamesList = new Dictionary<int, RenderedSteamGame>();
        
        _isLoginButtonEnabled = true;
        
        WeakReferenceMessenger.Default.Register<LoginWindowViewModel, LoadingFailedMessage>
        (this, static (loginWindow, _) =>
        {
            loginWindow._loginTaskCompletionSource.TrySetResult(false);
        });
        
        WeakReferenceMessenger.Default.Register<LoginWindowViewModel, UpdateLoginMessage>
            (this, static (loginWindow, receivedMessage) =>
            {
                loginWindow.LoadingMessage = receivedMessage.NewMessage;
                loginWindow.IsLoginButtonEnabled = receivedMessage.IsButtonEnabled;
            });
        
        
        WeakReferenceMessenger.Default.Register<LoginWindowViewModel, ReceiveFriendsList>
        (this, static (loginWindow, receivedMessage) => {
            if (!receivedMessage.FetchedSuccessfully)
            {
                loginWindow._loginTaskCompletionSource.TrySetResult(false);
                return;
            }
            Console.WriteLine("Fetched friends list");
            loginWindow.CompleteCallback();
            loginWindow._friendList = receivedMessage.NewFriendsList;
        });
        
        WeakReferenceMessenger.Default.Register<LoginWindowViewModel, ReceiveUserData>
        (this, static (loginWindow, receivedMessage) =>
        {
            
            if (!receivedMessage.FetchedSuccessfully)
            {
                loginWindow._loginTaskCompletionSource.TrySetResult(false);
                return;
            }
            Console.WriteLine("Fetched user data");
            loginWindow.CompleteCallback();
            loginWindow._userData = receivedMessage.User;
        });
        
        WeakReferenceMessenger.Default.Register<LoginWindowViewModel, ReceiveGameList>
        (this, static (loginWindow, receivedMessage) =>
        {
            if (!receivedMessage.FetchedSuccessfully)
            {
                loginWindow._loginTaskCompletionSource.TrySetResult(false);
                return;
            }
            Console.WriteLine("Fetched games list");
            loginWindow.CompleteCallback();
            loginWindow._gamesList = receivedMessage.NewGameList;
        });
        
        WeakReferenceMessenger.Default.Register<LoginWindowViewModel, ReceiveRecentlyPlayedGames>
        (this, static (loginWindow, receivedMessage) =>
        {
            if (!receivedMessage.FetchedSuccessfully)
            {
                loginWindow._loginTaskCompletionSource.TrySetResult(false);
                return;
            } 
            Console.WriteLine("Fetched recently played games");
            loginWindow.CompleteCallback();
            loginWindow._recentlyPlayedGamesResponse =  receivedMessage.RecentlyPlayedGames;
        });
        
        WeakReferenceMessenger.Default.Register<LoginWindowViewModel, ReceiveBadgesAndLevels>
        (this, static (loginWindow, receivedMessage) =>
        {
            if (!receivedMessage.FetchedSuccessfully)
            {
                loginWindow._loginTaskCompletionSource.TrySetResult(false);
                return;
            }
            Console.WriteLine("Fetched badges");
            loginWindow.CompleteCallback();
            loginWindow._badgesAndLevels = receivedMessage.FetchedBadges;
            
        });
        
        try
        {
            if (!string.IsNullOrEmpty(Username) 
                && !string.IsNullOrEmpty(_accessToken))
            {
                if (ParseRefreshToken(_accessToken))
                    Task.Run(async () =>
                    {
                        await LoginToSteamAsync();
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

    private void CompleteCallback()
    {
        if (Interlocked.Decrement(ref _remainingFetches) == 0)
        {
            _loginTaskCompletionSource.TrySetResult(true);
        }
    }

    [RelayCommand]
    private async Task LoginToSteamAsync()
    {
        
        // if (useAutoLogin == false && (string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(Password)))
        // {
        //     LoadingMessage = "Please enter a username and password";
        //     return;
        // }
        
        IsLoginButtonEnabled = false;
        LoadingMessage = "Logging in...";
        
        _steamLogin.GetCredentials(Username, Password, _guardData, _accessToken);
        
        _ = _steamLogin.InitializeClient();
        var didAllSteamRequestsSucceed = await _loginTaskCompletionSource.Task;
        
        
        if (!didAllSteamRequestsSucceed)
        {
            Console.WriteLine("Error processing");
            LoadingMessage = "Error processing request";
            IsLoginButtonEnabled = true;
            return;
        }

        var friendViewModels = new ObservableCollection<AuxiliaryFriendViewModel>();
        var gameViewModels = new ObservableCollection<AuxiliaryGameViewModel>();
        var recentlyPlayedGamesViewModels = new ObservableCollection<RenderedSteamGame>();
        
        foreach (var f in _friendList)
        {
            friendViewModels.Add(new AuxiliaryFriendViewModel(f));
        }
        foreach (var g in _gamesList)
        {
            gameViewModels.Add(new AuxiliaryGameViewModel(g.Value));
        }
        foreach (var recentIds in _recentlyPlayedGamesResponse)
        {
            if(_gamesList.TryGetValue(recentIds, out var game))
                recentlyPlayedGamesViewModels.Add(game);
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
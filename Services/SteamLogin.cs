using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Timers;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.Messaging;
using SteamAccountUtility.Messages;
using SteamAccountUtility.Models;
using SteamKit2;
using SteamKit2.Authentication;

namespace SteamAccountUtility.Services;

internal sealed class SteamLogin(string serverAddress) : IDisposable
{
    
    public void GetCredentials(string? username, string? password, string? guardData,
        string? accessToken)
    {
        
        _username = username;
        _password = password;
        _guardData = guardData;
        _refreshToken = accessToken;
    }
    
    private string? _username;
    private string? _password;
    private string? _guardData;
    private string? _refreshToken;

    
    private readonly ConcurrentDictionary<SteamID, FriendData> _friendsDictionary = new();
    private readonly ObservableCollection<FriendData> _friendsCollection = new();
    
    
    private bool _isRunning;
    private bool _isAutoLoginEnabled;
    
    private CallbackManager? _manager;
    private SteamClient? _steamClient;
    
    private SteamFriends? _steamFriends;
    private SteamUser? _steamUser;
    private readonly Timer _loginTimer = new (30000);
    private readonly SteamHttpRequests  _steamHttpRequests = new(serverAddress);
    
   

    
    
    private void QuitProgram(Object source, ElapsedEventArgs e)
    {
        _steamClient?.Disconnect();
        WeakReferenceMessenger.Default.Send(new LoadingFailedMessage());
    }
    
    public void Dispose()
    {
        throw new NotImplementedException();
    }

    private void SetTimer()
    {
        _loginTimer.Elapsed += QuitProgram;
        _loginTimer.AutoReset = false;
        _loginTimer.Enabled = true;
    }
    
    public async Task InitializeClient(bool useAutoLogin = false)
    {
        
        SetTimer();
        _steamClient = new SteamClient();

        _manager = new CallbackManager(_steamClient);
        _isAutoLoginEnabled = useAutoLogin;

        _steamUser = _steamClient.GetHandler<SteamUser>();
        _steamFriends = _steamClient.GetHandler<SteamFriends>();

        _manager.Subscribe<SteamClient.ConnectedCallback>(OnConnected);
        _manager.Subscribe<SteamClient.DisconnectedCallback>(OnDisconnected);

        _manager.Subscribe<SteamUser.LoggedOnCallback>(OnLoggedOn);
        _manager.Subscribe<SteamUser.LoggedOffCallback>(OnLoggedOff);

        _manager.Subscribe<SteamUser.AccountInfoCallback>(OnAccountInfo);
        _manager.Subscribe<SteamFriends.FriendsListCallback>(OnFriendsList);
        _manager.Subscribe<SteamFriends.PersonaStateCallback>(OnPersonaState);
        
        _isRunning = true;

        Console.WriteLine("Connecting to Steam...");

        _steamClient.Connect();
        
        while (_isRunning)
            await _manager.RunWaitCallbackAsync();
    }

    
    
    private async void OnConnected(SteamClient.ConnectedCallback callback)
    {
        try
        {
            Console.WriteLine("Connected to Steam! Logging in '{0}'...", _username);

            const bool shouldRememberPassword = true;
            
            WeakReferenceMessenger.Default.Send(
                new UpdateLoginMessage("Use the Steam Guard App to approve this login", false));

            if (_steamClient == null || _steamUser == null)
            {
                WeakReferenceMessenger.Default.Send(
                    new UpdateLoginMessage("Error: cannot log in", true));
                _isRunning = false;
                return;
            }

            var authSession = await _steamClient.Authentication.BeginAuthSessionViaCredentialsAsync(
                new AuthSessionDetails
                {
                    Username = _username,
                    Password = _password,
                    IsPersistentSession = shouldRememberPassword,

                    GuardData = _guardData,

                    Authenticator = new UserConsoleAuthenticator()
                });

            var pollResponse = await authSession.PollingWaitForResultAsync();

            if (pollResponse.NewGuardData != null)
                _guardData = pollResponse.NewGuardData;

            if (!string.IsNullOrEmpty(pollResponse.RefreshToken))
                _refreshToken = pollResponse.RefreshToken;
            
            _steamUser.LogOn(new SteamUser.LogOnDetails
            {
                Username = pollResponse.AccountName,
                AccessToken = _refreshToken,
                ShouldRememberPassword =
                    shouldRememberPassword 
            });
        }
        catch (Exception e)
        {
            WeakReferenceMessenger.Default.Send(
                new UpdateLoginMessage("Error: cannot log in", true));
            _isRunning = false;
            Console.WriteLine(e);
        }
    }

    private void OnDisconnected(SteamClient.DisconnectedCallback callback)
    {
        Console.WriteLine("Disconnected from Steam");
        _isRunning = false;
    }

    private void OnLoggedOn(SteamUser.LoggedOnCallback callback)
    {
        try
        {
            _loginTimer.Enabled = false;
            
            if (callback.Result != EResult.OK || _steamFriends == null || _steamUser == null || _steamUser.SteamID == null)
            {

                // Console.WriteLine("Unable to logon to Steam: {0} / {1}", callback.Result, callback.ExtendedResult);

                // WeakReferenceMessenger.Default.Send(
                //     new UpdateLoginMessage("Error: cannot log in", true));

                WeakReferenceMessenger.Default.Send(new LoadingFailedMessage());
                
                _isRunning = false;
                return;
            }
            
            Console.WriteLine("Successfully logged on!");
            
            _ = _steamFriends.RequestProfileInfo(_steamUser.SteamID);
            _ = GetGameList();
            _ = GetBadgesAndLevels();
            _ = GetRecentlyPlayedGames();
        }
        catch (Exception e)
        {
            WeakReferenceMessenger.Default.Send(
                new UpdateLoginMessage("Error: cannot log in", true));
            _isRunning = false;
            Console.WriteLine(e);
        }
    }

    private static void OnLoggedOff(SteamUser.LoggedOffCallback callback)
    {
#if DEBUG
        Console.WriteLine("Logged off of Steam: {0}", callback.Result);
#endif
    }
    
    

    private void OnAccountInfo(SteamUser.AccountInfoCallback callback)
    {
        _steamFriends?.SetPersonaState(EPersonaState.Online);
    }

    private async void OnFriendsList(SteamFriends.FriendsListCallback callback)
    {
        try
        {
            if (_steamFriends == null || _steamUser == null) return;
            
            for (var x = 0; x < _steamFriends.GetFriendCount(); x++)
            {
                var steamIdFriend = _steamFriends.GetFriendByIndex(x);
                if (steamIdFriend == _steamUser.SteamID) continue;
               
                await _steamFriends.RequestProfileInfo(steamIdFriend);
            }
            
            WeakReferenceMessenger.Default.Send(new ReceiveFriendsList(_friendsCollection, true));
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    private async void OnPersonaState(SteamFriends.PersonaStateCallback callback)
    {
        try
        {
            if (_steamUser == null || _steamUser.SteamID == null) return;
            if (callback.FriendID == _steamUser.SteamID)
            {

               var newAvatarPhoto = await _steamHttpRequests.FetchUserAvatar(callback.AvatarHash);

                var ud = new UserData()
                {
                    SteamID = callback.FriendID,
                    ProfileName = callback.Name,
                    AvatarHash = callback.AvatarHash,
                    AvatarIcon = newAvatarPhoto,
                };

                WeakReferenceMessenger.Default.Send(new ReceiveUserData(ud, true));
                return;
            }

            if (_friendsDictionary.ContainsKey(callback.FriendID))
            {
                return;
            }
            
            var avatarPhoto = await _steamHttpRequests.FetchUserAvatar(callback.AvatarHash);

            var fd = new FriendData()
            {
                SteamID = callback.FriendID,
                ProfileName = callback.Name,
                AvatarHash = callback.AvatarHash,
                AvatarIcon = avatarPhoto
            };
            _friendsDictionary.GetOrAdd(callback.FriendID, fd);
            _friendsCollection.Add(fd);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
    
    
    private async Task GetGameList()
    {
        if (_steamUser == null || _steamUser.SteamID == null) return;

        var sid = new SteamID(_steamUser.SteamID);
        Dictionary<int, RenderedSteamGame> gamesLibrary = await _steamHttpRequests.FetchUserGameLibrary(sid);

        if(gamesLibrary != null)
            WeakReferenceMessenger.Default.Send(new ReceiveGameList(gamesLibrary, true));
        else
            WeakReferenceMessenger.Default.Send(new StopFetchingInfoMessage());
    }
    
    private async Task GetBadgesAndLevels()
    {
        try
        {
            if (_steamUser == null || _steamUser.SteamID == null) return;
            var sid = new SteamID(_steamUser.SteamID);

            var badgesAndLevels = await _steamHttpRequests.FetchUserBadgesAndLevels(sid);

            WeakReferenceMessenger.Default.Send(new ReceiveBadgesAndLevels(badgesAndLevels, true));
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e);
        }
    }

    private async Task GetRecentlyPlayedGames()
    {
        try
        {
            if (_steamUser == null || _steamUser.SteamID == null) return;

            var sid = new SteamID(_steamUser.SteamID);

            var recentlyPlayed = await _steamHttpRequests.FetchRecentlyPlayedGames(sid);

            List<int> gameIDs = new List<int>();
            if (recentlyPlayed != null && recentlyPlayed.Response != null && recentlyPlayed.Response.Games != null)
            {
                foreach (var game in recentlyPlayed.Response.Games)
                {
                    gameIDs.Add(game.AppId);
                }
            }
            
            
            WeakReferenceMessenger.Default.Send(new ReceiveRecentlyPlayedGames(gameIDs,true));
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e);
            // WeakReferenceMessenger.Default.Send(new ReceiveRecentlyPlayedGames());
        }
    }

    private void ErrorHandling()
    {
        
    }
}
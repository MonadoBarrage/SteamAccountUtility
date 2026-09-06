using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.Messaging;
using SteamAccountUtility.Messages;
using SteamKit2;
using SteamKit2.Authentication;

namespace SteamAccountUtility.Models;

internal sealed class SteamLogin : IDisposable
{
    public void GetCredentials(string? username, string? password, string? steamKey, string? guardData,
        string? accessToken)
    {
        _username = username;
        _password = password;
        _steamKey = steamKey;
        _guardData = guardData;
        _refreshToken = accessToken;
    }

    private string? _username;
    private string? _password;
    private string? _steamKey;

    private string? _guardData;
    private string? _refreshToken;

    private readonly ConcurrentDictionary<SteamID, FriendData> _friendsList = new();
    private readonly ConcurrentBag<GameData> _steamGames = new();
    private readonly ConcurrentBag<GameData> _recentlyPlayedSteamGames = new();

    private bool _isRunning;
    private bool _isAutoLoginEnabled;

    private CallbackManager? _manager;
    private SteamClient? _steamClient;
    private SteamFriends? _steamFriends;
    private SteamUser? _steamUser;
    private SteamApps? _steamApps;
    
    private BadgeResponse? _badgesAndLevels;
    private RecentlyPlayedGamesResponse? _recentlyPlayedCollection;
    
    private readonly SteamHttpRequests  _steamHttpRequests = new SteamHttpRequests();
    
    public void Dispose()
    {
        throw new NotImplementedException();
    }


    public async Task InitializeClient(bool useAutoLogin = false)
    {
        
        
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

            if (_isAutoLoginEnabled)
            {
                _steamUser?.LogOn(new SteamUser.LogOnDetails
                {
                    Username = _username,
                    AccessToken = _refreshToken,
                    ShouldRememberPassword =
                        shouldRememberPassword
                });

                return;
            }


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
                // When using certain two factor methods (such as email 2fa), guard data may be provided by Steam
                // for use in future authentication sessions to avoid triggering 2FA again (this works similarly to the old sentry file system).
                // Do note that this guard data is also a JWT token and has an expiration date.
                _guardData = pollResponse.NewGuardData;

            if (!string.IsNullOrEmpty(pollResponse.RefreshToken))
                _refreshToken = pollResponse.RefreshToken;


            _steamUser.LogOn(new SteamUser.LogOnDetails
            {
                Username = pollResponse.AccountName,
                AccessToken = _refreshToken,
                ShouldRememberPassword =
                    shouldRememberPassword // If you set IsPersistentSession to true, this also must be set to true for it to work correctly
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
            if (callback.Result != EResult.OK)
            {

                Console.WriteLine("Unable to logon to Steam: {0} / {1}", callback.Result, callback.ExtendedResult);

                WeakReferenceMessenger.Default.Send(
                    new UpdateLoginMessage("Error: cannot log in", true));
                _isRunning = false;
                return;
            }

            WeakReferenceMessenger.Default.Send(new SaveForAutoLogin(_guardData,
                _refreshToken));

            Console.WriteLine("Successfully logged on!");

            if (_steamFriends == null || _steamUser == null || _steamUser.SteamID == null) return;
            
            _ = _steamFriends.RequestProfileInfo(_steamUser.SteamID);
            _ = FetchGameList();
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

            
            WeakReferenceMessenger.Default.Send(new ReceiveFriendsList(_friendsList));
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

                WeakReferenceMessenger.Default.Send(new ReceiveUserData(ud));
                return;
            }

            if (_friendsList.ContainsKey(callback.FriendID))
            {
                return;
            }

            
            Bitmap? avatarPhoto = await _steamHttpRequests.FetchUserAvatar(callback.AvatarHash);

            FriendData fd = new FriendData()
            {
                SteamID = callback.FriendID,
                ProfileName = callback.Name,
                AvatarHash = callback.AvatarHash,
                AvatarIcon = avatarPhoto
            };
            _friendsList.GetOrAdd(callback.FriendID, fd);
            
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    private async Task FetchGameList()
    {
        if (_steamUser == null || _steamUser.SteamID == null || _steamKey == null) return;

        var sid = new SteamID(_steamUser.SteamID);
        ObservableCollection<AppRenderedSteamGame>? gamesLibrary = await _steamHttpRequests.FetchUserGameLibrary(_steamKey, sid);

        if(gamesLibrary != null)
            WeakReferenceMessenger.Default.Send(new ReceiveGameList(new ObservableCollection<AppRenderedSteamGame>(gamesLibrary)));
        
    }
    

    private async Task GetBadgesAndLevels()
    {
        try
        {
            if (_steamUser == null || _steamUser.SteamID == null || _steamKey == null) return;
            var sid = new SteamID(_steamUser.SteamID);

            _badgesAndLevels = await _steamHttpRequests.FetchUserBadgesAndLevels(_steamKey, sid);

            WeakReferenceMessenger.Default.Send(new ReceiveBadgeResponse(_badgesAndLevels));
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e);
            _badgesAndLevels = new BadgeResponse();
            WeakReferenceMessenger.Default.Send(new ReceiveBadgeResponse(_badgesAndLevels));
        }
    }

    private async Task GetRecentlyPlayedGames()
    {
        try
        {
            if (_steamUser == null || _steamUser.SteamID == null || _steamKey == null) return;

            var sid = new SteamID(_steamUser.SteamID);

            var recentlyPlayed = await _steamHttpRequests.FetchRecentlyPlayedGames(_steamKey, sid);

            List<int> gameIDs = new List<int>();

            
            
            WeakReferenceMessenger.Default.Send(new ReceiveRecentlyPlayedGames(gameIDs));
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e);
            // WeakReferenceMessenger.Default.Send(new ReceiveRecentlyPlayedGames());
        }
    }
}
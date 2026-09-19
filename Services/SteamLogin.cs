using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.Messaging;
using QRCoder;
using SkiaSharp;
using SteamAccountUtility.Messages;
using SteamAccountUtility.Models;
using SteamAccountUtility.ViewModels;
using SteamKit2;
using SteamKit2.Authentication;

namespace SteamAccountUtility.Services;

internal sealed class SteamLogin: IDisposable
{
    
    private const bool ShouldRememberPassword = true;
    private string? _username;
    private string? _password;
    private string? _guardData;
    private string? _refreshToken;

    private readonly ConcurrentDictionary<SteamID, FriendData> _friendsDictionary = new();

    private bool _isRunning;

    private readonly CallbackManager _manager;
    private readonly SteamClient _steamClient;
    private readonly SteamHttpRequests _steamHttpRequests;
    
    private SteamFriends? _steamFriends;
    private SteamUser? _steamUser;
    
    private int _remainingFetches;
    private readonly TaskCompletionSource<bool> _loginTaskCompletionSource = new();
    
    private UserData _userData = new();
    private readonly ObservableCollection<FriendData> _friendsCollection = [];
    private Dictionary<int, RenderedSteamGame> _gamesList = [];
    private BadgesAndLevelsResponse _badgesAndLevels = new();
    private List<int> _recentlyPlayedGamesResponse = [];

    private bool _useQrCodeVerification;
    
    public void GetCredentials(string? username, string? password)
    {
        _username = username;
        _password = password;
    }

    public SteamLogin(string serverAddress)
    {
        _steamClient = new SteamClient();

        _manager = new CallbackManager(_steamClient);
        _steamHttpRequests = new SteamHttpRequests(serverAddress);
        _steamUser = _steamClient.GetHandler<SteamUser>();
        _steamFriends = _steamClient.GetHandler<SteamFriends>();

        _manager.Subscribe<SteamClient.ConnectedCallback>(OnConnected);
        _manager.Subscribe<SteamClient.DisconnectedCallback>(OnDisconnected);

        _manager.Subscribe<SteamUser.LoggedOnCallback>(OnLoggedOn);
        _manager.Subscribe<SteamUser.LoggedOffCallback>(OnLoggedOff);

        _manager.Subscribe<SteamUser.AccountInfoCallback>(OnAccountInfo);
        _manager.Subscribe<SteamFriends.FriendsListCallback>(GetFriendsList);
        _manager.Subscribe<SteamFriends.PersonaStateCallback>(OnPersonaState);

        _isRunning = true;
    }

    public async Task LoginToSteam(bool isQrCodeVerification)
    {
        _useQrCodeVerification = isQrCodeVerification;

        _remainingFetches = 5;
        _ = BeginLoginToSteam();
        var didClientFetchData = await _loginTaskCompletionSource.Task;

        if (!didClientFetchData)
            return;

        var steamData = CreateAllSteamData();
        WeakReferenceMessenger.Default.Send(new LoadingSuccessfulMessage(steamData));
    }

    private async Task BeginLoginToSteam()
    {

        Console.WriteLine("Connecting to Steam...");
        _isRunning = true;
        _steamClient.Connect();
        while (_isRunning)
            await _manager.RunWaitCallbackAsync();
    }

    public void DisconnectClient()
    {
        _steamClient?.Disconnect();
        _isRunning = false;
        _loginTaskCompletionSource.TrySetResult(false);
        Console.WriteLine("Disconnecting from Steam...");
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }

    private void CompleteCallback()
    {
        if (Interlocked.Decrement(ref _remainingFetches) == 0)
        {
            _loginTaskCompletionSource.TrySetResult(true);
        }
    }

    private async void OnConnected(SteamClient.ConnectedCallback callback)
    {
        try
        {
            Console.WriteLine("Connected to Steam! Logging in '{0}'...", _username);

            _ = _useQrCodeVerification ? SignInWithQrCode() : SignInWithDefault();
        }
        catch (Exception e)
        {
            DisconnectClient();
            Console.WriteLine(e);
        }
    }

    private async Task SignInWithQrCode()
    {
        if (_steamUser == null)
            return;

        // Start an authentication session by requesting a link
        var authSession = await _steamClient.Authentication.BeginAuthSessionViaQRAsync(
            new AuthSessionDetails()
        );

        // Steam will periodically refresh the challenge url, this callback allows you to draw a new qr code
        authSession.ChallengeURLChanged = () =>
        {
            Console.WriteLine();
            Console.WriteLine("Steam has refreshed the challenge url");

            DrawQrCode(authSession);
        };

        // Draw current qr right away
        DrawQrCode(authSession);

        var pollResponse = await authSession.PollingWaitForResultAsync();

        WeakReferenceMessenger.Default.Send(new CurrentlyLoggingInMessage());

        Console.WriteLine($"Logging in as '{pollResponse.AccountName}'...");

        // Logon to Steam with the access token we have received
        _steamUser.LogOn(
            new SteamUser.LogOnDetails
            {
                Username = pollResponse.AccountName,
                AccessToken = pollResponse.RefreshToken,
            }
        );
    }

    private async Task SignInWithDefault()
    {
        if (_steamUser == null)
            return;

        var authSession = await _steamClient.Authentication.BeginAuthSessionViaCredentialsAsync(
            new AuthSessionDetails
            {
                Username = _username,
                Password = _password,
                IsPersistentSession = ShouldRememberPassword,

                GuardData = _guardData,

                Authenticator = new UserConsoleAuthenticator(),
            }
        );

        var pollResponse = await authSession.PollingWaitForResultAsync();
        Console.WriteLine("Im in this johnson");
        if (pollResponse.NewGuardData != null)
            _guardData = pollResponse.NewGuardData;

        if (!string.IsNullOrEmpty(pollResponse.RefreshToken))
            _refreshToken = pollResponse.RefreshToken;

        _steamUser.LogOn(
            new SteamUser.LogOnDetails
            {
                Username = pollResponse.AccountName,
                AccessToken = _refreshToken,
                ShouldRememberPassword = ShouldRememberPassword,
            }
        );
    }

    void DrawQrCode(QrAuthSession authSession)
    {
        Console.WriteLine($"Challenge URL: {authSession.ChallengeURL}");
        Console.WriteLine();

        // Encode the link as a QR code
        using var qrGenerator = new QRCodeGenerator();
        var qrCodeData = qrGenerator.CreateQrCode(
            authSession.ChallengeURL,
            QRCodeGenerator.ECCLevel.L
        );

        using var pngRenderer = new PngByteQRCode(qrCodeData);
        var qrCodeImage = pngRenderer.GetGraphic(20);
        using var stream = new MemoryStream(qrCodeImage);
        WeakReferenceMessenger.Default.Send(new UpdateQrCode(new Bitmap(stream)));

        Console.WriteLine("Use the Steam Mobile App to sign in via QR code:");
    }

    private void OnDisconnected(SteamClient.DisconnectedCallback callback)
    {
        Console.WriteLine("Disconnected from Steam");
    }

    private void OnLoggedOn(SteamUser.LoggedOnCallback callback)
    {
        try
        {
            if (
                callback.Result != EResult.OK
                || _steamFriends == null
                || _steamUser == null
                || _steamUser.SteamID == null
            )
            {
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
            DisconnectClient();
            Console.WriteLine(e);
        }
    }

    private static void OnLoggedOff(SteamUser.LoggedOffCallback callback)
    {
        Console.WriteLine("Logged off of Steam: {0}", callback.Result);
    }

    private void OnAccountInfo(SteamUser.AccountInfoCallback callback)
    {
        _steamFriends?.SetPersonaState(EPersonaState.Online);
    }

    private async void OnPersonaState(SteamFriends.PersonaStateCallback callback)
    {
        try
        {
            if (_steamUser == null || _steamUser.SteamID == null)
                return;
            if (callback.FriendID == _steamUser.SteamID)
            {
                _ = GetUserProfile(callback);
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
                AvatarIcon = avatarPhoto,
            };
            _friendsDictionary.GetOrAdd(callback.FriendID, fd);
            _friendsCollection.Add(fd);
        }
        catch (Exception e)
        {
            DisconnectClient();
            Console.WriteLine(e);
        }
    }

    private async Task GetUserProfile(SteamFriends.PersonaStateCallback callback)
    {
        var newAvatarPhoto = await _steamHttpRequests.FetchUserAvatar(callback.AvatarHash);

        var ud = new UserData()
        {
            SteamID = callback.FriendID,
            ProfileName = callback.Name,
            AvatarHash = callback.AvatarHash,
            AvatarIcon = newAvatarPhoto,
        };

        Console.WriteLine("Completed user profile");
        _userData = ud;
        CompleteCallback();
    }

    private async void GetFriendsList(SteamFriends.FriendsListCallback callback)
    {
        if (_steamFriends == null || _steamUser == null)
        {
            _loginTaskCompletionSource.TrySetResult(false);
            return;
        }

        for (var x = 0; x < _steamFriends.GetFriendCount(); x++)
        {
            var steamIdFriend = _steamFriends.GetFriendByIndex(x);
            if (steamIdFriend == _steamUser.SteamID)
                continue;

            await _steamFriends.RequestProfileInfo(steamIdFriend);
        }

        Console.WriteLine("Completed friends list");
        CompleteCallback();
    }

    private async Task GetGameList()
    {
        if (_steamUser == null || _steamUser.SteamID == null)
        {
            _loginTaskCompletionSource.TrySetResult(false);
            return;
        }

        var sid = new SteamID(_steamUser.SteamID);

        var fetchedGames = await _steamHttpRequests.FetchUserGameLibrary(sid);

        if (fetchedGames == null)
        {
            _loginTaskCompletionSource.TrySetResult(false);
            return;
        }

        _gamesList = fetchedGames;
        Console.WriteLine("Completed game list");

        CompleteCallback();
    }

    private async Task GetBadgesAndLevels()
    {
        if (_steamUser == null || _steamUser.SteamID == null)
        {
            _loginTaskCompletionSource.TrySetResult(false);
            return;
        }
        var sid = new SteamID(_steamUser.SteamID);

        var fetchedBadgesAndLevels = await _steamHttpRequests.FetchUserBadgesAndLevels(sid);

        if (fetchedBadgesAndLevels == null)
        {
            _loginTaskCompletionSource.TrySetResult(false);
            return;
        }

        _badgesAndLevels = fetchedBadgesAndLevels;

        Console.WriteLine("Completed badges and levels");
        CompleteCallback();
    }

    private async Task GetRecentlyPlayedGames()
    {
        if (_steamUser == null || _steamUser.SteamID == null)
        {
            _loginTaskCompletionSource.TrySetResult(false);
            return;
        }

        var sid = new SteamID(_steamUser.SteamID);

        var recentlyPlayed = await _steamHttpRequests.FetchRecentlyPlayedGames(sid);

        if (recentlyPlayed == null)
        {
            _loginTaskCompletionSource.TrySetResult(false);
            return;
        }

        List<int> gameIDs = [];
        if (recentlyPlayed.Response?.Games != null)
        {
            gameIDs = [.. recentlyPlayed.Response.Games.Select(game => game.AppId)];
        }

        _recentlyPlayedGamesResponse = gameIDs;

        Console.WriteLine("Completed recently played games");
        CompleteCallback();
    }

    private AllSteamData CreateAllSteamData()
    {
        var friendViewModels = new ObservableCollection<AuxiliaryFriendViewModel>();
        var gameViewModels = new ObservableCollection<AuxiliaryGameViewModel>();
        var recentlyPlayedGamesViewModels = new ObservableCollection<RenderedSteamGame>();

        foreach (var f in _friendsCollection)
        {
            friendViewModels.Add(new AuxiliaryFriendViewModel(f));
        }
        foreach (var g in _gamesList)
        {
            gameViewModels.Add(new AuxiliaryGameViewModel(g.Value));
        }

        foreach (var recentIds in _recentlyPlayedGamesResponse)
        {
            if (_gamesList.TryGetValue(recentIds, out var game))
                recentlyPlayedGamesViewModels.Add(game);
        }

        return new AllSteamData()
        {
            CurrentUser = _userData,
            Friends = _friendsCollection,
            Games = _gamesList,
            FriendVm = friendViewModels,
            GameVm = gameViewModels,
            BadgesAndLevels = _badgesAndLevels,
            RecentGames = recentlyPlayedGamesViewModels,
        };
    }
}

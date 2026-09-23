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
using KeySharp;
using QRCoder;
using SteamAccountUtility.Messages;
using SteamAccountUtility.Models;
using SteamAccountUtility.ViewModels;
using SteamKit2;
using SteamKit2.Authentication;

namespace SteamAccountUtility.Services;

public class SteamLogin : IDisposable
{
    private const bool ShouldRememberPassword = true;
    private string? _username;
    private string? _password;
    private string? _refreshToken;

    // private string? _guardData;

    private readonly ConcurrentDictionary<SteamID, FriendData> _friendsDictionary = new();

    private bool _isRunning;

    private readonly CallbackManager _manager;
    private readonly SteamClient _steamClient;
    private readonly SteamHttpRequests _steamHttpRequests;

    private SteamFriends? _steamFriends;
    private SteamUser? _steamUser;

    private int _remainingFetches;
    private TaskCompletionSource<bool> _loginTaskCompletionSource;

    private int _errorChecker;
    private TaskCompletionSource<bool> _errorCheckTaskCompletionSource;

    private UserData _userData = new();
    private readonly ObservableCollection<FriendData> _friendsCollection = [];
    private Dictionary<int, RenderedSteamGame> _gamesList = [];
    private BadgesAndLevelsResponse _badgesAndLevels = new();
    private List<int> _recentlyPlayedGamesResponse = [];

    private CancellationTokenSource _cancellationSource;
    private SteamLoginType _currentLoginType = SteamLoginType.Default;
    private CancellationToken _token;

    public void GetCredentials(string? username, string? password)
    {
        _username = username;
        _password = password;
    }

    public SteamLogin(string serverAddress)
    {
        _cancellationSource = new CancellationTokenSource();
        _token = _cancellationSource.Token;
        _loginTaskCompletionSource = new TaskCompletionSource<bool>();
        _errorCheckTaskCompletionSource = new TaskCompletionSource<bool>();

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
    }

    public async Task LoginToSteam(SteamLoginType lt)
    {
        try
        {
            _cancellationSource = new CancellationTokenSource();
            _token = _cancellationSource.Token;
            _loginTaskCompletionSource = new TaskCompletionSource<bool>();
            _errorCheckTaskCompletionSource = new TaskCompletionSource<bool>();

            _steamClient.Disconnect();
            _currentLoginType = lt;
            _isRunning = true;

            _remainingFetches = 5;
            _errorChecker = 1;
            Console.WriteLine("Connecting to Steam...");

            _steamClient.Connect();
            while (_isRunning && !_token.IsCancellationRequested)
                await _manager.RunWaitCallbackAsync(_token);
        }
        catch (Exception e)
        {
            TerminateClient();
            Console.WriteLine(e);
        }
    }

    public void TerminateClient()
    {
        if (Interlocked.Decrement(ref _errorChecker) == 0)
        {
            _cancellationSource.Cancel();
            _steamClient.Disconnect();
            _isRunning = false;
            _loginTaskCompletionSource.TrySetResult(false);
            switch (_currentLoginType)
            {
                case SteamLoginType.Default:
                    WeakReferenceMessenger.Default.Send(new GoToLoginScreen("Error logging in"));
                    break;
                case SteamLoginType.RefreshToken:
                    WeakReferenceMessenger.Default.Send(new GoToLoginScreen(""));
                    break;
                case SteamLoginType.QrCode:
                    WeakReferenceMessenger.Default.Send(new RefreshQrCodeLogin());
                    break;
            }
        }

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
            _isRunning = false;
        }
        Console.Write("Fetches remaining: {0}", _remainingFetches);
    }

    private async void OnConnected(SteamClient.ConnectedCallback callback)
    {
        try
        {
            Console.WriteLine("Connected to Steam! Logging in '{0}'...", _username);
            switch (_currentLoginType)
            {
                case SteamLoginType.Default:
                    _ = SignInWithDefault();
                    break;
                case SteamLoginType.QrCode:
                    _ = SignInWithQrCode();
                    break;
                case SteamLoginType.RefreshToken:
                    SignInWithRefreshToken();
                    break;
                default:
                    throw new Exception();
            }
        }
        catch (Exception e)
        {
            TerminateClient();
            Console.WriteLine(e);
        }
    }

    private void SignInWithRefreshToken()
    {
        try
        {
            _username = Keyring.GetPassword("SteamAccountUtility", "Steam", "username");
            _refreshToken = Keyring.GetPassword("SteamAccountUtility", "Steam", "refreshToken");
        }
        catch (KeyringException ex)
        {
            Console.Error.WriteLine(ex);
            TerminateClient();
            return;
        }

        _steamUser?.LogOn(
            new SteamUser.LogOnDetails
            {
                Username = _username,
                AccessToken = _refreshToken,
                ShouldRememberPassword = ShouldRememberPassword,
            }
        );
    }

    private async Task SignInWithQrCode()
    {
        try
        {
            if (_steamUser == null)
            {
                TerminateClient();
                return;
            }

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

            var pollResponse = await authSession.PollingWaitForResultAsync(_token);
            _username = pollResponse.AccountName;
            _refreshToken = pollResponse.RefreshToken;
            WeakReferenceMessenger.Default.Send(new CurrentlyLoggingInMessage());

            Console.WriteLine($"Logging in as '{pollResponse.AccountName}'...");

            // Logon to Steam with the access token we have received
            _steamUser.LogOn(
                new SteamUser.LogOnDetails
                {
                    Username = _username,
                    AccessToken = _refreshToken,
                    ShouldRememberPassword = ShouldRememberPassword,
                }
            );
        }
        catch (Exception e)
        {
            TerminateClient();
            Console.WriteLine(e);
        }
    }

    private async Task SignInWithDefault()
    {
        try
        {
            if (_steamUser == null)
            {
                TerminateClient();
                return;
            }

            var authSession = await _steamClient.Authentication.BeginAuthSessionViaCredentialsAsync(
                new AuthSessionDetails
                {
                    Username = _username,
                    Password = _password,
                    IsPersistentSession = ShouldRememberPassword,

                    // GuardData = _guardData,

                    Authenticator = new SteamKit2Authenticator(),
                }
            );

            var pollResponse = await authSession.PollingWaitForResultAsync(_token);
            // if (pollResponse.NewGuardData != null)
            //     _guardData = pollResponse.NewGuardData;

            _username = pollResponse.AccountName;
            _refreshToken = pollResponse.RefreshToken;

            _steamUser.LogOn(
                new SteamUser.LogOnDetails
                {
                    Username = _username,
                    AccessToken = _refreshToken,
                    ShouldRememberPassword = ShouldRememberPassword,
                }
            );
        }
        catch (Exception e)
        {
            TerminateClient();
            Console.WriteLine(e);
        }
    }

    private void DrawQrCode(QrAuthSession authSession)
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

    private async void OnLoggedOn(SteamUser.LoggedOnCallback callback)
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
                TerminateClient();
                return;
            }

            Keyring.SetPassword("SteamAccountUtility", "Steam", "username", _username);
            Keyring.SetPassword("SteamAccountUtility", "Steam", "refreshToken", _refreshToken);

            Console.WriteLine("Successfully logged on!");

            _loginTaskCompletionSource = new TaskCompletionSource<bool>();

            _ = _steamFriends.RequestProfileInfo(_steamUser.SteamID);
            _ = GetGameList();
            _ = GetBadgesAndLevels();
            _ = GetRecentlyPlayedGames();

            var didClientFetchData = await _loginTaskCompletionSource.Task;
            if (!didClientFetchData)
            {
                TerminateClient();
                return;
            }

            Console.WriteLine(
                "Successfully fetched all Steam Data. Attempting to send data to MainWindow . . ."
            );
            var steamData = CreateAllSteamData();
            WeakReferenceMessenger.Default.Send(new LoadingSuccessfulMessage(steamData));
        }
        catch (Exception e)
        {
            TerminateClient();
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
            {
                TerminateClient();
                return;
            }
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
            TerminateClient();
            Console.WriteLine(e);
        }
    }

    private async Task GetUserProfile(SteamFriends.PersonaStateCallback callback)
    {
        try
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
        catch (Exception e)
        {
            TerminateClient();
            Console.WriteLine(e);
        }
    }

    private async void GetFriendsList(SteamFriends.FriendsListCallback callback)
    {
        try
        {
            if (_steamFriends == null || _steamUser == null)
            {
                TerminateClient();
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
        catch (Exception e)
        {
            TerminateClient();
            Console.WriteLine(e);
        }
    }

    private async Task GetGameList()
    {
        try
        {
            if (_steamUser == null || _steamUser.SteamID == null)
            {
                TerminateClient();
                return;
            }

            var sid = new SteamID(_steamUser.SteamID);

            var fetchedGames = await _steamHttpRequests.FetchUserGameLibrary(sid);

            if (fetchedGames == null)
            {
                TerminateClient();
                return;
            }

            _gamesList = fetchedGames;
            Console.WriteLine("Completed game list");

            CompleteCallback();
        }
        catch (Exception e)
        {
            TerminateClient();
            Console.WriteLine(e);
        }
    }

    private async Task GetBadgesAndLevels()
    {
        try
        {
            if (_steamUser == null || _steamUser.SteamID == null)
            {
                TerminateClient();
                return;
            }

            var sid = new SteamID(_steamUser.SteamID);

            var fetchedBadgesAndLevels = await _steamHttpRequests.FetchUserBadgesAndLevels(sid);

            if (fetchedBadgesAndLevels == null)
            {
                TerminateClient();
                return;
            }

            _badgesAndLevels = fetchedBadgesAndLevels;

            Console.WriteLine("Completed badges and levels");
            CompleteCallback();
        }
        catch (Exception e)
        {
            TerminateClient();
            Console.WriteLine(e);
        }
    }

    private async Task GetRecentlyPlayedGames()
    {
        try
        {
            if (_steamUser == null || _steamUser.SteamID == null)
            {
                TerminateClient();
                return;
            }

            var sid = new SteamID(_steamUser.SteamID);

            var recentlyPlayed = await _steamHttpRequests.FetchRecentlyPlayedGames(sid);

            if (recentlyPlayed == null)
            {
                TerminateClient();
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
        catch (Exception e)
        {
            TerminateClient();
            Console.WriteLine(e);
        }
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

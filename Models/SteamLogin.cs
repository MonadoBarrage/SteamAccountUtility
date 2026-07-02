using System;
using System.Collections.Concurrent;
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

    public void GetCredentials(string username, string password, string? steamkey, string? guarddata, string? accesstoken)
    {
        _username = username;
        _password = password;
        _steamKey = steamkey;
        _previouslyStoredGuardData = guarddata;
        _refreshToken = accesstoken;
    }
    
    private string? _username;
    private string? _password;
    private string? _steamKey;
    
    private string? _previouslyStoredGuardData;
    private string? _refreshToken;
    
    private readonly ConcurrentDictionary<SteamID, FriendData> _friendsList = new();

    private bool _isRunning;

    private CallbackManager? _manager;
    private SteamClient? _steamClient;
    private SteamFriends? _steamFriends;
    private SteamUser? _steamUser;

    private int _numberOfFriends;
    private int _processedFriends;
    private readonly HttpClient _httpClient = new HttpClient();


    public void Dispose()
    {
        throw new NotImplementedException();
    }
    

    public async Task InitializeClient(bool useAutoLogin = false)
    {
        _steamClient = new SteamClient();

        _manager = new CallbackManager(_steamClient);

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

#if DEBUG
        Console.WriteLine("Connecting to Steam...");
#endif

        _steamClient.Connect();

        while (_isRunning)

            await _manager.RunWaitCallbackAsync();
    }

    private async void OnConnected(SteamClient.ConnectedCallback callback)
    {
        try
        {
#if DEBUG
            Console.WriteLine("Connected to Steam! Logging in '{0}'...", _username);
#endif

            
            const bool shouldRememberPassword = true;
            
            if (!string.IsNullOrEmpty(_refreshToken) && ParseRefreshToken(_refreshToken))
            {
                
                WeakReferenceMessenger.Default.Send(new SaveForAutoLogin(_previouslyStoredGuardData,
                    _refreshToken));
                
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
                new UpdateLoginMessage("Use the Steam Guard App to approve this login"));

            if (_steamClient == null || _steamUser == null) 
            {
                return;
            }

            var authSession = await _steamClient.Authentication.BeginAuthSessionViaCredentialsAsync(
                    new AuthSessionDetails
                    {
                        Username = _username,
                        Password = _password,
                        IsPersistentSession = shouldRememberPassword,

                        GuardData = _previouslyStoredGuardData,

                        Authenticator = new UserConsoleAuthenticator()
                    });

                var pollResponse = await authSession.PollingWaitForResultAsync();

                if (pollResponse.NewGuardData != null)
                    // When using certain two factor methods (such as email 2fa), guard data may be provided by Steam
                    // for use in future authentication sessions to avoid triggering 2FA again (this works similarly to the old sentry file system).
                    // Do note that this guard data is also a JWT token and has an expiration date.
                    _previouslyStoredGuardData = pollResponse.NewGuardData;

                WeakReferenceMessenger.Default.Send(new SaveForAutoLogin(_previouslyStoredGuardData,
                    pollResponse.RefreshToken));


                _steamUser.LogOn(new SteamUser.LogOnDetails
                {
                    Username = pollResponse.AccountName,
                    AccessToken = pollResponse.RefreshToken,
                    ShouldRememberPassword =
                        shouldRememberPassword // If you set IsPersistentSession to true, this also must be set to true for it to work correctly
                });

            
            // ParseJsonWebToken(pollResponse.AccessToken, nameof(pollResponse.AccessToken));
            // ParseJsonWebToken(pollResponse.RefreshToken, nameof(pollResponse.RefreshToken));
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    private void OnDisconnected(SteamClient.DisconnectedCallback callback)
    {
#if DEBUG
        Console.WriteLine("Disconnected from Steam");
#endif
        _isRunning = false;
    }

    private async void OnLoggedOn(SteamUser.LoggedOnCallback callback)
    {
        try
        {
            if (callback.Result != EResult.OK)
            {
#if DEBUG
                Console.WriteLine("Unable to logon to Steam: {0} / {1}", callback.Result, callback.ExtendedResult);
#endif
                _isRunning = false;
                return;
            }

#if DEBUG
            Console.WriteLine("Successfully logged on!");
#endif

            if (_steamFriends == null || _steamUser == null || _steamUser.SteamID == null) return;
            await _steamFriends.RequestProfileInfo(_steamUser.SteamID);

            await FetchGameList();
        }
        catch (Exception e)
        {
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
        // before being able to interact with friends, you must wait for the account info callback
        // this callback is posted shortly after a successful logon

        // at this point, we can go online on friends, so lets do that0
        _steamFriends?.SetPersonaState(EPersonaState.Online);
    }


    private async void OnFriendsList(SteamFriends.FriendsListCallback callback)
    {
        try
        {
            if (_steamFriends == null || _steamUser == null) return;

            _numberOfFriends = _steamFriends.GetFriendCount();

#if DEBUG
            Console.WriteLine("We have {0} friends", _numberOfFriends);
#endif
            for (var x = 0; x < _numberOfFriends; x++)
            {
                var steamIdFriend = _steamFriends.GetFriendByIndex(x);
                if (steamIdFriend == _steamUser.SteamID) continue;
                await _steamFriends.RequestProfileInfo(steamIdFriend);

            }
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
#if DEBUG
                Console.WriteLine("My id: {0}", _steamUser.SteamID);
#endif
                var userConvertedByteArray = ConvertByteArrayToString(callback.AvatarHash);
                var userNewLink = "https://avatars.fastly.steamstatic.com/"
                                  + userConvertedByteArray
                                  + "_full.jpg";

                var userNewPhoto = await LoadImageAsync(userNewLink);

                var ud = new UserData()
                {
                    SteamID = callback.FriendID,
                    ProfileName = callback.Name,
                    AvatarHash = callback.AvatarHash,
                    AvatarIcon = userNewPhoto

                };

                WeakReferenceMessenger.Default.Send(new ReceiveUserData(ud));
                return;
            }

            if (_friendsList.ContainsKey(callback.FriendID))
            {
                return;
            }


            var convertedByteArray = ConvertByteArrayToString(callback.AvatarHash);
            var newLink = "https://avatars.fastly.steamstatic.com/"
                          + convertedByteArray
                          + "_full.jpg";

            Bitmap? newPhoto = await LoadImageAsync(newLink);

            FriendData fd = new FriendData()
            {
                SteamID = callback.FriendID,
                ProfileName = callback.Name,
                AvatarHash = callback.AvatarHash,
                AvatarIcon = newPhoto
            };
            _friendsList.GetOrAdd(callback.FriendID, fd);
            ++_processedFriends;
            if (_processedFriends == _numberOfFriends)
            {
#if DEBUG
                Console.WriteLine("Received Friends List");
#endif

                WeakReferenceMessenger.Default.Send(new ReceiveFriendsList(_friendsList));
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    private async Task FetchGameList()
    {
        if (_steamUser == null || _steamUser.SteamID == null) return;
        
        
        var sid = new SteamID(_steamUser.SteamID);
        var requestLink = "https://api.steampowered.com/IPlayerService/GetOwnedGames/v1/?key=" +
                          _steamKey +
                          "&steamid=" +
                          sid.ConvertToUInt64() +
                          "&include_appinfo=1";
#if DEBUG
        Console.WriteLine($"Fetching {requestLink}");
#endif

        using var response = await _httpClient.GetAsync(requestLink);

        response.EnsureSuccessStatusCode();


        var jsonResponse = await response.Content.ReadAsStringAsync();

        var obj = JsonSerializer.Deserialize<SteamGameHttpRequest>(jsonResponse);
        if (obj == null)
        {
#if DEBUG
            Console.WriteLine("No Gamelist");
#endif
            return;
        }

        var games = obj.Response.Games;
        foreach (var g in games)
        {
            var newUri = "https://shared.fastly.steamstatic.com/store_item_assets/steam/apps/"
                         + g.AppId
                         + "/library_600x900.jpg";
            var newUri2 = "https://shared.fastly.steamstatic.com/store_item_assets/steam/apps/"
                          + g.AppId
                          + "/"
                          + g.ImgIconUrl
                          + "/library_600x900.jpg";
            g.AppIcon = await LoadImageAsync(newUri, newUri2);
        }

        WeakReferenceMessenger.Default.Send(new ReceiveGameList(games));
    }


    private static bool CheckUnixTimestamp(long? expirationTimestamp)
    {
        if (expirationTimestamp == null) return false;
        return DateTimeOffset.UtcNow.ToUnixTimeSeconds() < (expirationTimestamp - 604800);
    }


    private bool ParseRefreshToken(string token)
    {
        var tokenComponents = token.Split('.');

        var base64 = tokenComponents[1].Replace('-', '+').Replace('_', '/');

        if (base64.Length % 4 != 0) base64 += new string('=', 4 - base64.Length % 4);

        var payloadBytes = Convert.FromBase64String(base64);
        
        var newTokenJson = JsonSerializer.Deserialize<RefreshTokenJson>(payloadBytes);
        
        #if DEBUG
            Console.WriteLine(JsonSerializer.Serialize(newTokenJson));
        #endif
        
        return (newTokenJson != null) && CheckUnixTimestamp(newTokenJson.Expiration);
    }

    private static string ConvertByteArrayToString(byte[]? hash)
    {
        
        return (hash == null ? "" : BitConverter.ToString(hash).Replace("-", "").ToLower());
    }
    
    private async Task<Bitmap?> LoadImageAsync(string? imageUrl, string? secondaryImageUrl = null)
    {
        if (string.IsNullOrEmpty(imageUrl)) return null;
        try
        {
            var bytes = await _httpClient.GetByteArrayAsync(imageUrl);
            using var stream = new MemoryStream(bytes);
            return new Bitmap(stream);
        }
        catch (Exception)
        {
            if(string.IsNullOrEmpty(secondaryImageUrl)) return null;
        }

        try
        {
            Console.WriteLine("Attempted new image: {0}",secondaryImageUrl);
            var bytes = await _httpClient.GetByteArrayAsync(secondaryImageUrl);
            using var stream = new MemoryStream(bytes);
            return new Bitmap(stream);
        }
        catch (Exception)
        {
            return null;
        }

    }
}
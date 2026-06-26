using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging;
using SteamAccountUtility.Messages;
using SteamKit2;
using SteamKit2.Authentication;
using SteamKit2.Internal;
using SteamAccountUtility;

internal sealed class SteamLogin : IDisposable
{
    
    
    private string _password;
    private string _previouslyStoredGuardData;
    private string _username;
    private string _steamKey;
    public readonly Dictionary<string,string> FriendsList = new();
    
    private bool isRunning;

    private string LoginFilePath = "";
    private CallbackManager manager;
    private SteamClient steamClient;
    private SteamFriends steamFriends;
    private SteamUser steamUser;
    private SteamUserStats steamUserStats;
    private SteamApps steamApps;
    

    private int NumberOfFriends;
    private int processedFriends = 0;
    private HttpClient httpClient = new()
    {
        BaseAddress = new Uri("https://api.steampowered.com")
    };

    
    public void Dispose()
    {
        throw new NotImplementedException();
    }

    public void GetCredentials(string username, string password, string steamkey)
    {
        _username = username;
        _password = password;
        _steamKey = steamkey;
    }
    
    
    public async Task InitializeClient()
    {
        steamClient = new SteamClient();

        manager = new CallbackManager(steamClient);

        steamUser = steamClient.GetHandler<SteamUser>();
        steamFriends = steamClient.GetHandler<SteamFriends>();
        steamUserStats = steamClient.GetHandler<SteamUserStats>();
        steamApps = steamClient.GetHandler<SteamApps>();
        
        manager.Subscribe<SteamClient.ConnectedCallback>(OnConnected);
        manager.Subscribe<SteamClient.DisconnectedCallback>(OnDisconnected);

        manager.Subscribe<SteamUser.LoggedOnCallback>(OnLoggedOn);
        manager.Subscribe<SteamUser.LoggedOffCallback>(OnLoggedOff);

        manager.Subscribe<SteamUser.AccountInfoCallback>(OnAccountInfo);
        manager.Subscribe<SteamFriends.FriendsListCallback>(OnFriendsList);
        manager.Subscribe<SteamFriends.PersonaStateCallback>(OnPersonaState);
        
        
        isRunning = true;

        #if DEBUG
        Console.WriteLine("Connecting to Steam...");
        #endif
        
        steamClient.Connect();

        while (isRunning)

            // manager.RunWaitCallbacks(TimeSpan.FromSeconds(1));
            await manager.RunWaitCallbackAsync();
    }

    private async void OnConnected(SteamClient.ConnectedCallback callback)
    {
        try
        {
            #if DEBUG
                Console.WriteLine("Connected to Steam! Logging in '{0}'...", _username);
            #endif
            
            var shouldRememberPassword = true;

            WeakReferenceMessenger.Default.Send(new UpdateLoginMessage("Use the Steam Guard App to approve this login"));
            
            var authSession = await steamClient.Authentication.BeginAuthSessionViaCredentialsAsync(
                new AuthSessionDetails
                {
                    Username = _username,
                    Password = _password,
                    IsPersistentSession = shouldRememberPassword,

                    GuardData = _previouslyStoredGuardData,

                    // <see cref="UserConsoleAuthenticator"/> is the default authenticator implemention provided by SteamKit
                    // for ease of use which blocks the thread and asks for user input to enter the code.
                    // However, if you require special handling (e.g. you have the TOTP secret and can generate codes on the fly),
                    // you can implement your own <see cref="SteamKit2.Authentication.IAuthenticator"/>.
                    Authenticator = new UserConsoleAuthenticator()
                });

            var pollResponse = await authSession.PollingWaitForResultAsync();

            if (pollResponse.NewGuardData != null)
                // When using certain two factor methods (such as email 2fa), guard data may be provided by Steam
                // for use in future authentication sessions to avoid triggering 2FA again (this works similarly to the old sentry file system).
                // Do note that this guard data is also a JWT token and has an expiration date.
                _previouslyStoredGuardData = pollResponse.NewGuardData;

            // Logon to Steam with the access token we have received
            // Note that we are using RefreshToken for logging on here
            steamUser.LogOn(new SteamUser.LogOnDetails
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
            
        }
    }

    private void OnDisconnected(SteamClient.DisconnectedCallback callback)
    {
        #if DEBUG
            Console.WriteLine("Disconnected from Steam");
        #endif
        isRunning = false;
    }

    private async void OnLoggedOn(SteamUser.LoggedOnCallback callback)
    {
        if (callback.Result != EResult.OK)
        {
            #if DEBUG
                Console.WriteLine("Unable to logon to Steam: {0} / {1}", callback.Result, callback.ExtendedResult);
            #endif
            isRunning = false;
            return;
        }
        
        #if DEBUG
        Console.WriteLine("Successfully logged on!");
        #endif
        await steamFriends.RequestProfileInfo(steamUser.SteamID);

        await FetchGameList();

        // at this point, we'd be able to perform actions on Steam

        // for this sample we'll just log off
        // steamUser.LogOff();
    }

    private void OnLoggedOff(SteamUser.LoggedOffCallback callback)
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
        
        steamFriends.SetPersonaState(EPersonaState.Online);
    }


    private async void OnFriendsList(SteamFriends.FriendsListCallback callback)
    {
        // at this point, the client has received it's friends list
        
        NumberOfFriends = steamFriends.GetFriendCount();

        #if DEBUG
        Console.WriteLine("We have {0} friends", NumberOfFriends);
        #endif
        for (var x = 0; x < NumberOfFriends; x++)
        {
            // steamids identify objects that exist on the steam network, such as friends, as an example
            var steamIdFriend = steamFriends.GetFriendByIndex(x);
            if (steamIdFriend == steamUser.SteamID) continue;
            var friendName = await steamFriends.RequestProfileInfo(steamIdFriend);
            
            // we'll just display the STEAM_ rendered version
            // Console.WriteLine( "Friend: {0}", steamIdFriend.Render() );
            // FriendsList.Add(steamFriends.RequestProfileInfo(steamIdFriend).ToString());
            // Console.WriteLine( steamIdFriend. );
        }
        
        
        
        // // we can also iterate over our friendslist to accept or decline any pending invites
        //
        // foreach ( var friend in callback.FriendList )
        // {
        //     if (friend.Relationship == EFriendRelationship.RequestRecipient)
        //     {
        //         // this user has added us, let's add him back
        //         steamFriends.AddFriend(friend.SteamID);
        //     }
        // }
    }

    private void OnPersonaState(SteamFriends.PersonaStateCallback callback)
    {
        // Console.WriteLine( "PersonaState: {0}", callback.Name );
        if (callback.FriendID == steamUser.SteamID)
        {
            #if DEBUG
                Console.WriteLine("My id: {0}",steamUser.SteamID);
            #endif
            WeakReferenceMessenger.Default.Send(new ReceiveProfileName(callback.Name));
            return;
        }

        if (FriendsList.ContainsKey(callback.FriendID.ToString()))
        {
            return;
        }
        FriendsList.Add(callback.FriendID.ToString(),callback.Name);
        ++processedFriends;
        if (processedFriends == NumberOfFriends)
        {
            #if DEBUG
                Console.WriteLine("Received Friends List");
            #endif
            
            WeakReferenceMessenger.Default.Send(new ReceiveFriendsList(FriendsList));
        }
    }

    private async Task FetchGameList()
    {
        var sid = new SteamKit2.SteamID(steamUser.SteamID);
        var requestLink = "IPlayerService/GetOwnedGames/v1/?key=" +
                          _steamKey +
                          "&steamid=" +
                          sid.ConvertToUInt64() +
                          "&include_appinfo=1";
        #if DEBUG
        Console.WriteLine($"Fetching {requestLink}");
        #endif
        
        using HttpResponseMessage response = await httpClient.GetAsync(requestLink);
        
        response.EnsureSuccessStatusCode();
        
        
        var jsonResponse = await response.Content.ReadAsStringAsync();

        var obj = JsonSerializer.Deserialize<SteamGameHTTPRequest>(jsonResponse);
        if (obj == null)
        {
            #if DEBUG
                Console.WriteLine("No Gamelist");
            #endif
            return;
        }

        
        WeakReferenceMessenger.Default.Send(new ReceiveGameList(obj.Response.Games));
    }
    







    // This is simply showing how to parse JWT, this is not required to login to Steam
    private void ParseJsonWebToken(string token, string name)
    {
        // You can use a JWT library to do the parsing for you
        var tokenComponents = token.Split('.');

        // Fix up base64url to normal base64
        var base64 = tokenComponents[1].Replace('-', '+').Replace('_', '/');

        if (base64.Length % 4 != 0) base64 += new string('=', 4 - base64.Length % 4);

        var payloadBytes = Convert.FromBase64String(base64);

        // Payload can be parsed as JSON, and then fields such expiration date, scope, etc can be accessed
        var payload = JsonDocument.Parse(payloadBytes);

        // For brevity, we will simply output formatted json to console
        var formatted = JsonSerializer.Serialize(payload, new JsonSerializerOptions
        {
            WriteIndented = true
        });
        Console.WriteLine($"{name}: {formatted}");
        Console.WriteLine();
    }
}
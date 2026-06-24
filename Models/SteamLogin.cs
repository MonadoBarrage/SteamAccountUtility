using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using SteamKit2;
using SteamKit2.Authentication;

public record LoginDetails
{
    public string Username { get; set; }
    public string Password { get; set; }
    public string PreviouslyStoredGuardData { get; set; }
}

internal sealed class SteamLogin : IDisposable
{
    private string _password;
    private string _previouslyStoredGuardData;
    private string _username;
    private readonly List<string> FriendsList = new();
    private bool isRunning;

    private string LoginFilePath = "";
    private CallbackManager manager;
    private string ProfileName;
    private SteamClient steamClient;
    private SteamFriends steamFriends;
    private SteamUser steamUser;
    private SteamUserStats steamUserStats;

    public void Dispose()
    {
        throw new NotImplementedException();
    }

    public string GetName()
    {
        return steamFriends.GetPersonaName();
    }

    public List<string> GetFriendName()
    {
        return FriendsList;
    }

    // public void FetchFilePath(string path)
    // {
    //     LoginFilePath = path;
    // }
    //
    // public void FetchCredentials()
    // {
    //     LoginDetails? loginDetails;
    //
    //     var jsonString = File.ReadAllText(LoginFilePath);
    //
    //     // string? jsonString = File.ReadAllText(filename);
    //     if (!string.IsNullOrWhiteSpace(jsonString))
    //         loginDetails = JsonSerializer.Deserialize<LoginDetails>(jsonString);
    //     else return;
    //     _username = loginDetails.Username;
    //     _password = loginDetails.Password;
    //     _previouslyStoredGuardData = loginDetails.PreviouslyStoredGuardData;
    // }

    // public void SaveCredentials()
    // {
    //     // LoginDetails loginDetails = new LoginDetails{Username = _username, Password = _password, PreviouslyStoredGuardData = _previouslyStoredGuardData};
    //     // string jsonString = JsonSerializer.Serialize(loginDetails);
    //     // byte[] dataBytes = Encoding.UTF8.GetBytes(jsonString);
    //     // stream.Write(dataBytes, 0, dataBytes.Length);
    //     // stream.Flush();
    //     var loginDetails = new LoginDetails
    //         { Username = _username, Password = _password, PreviouslyStoredGuardData = _previouslyStoredGuardData };
    //
    //     Console.WriteLine(JsonSerializer.Serialize(loginDetails));
    //     var writer = new StreamWriter(LoginFilePath);
    //     writer.Write(JsonSerializer.Serialize(loginDetails));
    //     writer.Flush();
    //     writer.Close();
    // }

    public void GetCredentials(string username, string password)
    {
        _username = username;
        _password = password;
    }
    
    public async Task InitializeClient()
    {
        steamClient = new SteamClient();

        manager = new CallbackManager(steamClient);

        steamUser = steamClient.GetHandler<SteamUser>();
        steamFriends = steamClient.GetHandler<SteamFriends>();
        steamUserStats = steamClient.GetHandler<SteamUserStats>();
        
        manager.Subscribe<SteamClient.ConnectedCallback>(OnConnected);
        manager.Subscribe<SteamClient.DisconnectedCallback>(OnDisconnected);

        manager.Subscribe<SteamUser.LoggedOnCallback>(OnLoggedOn);
        manager.Subscribe<SteamUser.LoggedOffCallback>(OnLoggedOff);

        manager.Subscribe<SteamUser.AccountInfoCallback>(OnAccountInfo);
        manager.Subscribe<SteamFriends.FriendsListCallback>(OnFriendsList);
        manager.Subscribe<SteamFriends.PersonaStateCallback>(OnPersonaState);


        isRunning = true;

        Console.WriteLine("Connecting to Steam...");

        steamClient.Connect();

        while (isRunning)

            // manager.RunWaitCallbacks(TimeSpan.FromSeconds(1));
            await manager.RunWaitCallbackAsync();
    }

    private async void OnConnected(SteamClient.ConnectedCallback callback)
    {
        try
        {
            Console.WriteLine("Connected to Steam! Logging in '{0}'...", _username);

            var shouldRememberPassword = true;

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
        Console.WriteLine("Disconnected from Steam");

        isRunning = false;
    }

    private void OnLoggedOn(SteamUser.LoggedOnCallback callback)
    {
        if (callback.Result != EResult.OK)
        {
            Console.WriteLine("Unable to logon to Steam: {0} / {1}", callback.Result, callback.ExtendedResult);

            isRunning = false;
            return;
        }

        Console.WriteLine("Successfully logged on!");

        // at this point, we'd be able to perform actions on Steam

        // for this sample we'll just log off
        // steamUser.LogOff();
    }

    private void OnLoggedOff(SteamUser.LoggedOffCallback callback)
    {
        Console.WriteLine("Logged off of Steam: {0}", callback.Result);
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

        var friendCount = steamFriends.GetFriendCount();

        Console.WriteLine("We have {0} friends", friendCount);

        for (var x = 0; x < friendCount; x++)
        {
            // steamids identify objects that exist on the steam network, such as friends, as an example
            var steamIdFriend = steamFriends.GetFriendByIndex(x);
            if (steamIdFriend == steamUser.SteamID) continue;
            await steamFriends.RequestProfileInfo(steamIdFriend);
            // we'll just display the STEAM_ rendered version
            // Console.WriteLine( "Friend: {0}", steamIdFriend.Render() );
            // FriendsList.Add(steamFriends.RequestProfileInfo(steamIdFriend).ToString());
            // Console.WriteLine( steamIdFriend. );
        }

        steamUser.LogOff();
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
        FriendsList.Add(callback.Name);
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
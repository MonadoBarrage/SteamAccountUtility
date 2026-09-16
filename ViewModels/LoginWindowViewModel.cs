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
    [ObservableProperty] private string? _errorMessage;
    
    [ObservableProperty] private bool _isLoginButtonEnabled;

    private readonly string _guardData = "";
    private readonly string _accessToken = "";
    private readonly SteamLogin _steamLogin;

    
    
    private int _remainingFetches = 5;

    private readonly TaskCompletionSource<bool> _loginTaskCompletionSource;
    
    public LoginWindowViewModel(string serverAddress)
    {
        _steamLogin = new SteamLogin(serverAddress);
        _loginTaskCompletionSource = new TaskCompletionSource<bool>();
        _isLoginButtonEnabled = true;
        
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
        ErrorMessage = "Logging in...";
        // LoadingMessage = "Logging in...";
        
        _steamLogin.GetCredentials(Username, Password, _guardData, _accessToken);
        
        var steamData = await _steamLogin.LoginToSteam();
        Console.WriteLine("GOTG HERE");
        if (steamData == null)
        {
            Console.WriteLine("Error processing");
            ErrorMessage = "Error processing request";
            IsLoginButtonEnabled = true;
            return;
        }
        
       
        WeakReferenceMessenger.Default.Send(new GoToHomePageMessage(steamData));

    }
    private static bool ParseRefreshToken(string token)
    {
        var tokenComponents = token.Split('.');

        var base64 = tokenComponents[1].Replace('-', '+').Replace('_', '/');

        if (base64.Length % 4 != 0) base64 += new string('=', 4 - base64.Length % 4);

        var payloadBytes = Convert.FromBase64String(base64);
        
        var newTokenJson = JsonSerializer.Deserialize<RefreshTokenJson>(payloadBytes);
        
        return newTokenJson != null && CheckUnixTimestamp(newTokenJson.Expiration);
    }
    private static bool CheckUnixTimestamp(long? expirationTimestamp)
    {
        if (expirationTimestamp == null) return false;
        return DateTimeOffset.UtcNow.ToUnixTimeSeconds() < (expirationTimestamp - 604800);
    }
    
}
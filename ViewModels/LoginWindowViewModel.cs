using System;
using System.Text.Json;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SteamAccountUtility.Messages;
using SteamAccountUtility.Models;
using SteamAccountUtility.Services;

namespace SteamAccountUtility.ViewModels;

public partial class LoginWindowViewModel(string serverAddress): ViewModelBase
{

    [ObservableProperty] private string? _username = "";
    [ObservableProperty] private string? _password = "";
    [ObservableProperty] private string? _errorMessage;
    
    [ObservableProperty] private bool _isLoginButtonEnabled = true;

    private readonly string _guardData = "";
    private readonly string _accessToken = "";
    private readonly SteamLogin _steamLogin = new (serverAddress);
    
    
    [RelayCommand]
    private async Task LoginToSteamAsync()
    {
        IsLoginButtonEnabled = false;
        ErrorMessage = "Logging in...";

        _steamLogin.GetCredentials(Username, Password, _guardData, _accessToken);
        var steamData = await _steamLogin.LoginToSteam();
        
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
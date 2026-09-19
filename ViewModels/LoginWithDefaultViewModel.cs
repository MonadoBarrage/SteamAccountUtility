using System;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SteamAccountUtility.Messages;
using SteamAccountUtility.Models;
using SteamAccountUtility.Services;

namespace SteamAccountUtility.ViewModels;

public partial class LoginWithDefaultViewModel : ViewModelBase
{
    [ObservableProperty]
    private string? _username = "generalwardragon";

    [ObservableProperty]
    private string? _password = "=g_vP>q-myx2";

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private bool _isLoginButtonEnabled = true;

    [ObservableProperty]
    private Bitmap? _qrCodeImage;

    private AllSteamData? _allSteamData;

    private readonly SteamLogin _steamLogin;

    public LoginWithDefaultViewModel(string serverAddress)
    {
        _steamLogin = new SteamLogin(serverAddress);

        WeakReferenceMessenger.Default.Register<LoginWithDefaultViewModel, UpdateQrCode>(
            this,
            (mainWindow, receivedMessage) =>
            {
                mainWindow.QrCodeImage = receivedMessage._steamQrCode;
                if (receivedMessage._steamQrCode == null)
                    Console.WriteLine("Received null QR code");
            }
        );

        _ = LoginToSteamAsync();
    }

    [RelayCommand]
    private async Task LoginToSteamAsync()
    {
        IsLoginButtonEnabled = false;
        ErrorMessage = "Logging in...";

        _steamLogin.GetCredentials(Username, Password);
        _ = _steamLogin.LoginToSteam(true);
    }

    [RelayCommand]
    private void DisconnectFromSteam()
    {
        IsLoginButtonEnabled = true;
        _steamLogin.DisconnectClient();
        ErrorMessage = "Disconnected";
    }

    private static bool ParseRefreshToken(string token)
    {
        var tokenComponents = token.Split('.');

        var base64 = tokenComponents[1].Replace('-', '+').Replace('_', '/');

        if (base64.Length % 4 != 0)
            base64 += new string('=', 4 - base64.Length % 4);

        var payloadBytes = Convert.FromBase64String(base64);

        var newTokenJson = JsonSerializer.Deserialize<RefreshTokenJson>(payloadBytes);

        return newTokenJson != null && CheckUnixTimestamp(newTokenJson.Expiration);
    }

    private static bool CheckUnixTimestamp(long? expirationTimestamp)
    {
        if (expirationTimestamp == null)
            return false;
        return DateTimeOffset.UtcNow.ToUnixTimeSeconds() < (expirationTimestamp - 604800);
    }
}

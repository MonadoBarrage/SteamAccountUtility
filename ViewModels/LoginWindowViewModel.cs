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

public partial class LoginWindowViewModel : ViewModelBase
{
    [ObservableProperty]
    private string? _username;

    [ObservableProperty]
    private string? _password;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private bool _isLoginButtonEnabled = true;

    [ObservableProperty]
    private Bitmap? _qrCodeImage;

    [ObservableProperty]
    private bool _isQrCodeButtonEnabled;

    private AllSteamData? _allSteamData;

    private readonly SteamLogin _steamLoginDefault;
    private readonly SteamLogin _steamLoginQrCode;

    public LoginWindowViewModel(string serverAddress, string? error = null)
    {
        _steamLoginDefault = new SteamLogin(serverAddress);
        _steamLoginQrCode = new SteamLogin(serverAddress);
        _errorMessage = error;
        WeakReferenceMessenger.Default.Register<LoginWindowViewModel, UpdateQrCode>(
            this,
            (mainWindow, receivedMessage) =>
            {
                mainWindow.QrCodeImage = receivedMessage._steamQrCode;
                if (receivedMessage._steamQrCode == null)
                    Console.WriteLine("Received null QR code");
            }
        );

        WeakReferenceMessenger.Default.Register<LoginWindowViewModel, RefreshQrCodeLogin>(
            this,
            (mainWindow, _) =>
            {
                _qrCodeImage = null;
                mainWindow.IsQrCodeButtonEnabled = true;
            }
        );
        _ = LoginToSteamUsingQrCodeAsync();
    }

    [RelayCommand]
    private async Task LoginToSteamUsingDefaultAsync()
    {
        IsLoginButtonEnabled = false;
        _steamLoginQrCode.TerminateClient();
        _steamLoginDefault.GetCredentials(Username, Password);
        await _steamLoginDefault.LoginToSteam(SteamLoginType.Default);
    }

    [RelayCommand]
    private async Task LoginToSteamUsingQrCodeAsync()
    {
        IsQrCodeButtonEnabled = false;
        _ = _steamLoginQrCode.LoginToSteam(SteamLoginType.QrCode);
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

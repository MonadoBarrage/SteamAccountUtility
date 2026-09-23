using System;
using Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SteamAccountUtility.Messages;

namespace SteamAccountUtility.ViewModels;

public partial class LoginSubmitCodeViewModel : ViewModelBase
{
    [ObservableProperty] private string? _steamCode;
    
    [ObservableProperty] private bool _isButtonEnabled = true;


    [RelayCommand]
    private void EnterSteamCode()
    {
        if (string.IsNullOrEmpty(SteamCode))
            return;
        IsButtonEnabled = false;
        
        WeakReferenceMessenger.Default.Send(new SendLoginCodeMessage(SteamCode));
        WeakReferenceMessenger.Default.Send(new CurrentlyLoggingInMessage("Logging in with Guard code. . . "));
    }

    
}

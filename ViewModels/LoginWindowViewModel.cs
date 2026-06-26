using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SteamAccountUtility.Messages;
using SteamAccountUtility.Models;
using SteamKit2;


namespace SteamAccountUtility.ViewModels;

public partial class LoginWindowViewModel : ViewModelBase
{

    [ObservableProperty] private string username;
    [ObservableProperty] private string password;
    [ObservableProperty] private string steamKey;
    [ObservableProperty] private string loadingMessage;
    
    
    private SteamLogin steamLogin;

    
    public LoginWindowViewModel()
    {
        steamLogin = new SteamLogin();
        WeakReferenceMessenger.Default.Register<LoginWindowViewModel, UpdateLoginMessage>
            (this, static (win, mang) =>
            {
                win.LoadingMessage = mang.newMessage;
            });
    }

    [RelayCommand]
    public async Task LoginToSteamAsync()
    {
        
        if (string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(Password))
        {
            LoadingMessage = "Please enter a username and password";
            return;
        }
        LoadingMessage = "Logging in...";
        steamLogin.GetCredentials(Username, Password, SteamKey);
        
        await steamLogin.InitializeClient();
        LoadingMessage = "Signing in";
        
        
    }
    
}
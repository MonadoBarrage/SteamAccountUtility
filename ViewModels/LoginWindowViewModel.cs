using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SteamAccountUtility.Messages;
using SteamAccountUtility.Models;

namespace SteamAccountUtility.ViewModels;

public partial class LoginWindowViewModel() : ViewModelBase
{

    [ObservableProperty] private string username;
    [ObservableProperty] private string password;
    [ObservableProperty] private string loadingMessage;
    
    private SteamLogin steamLogin = new SteamLogin();
    
    

    [RelayCommand]
    public async Task LoginToSteamAsync()
    {
        if (string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(Password))
        {
            LoadingMessage = "Please enter a username and password";
            return;
        }
        LoadingMessage = "Logging in...";
        steamLogin.GetCredentials(Username, Password);
        await steamLogin.InitializeClient();
        
        var t = WeakReferenceMessenger.Default.Send(new GoToHomePage());
        // var t = WeakReferenceMessenger.Default.Send(new CloseLoginDialogMessaage());
    }
    
    

    public void fefef(Func<string> f)
    {
        f();
    }
}
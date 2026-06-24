using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SteamAccountUtility.Messages;

namespace SteamAccountUtility.ViewModels;

public partial class HomeWindowViewModel(string t = "this is before") : ViewModelBase
{

    private SteamLogin _steamLogin;
    
    [ObservableProperty] private string _accountName = t;
    
    [RelayCommand]
    private async Task LoginToSteamAsync()
    {
        var album = await WeakReferenceMessenger.Default.Send(new LoginMessage());
    }
}
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
namespace SteamAccountUtility.ViewModels;

public partial class AuxiliaryFriendViewModel(FriendData fd): ViewModelBase
{
    
    
    
    [ObservableProperty]
    private FriendData _friend = fd;
    
    [RelayCommand]
    public void Yay()
    {
        Console.WriteLine(Friend.ProfileName);
    }
}
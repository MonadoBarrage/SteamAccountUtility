using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SteamAccountUtility.Messages;
using SteamKit2;

namespace SteamAccountUtility.ViewModels;

public partial class HomeWindowViewModel(UserData ud, List<Game> gl, Dictionary<SteamID,FriendData> fl) : ViewModelBase
{

    private SteamLogin _steamLogin;
    
    [ObservableProperty] private string profileName = ud.ProfileName;
    [ObservableProperty] private List<Game> gameList = gl;
    [ObservableProperty] private ObservableCollection<FriendData> friendList = ConvertFriendList(fl);

    
    
    public static ObservableCollection<FriendData> ConvertFriendList(Dictionary<SteamID, FriendData> fl)
    {
        ObservableCollection<FriendData> newFriendList = new ObservableCollection<FriendData>();
        foreach (var val in fl)
        {
            newFriendList.Add(val.Value);
        }
        return newFriendList;
    }
}
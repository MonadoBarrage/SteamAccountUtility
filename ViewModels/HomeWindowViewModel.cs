using System.Collections.Generic;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SteamAccountUtility.Messages;

namespace SteamAccountUtility.ViewModels;

public partial class HomeWindowViewModel(string pn, List<Game> gl, Dictionary<string,string> fl) : ViewModelBase
{

    private SteamLogin _steamLogin;
    
    [ObservableProperty] private string profileName = pn;
    [ObservableProperty] private List<Game> gameList = gl;
    [ObservableProperty] private Dictionary<string,string> friendList = fl;
    
    
}
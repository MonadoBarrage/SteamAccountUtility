using System.Collections.Generic;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using SteamAccountUtility.Models;

namespace SteamAccountUtility.ViewModels;

public partial class HomeWindowViewModel(AllSteamData allSteamData) : ViewModelBase
{
    [ObservableProperty]
    private UserData _currentUser = allSteamData.CurrentUser;

    [ObservableProperty]
    private BadgesAndLevelsResponse _badgeData =
        allSteamData.BadgesAndLevels ?? new BadgesAndLevelsResponse();

    [ObservableProperty]
    private Dictionary<int, RenderedSteamGame> _gameData =
        allSteamData.Games ?? new Dictionary<int, RenderedSteamGame>();

    [ObservableProperty]
    private ObservableCollection<FriendData> _friendsData =
        allSteamData.Friends ?? new ObservableCollection<FriendData>();

    [ObservableProperty]
    private ObservableCollection<RenderedSteamGame> _recentlyPlayedGames =
        allSteamData.RecentGames ?? new ObservableCollection<RenderedSteamGame>();
}

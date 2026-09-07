using System.Collections.Generic;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace SteamAccountUtility.ViewModels;

public partial class HomeWindowViewModel(AllSteamData allSteamData): ViewModelBase
{
    
    [ObservableProperty] private UserData _currentUser = allSteamData.CurrentUser;
    [ObservableProperty] private BadgesAndLevelsData _badgeData = 
        allSteamData?.FetchedBadgesResponse?.Response ?? new BadgesAndLevelsData();

    [ObservableProperty] private Dictionary<int, AppRenderedSteamGame> _gameData = 
        allSteamData?.Games ?? new Dictionary<int, AppRenderedSteamGame>();

    [ObservableProperty] private ObservableCollection<FriendData> _friendsData =
        allSteamData?.Friends ?? new ObservableCollection<FriendData>();

    [ObservableProperty] private ObservableCollection<AppRenderedSteamGame> _recentlyPlayedGames =
        allSteamData?.RecentGames ?? new ObservableCollection<AppRenderedSteamGame>();
}
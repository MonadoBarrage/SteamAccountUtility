using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace SteamAccountUtility.ViewModels;

public partial class HomeWindowViewModel(AllSteamData allSteamData): ViewModelBase
{
    
    [ObservableProperty] private UserData _currentUser = allSteamData.CurrentUser;
    [ObservableProperty] private BadgesAndLevelsData _badgeData = 
        allSteamData?.FetchedBadgesResponse?.Response ?? new BadgesAndLevelsData();

    [ObservableProperty] private ObservableCollection<GameData> _gameData = 
        allSteamData?.Games ?? new ObservableCollection<GameData>();

    [ObservableProperty] private ObservableCollection<FriendData> _friendsData =
        allSteamData?.Friends ?? new ObservableCollection<FriendData>();
    
    [ObservableProperty] private RecentlyPlayedGamesData _recentlyPlayedGames =
        allSteamData?.FetchedRecentlyPlayedGamesResponse?.Response ?? new RecentlyPlayedGamesData();
}
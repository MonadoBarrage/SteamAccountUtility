using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using SteamAccountUtility.Messages;
using SteamAccountUtility.Messages.Navigation;

namespace SteamAccountUtility.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{

    [ObservableProperty] private ViewModelBase _currentPage;
    
    [ObservableProperty] private AllSteamData? _allSteamData;
    
    
    
    
    public MainWindowViewModel()
    {
        CurrentPage = new LoginWindowViewModel();
         WeakReferenceMessenger.Default.Register<MainWindowViewModel, GoToHomePage>
         (this, (s, p) =>
         {
             var friendViewModels = new ObservableCollection<AuxiliaryFriendViewModel>();
             var gameViewModels = new ObservableCollection<AuxiliaryGameViewModel>();
             foreach (var f in p.UserFriendList.Values)
             {
                 friendViewModels.Add(new AuxiliaryFriendViewModel(f));
             }

             foreach (var g in p.UserGameList)
             {
                 gameViewModels.Add(new AuxiliaryGameViewModel(g));
             }
             
             _allSteamData = new AllSteamData()
             {
                 CurrentUser = p.User,
                 Friends = new ObservableCollection<FriendData>(p.UserFriendList.Values),
                 Games = new ObservableCollection<GameData>(p.UserGameList),
                 FriendVM = friendViewModels,
                 GameVM =  gameViewModels
                 
             };
             s.CurrentPage = new HomeWindowViewModel(_allSteamData);    
         });
         
         WeakReferenceMessenger.Default.Register<MainWindowViewModel, SendToFriendPage>
         (this, (s, _) =>
         {
             if (_allSteamData != null) 
                s.CurrentPage = new FriendListViewModel(_allSteamData.FriendVM);    
         });
         
         WeakReferenceMessenger.Default.Register<MainWindowViewModel, SendToGamePage>
         (this, (s, _) =>
         {
             if (_allSteamData != null) 
                s.CurrentPage = new GameLibraryViewModel(_allSteamData.GameVM);    
         });
         
         
         
    }


    
    
    
}

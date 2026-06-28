using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SteamAccountUtility.Messages;
using SteamAccountUtility.Messages.Navigation;
using SteamAccountUtility.Views;

namespace SteamAccountUtility.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{

    [ObservableProperty] private ViewModelBase _currentPage;
    
    [ObservableProperty] private AllSteamData _allSteamData;
    
    
    
    
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
                 currentUser = p.User,
                 friends = new ObservableCollection<FriendData>(p.UserFriendList.Values),
                 games = new ObservableCollection<GameData>(p.UserGameList),
                 friendVM = friendViewModels,
                 gameVM =  gameViewModels
                 
             };
             s.CurrentPage = new HomeWindowViewModel(_allSteamData);    
         });
         
         WeakReferenceMessenger.Default.Register<MainWindowViewModel, SendToFriendPage>
         (this, (s, p) =>
         {
             s.CurrentPage = new FriendListViewModel(_allSteamData.friendVM);    
         });
         
         WeakReferenceMessenger.Default.Register<MainWindowViewModel, SendToGamePage>
         (this, (s, p) =>
         {
             s.CurrentPage = new GameLibraryViewModel(_allSteamData.gameVM);    
         });
         
         
         
    }


    
    
    
}

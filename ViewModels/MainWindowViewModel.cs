using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SteamAccountUtility.Messages;
using SteamAccountUtility.Views;

namespace SteamAccountUtility.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{

    [ObservableProperty] private ViewModelBase _currentPage;
    
    public MainWindowViewModel()
    {
        CurrentPage = new LoginWindowViewModel();
         
    }
    
    
}

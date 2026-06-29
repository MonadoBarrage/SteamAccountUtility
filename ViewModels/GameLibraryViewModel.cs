using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace SteamAccountUtility.ViewModels;

public partial class GameLibraryViewModel(ObservableCollection<AuxiliaryGameViewModel> gl) : ViewModelBase
{
    [ObservableProperty] private ObservableCollection<AuxiliaryGameViewModel> _gameList = gl;
    
}
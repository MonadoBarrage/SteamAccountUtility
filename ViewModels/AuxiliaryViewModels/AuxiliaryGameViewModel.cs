using CommunityToolkit.Mvvm.ComponentModel;
using SteamAccountUtility.Models;

namespace SteamAccountUtility.ViewModels.AuxiliaryViewModels;

public partial class AuxiliaryGameViewModel(RenderedSteamGame gameData): ViewModelBase
{
    [ObservableProperty]
    private RenderedSteamGame _game = gameData;
    
}
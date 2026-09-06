using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace SteamAccountUtility.ViewModels;

public partial class AuxiliaryGameViewModel(AppRenderedSteamGame gameData): ViewModelBase
{
    [ObservableProperty]
    private AppRenderedSteamGame _game = gameData;
    
}
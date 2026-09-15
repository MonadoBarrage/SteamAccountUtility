using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace SteamAccountUtility.ViewModels;

public partial class AuxiliaryGameViewModel(RenderedSteamGame gameData): ViewModelBase
{
    [ObservableProperty]
    private RenderedSteamGame _game = gameData;
    
}
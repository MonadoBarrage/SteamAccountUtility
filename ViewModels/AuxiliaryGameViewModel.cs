using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace SteamAccountUtility.ViewModels;

public partial class AuxiliaryGameViewModel(GameData gameData): ViewModelBase
{
    [ObservableProperty]
    private GameData _game = gameData;
    
    
    [RelayCommand]
    private void Yay()
    {
        Game.PrintAll();
    }
}
using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SteamAccountUtility.ViewModels;

namespace SteamAccountUtility.ViewModels;

public partial class GameLibraryViewModel(ObservableCollection<AuxiliaryGameViewModel> gl)
    : ViewModelBase
{
    [ObservableProperty]
    private ObservableCollection<AuxiliaryGameViewModel> _gameList = gl;

    [RelayCommand]
    public void PrintOutput()
    {
        foreach (var game in _gameList)
        {
            Console.WriteLine(game.Game.LibraryImage);
        }
    }
}

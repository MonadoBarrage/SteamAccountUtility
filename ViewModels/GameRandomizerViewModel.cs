using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SteamAccountUtility.ViewModels.AuxiliaryViewModels;

namespace SteamAccountUtility.ViewModels;

public partial class GameRandomizerViewModel: ViewModelBase
{
    
    [ObservableProperty] private AuxiliaryGameViewModel? _selectedGame;
    [ObservableProperty] private string? _buttonText = "Click to select game";
    [ObservableProperty] private bool _isRandomizing = true;
    
    private readonly ObservableCollection<AuxiliaryGameViewModel> _gamesList;
    private readonly Random _randomGenerator;
    private int _delayCounter;
    
    public GameRandomizerViewModel(ObservableCollection<AuxiliaryGameViewModel> ag)
    {
        _delayCounter = 40;
        _randomGenerator = new Random();
        _gamesList = ag;
        
        _ = LoadGames();
    }

    private async Task LoadGames()
    {
        while (IsRandomizing)
        {
            // Username += "f";
            SelectedGame = _gamesList[_randomGenerator.Next(_gamesList.Count)];
            await Task.Delay(_delayCounter);
        }
    }


    [RelayCommand]
    private void StopAndSelectGame()
    {
        IsRandomizing = !IsRandomizing;
        if (IsRandomizing)
        {
            ButtonText = "Click to select game";
            _ = LoadGames();
        }
        else ButtonText = "Randomize again";

    }
    
    
}
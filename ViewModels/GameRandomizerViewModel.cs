using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace SteamAccountUtility.ViewModels;

public partial class GameRandomizerViewModel: ViewModelBase
{
    
    [ObservableProperty] private AuxiliaryGameViewModel? _selectedGame;
    [ObservableProperty] private bool _enableButton;
    
    private readonly ObservableCollection<AuxiliaryGameViewModel> _gamesList;
    private bool _continueToWork = true;
    private readonly Random _randomGenerator;
    private int _delayCounter;
    
    public GameRandomizerViewModel(ObservableCollection<AuxiliaryGameViewModel> ag)
    {
        _enableButton = true;
        _delayCounter = 40;
        _randomGenerator = new Random();
        _gamesList = ag;
        
        _ = LoadGames();
    }

    private async Task LoadGames()
    {
        while (_continueToWork)
        {
            // Username += "f";
            SelectedGame = _gamesList[_randomGenerator.Next(_gamesList.Count)];
            await Task.Delay(_delayCounter);
        }
    }


    [RelayCommand]
    private void StopAndSelectGame()
    {
        EnableButton = false;
        _continueToWork = false;

    }
    
    
}
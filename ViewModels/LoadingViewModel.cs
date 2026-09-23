using CommunityToolkit.Mvvm.ComponentModel;

namespace SteamAccountUtility.ViewModels;

public partial class LoadingViewModel(string? msg = null) : ViewModelBase
{
    [ObservableProperty]
    private string? _message = msg;
}

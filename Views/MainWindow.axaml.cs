using Avalonia.Controls;
using CommunityToolkit.Mvvm.Messaging;
using SteamAccountUtility.Messages;
using SteamAccountUtility.ViewModels;

namespace SteamAccountUtility.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        
        if (Design.IsDesignMode) return;
        // WeakReferenceMessenger.Default.Register<MainWindow, LoginMessage>
        //     (this, static (win, mang) =>
        //     {
        //         var dialog = new LoginWindowView
        //         {
        //             DataContext = new LoginWindowViewModel("champ")
        //         };
        //         
        //         mang.Reply(dialog.ShowDialog<LoginWindowViewModel?>(win));
        //     });
        
        
        // WeakReferenceMessenger.Default.Register<MainWindow, GoToHomePage>
        // (this, static (win, mang) =>
        // {
        //     if(mang.IsValid)
        //         if (win.DataContext is MainWindowViewModel vm)
        //         {
        //             vm.CurrentPage = new HomeWindowViewModel("This is after");
        //         }
        // });
        
    }
}
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using CommunityToolkit.Mvvm.Messaging;
using SteamAccountUtility.Messages;
using SteamAccountUtility.ViewModels;
using SteamAccountUtility.Views;

namespace SteamAccountUtility;

public class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        
    }

    public override void OnFrameworkInitializationCompleted()
    {
        //  WeakReferenceMessenger.Default.Register<App, GoToHomePage>
        // (this, (appHandler, homePageHandler) =>
        // {
        //     appHandler.Dispatcher.Invoke(() =>
        //     {
        //         if (appHandler.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime oldDesktop)
        //         {
        //             var oldWindow = oldDesktop.MainWindow;
        //             oldDesktop.MainWindow = new MainWindow()
        //             {
        //                 DataContext = new MainWindowViewModel(homePageHandler.UserSteamData)
        //             };
        //             oldDesktop.MainWindow.Show();
        //             oldWindow?.Close();
        //         }
        //     });
        // });
         
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            
            desktop.MainWindow = new MainWindow()
            {
                DataContext = new MainWindowViewModel()
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}


using System;
using Avalonia;
using Avalonia.Media.Fonts;

namespace SteamAccountUtility;

public sealed class AppFontCollection : EmbeddedFontCollection
{
    public AppFontCollection()
        : base(
            new Uri("fonts:AppFonts", UriKind.Absolute),
            new Uri("avares://SteamAccountUtility/Assets/Fonts", UriKind.Absolute)
        ) { }
}

sealed class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args) =>
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder
            .Configure<App>()
            .UsePlatformDetect()
#if DEBUG
            .WithDeveloperTools()
#endif
            .ConfigureFonts(fontManager =>
            {
                fontManager.AddFontCollection(new AppFontCollection());
            })
            .WithDataAnnotationsValidation()
            .WithInterFont()
            .LogToTrace();
}

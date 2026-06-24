using Avalonia;
using System;
using Avalonia.Media.Fonts;


namespace SteamAccountUtility;

public sealed class MyFontCollection : EmbeddedFontCollection
{
    public MyFontCollection() : base(
        new Uri("fonts:MyFonts", UriKind.Absolute),
        new Uri("avares://SteamAccountUtil/Assets/Fonts", UriKind.Absolute))
    {
    }
}

sealed class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args) => BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
#if DEBUG
            .WithDeveloperTools()
#endif
            .ConfigureFonts(fontManager => { fontManager.AddFontCollection(new MyFontCollection()); })
            .WithDataAnnotationsValidation()
            .WithInterFont()
            .LogToTrace();
}

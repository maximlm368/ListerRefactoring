using System;
using System.Data;
using System.Globalization;
using Avalonia;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.ReactiveUI;


namespace Lister.Desktop;

class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main ( string [] args )
    {
        try
        {
            BuildAvaloniaApp ()
            .StartWithClassicDesktopLifetime (args);
        }
        catch ( StackOverflowException ex )
        {
        }
        catch ( System.Threading.Tasks.TaskCanceledException ex ) 
        {
        }
    }


    //Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp ()
        => AppBuilder.Configure<App> ()
            .UsePlatformDetect ()
            .WithInterFont ()
            .LogToTrace ()
            .UseReactiveUI ();
}

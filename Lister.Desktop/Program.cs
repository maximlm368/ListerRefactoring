using System;
using System.Data;
using Avalonia;
using Avalonia.ReactiveUI;
using ContentAssembler;

namespace Lister.Desktop;

class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main ( string [] args )
    {
        bool isWithoutCollector = GC.TryStartNoGCRegion (500000000);

        try 
        {
            BuildAvaloniaApp ()
            .StartWithClassicDesktopLifetime (args);
        }
        catch( StackOverflowException ex ) 
        {
        }
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace()
            .UseReactiveUI();
}

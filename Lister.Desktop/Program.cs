using Avalonia;
using Avalonia.ReactiveUI;
using Lister.Desktop.App;


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
        => AppBuilder.Configure<ListerApp> ()
            .UsePlatformDetect ()
            .WithInterFont ()
            .LogToTrace ()
            .UseReactiveUI ();
}

using System;
using System.Data;
using System.Globalization;
using Avalonia;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.ReactiveUI;
using ContentAssembler;
using SkiaSharp;
using Avalonia.Skia;
using Avalonia.Media.Fonts;
using System.Reactive.Concurrency;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using DataGateway;
using Lister.ViewModels;
using Lister.Views;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;
using DocumentFormat.OpenXml.Packaging;
using System.Text.Json;
//using Json.Schema;
//using System.Text.Json.Nodes;
using Newtonsoft.Json.Linq;
using NJsonSchema;
using NJsonSchema.Validation;
using NJsonSchema.Generation;


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

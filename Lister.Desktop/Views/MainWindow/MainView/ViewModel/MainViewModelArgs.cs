using Lister.Desktop.Views.DialogMessageWindows.PrintDialog.ViewModel;
using Lister.Desktop.Views.MainWindow.MainView.Parts.BuildButton.ViewModel;
using Lister.Desktop.Views.MainWindow.MainView.Parts.NavigationZoom.ViewModel;
using Lister.Desktop.Views.MainWindow.MainView.Parts.PersonChoosing.ViewModel;
using Lister.Desktop.Views.MainWindow.MainView.Parts.PersonSource.ViewModel;
using Lister.Desktop.Views.MainWindow.MainView.Parts.Scene;
using Lister.Desktop.Views.MainWindow.MainView.Parts.Scene.ViewModel;
using Lister.Desktop.Views.MainWindow.WaitingView.ViewModel;

namespace Lister.Desktop.Views.MainWindow.MainView.ViewModel;

internal class MainViewModelArgs
{
    internal string OsName { get; set; } = string.Empty;
    internal string SuggestedFileNames { get; set; } = string.Empty;
    internal string SaveTitle { get; set; } = string.Empty;
    internal string IncorrectXSLX { get; set; } = string.Empty;
    internal string BuildingLimitExhaustedMessage { get; set; } = string.Empty;
    internal string FileIsOpenMessage { get; set; } = string.Empty;

    internal Printer Printer { get; set; }
    internal PrintDialogViewModel PrintDialog { get; set; }
    internal PersonChoosingViewModel PersonChoosing { get; set; }
    internal PersonSourceViewModel PersonSource { get; set; }
    internal NavigationZoomViewModel NavigationZoom { get; set; }
    internal SceneViewModel Scene { get; set; }
    internal WaitingViewModel Waiting { get; set; }

    internal MainViewModelArgs ( Printer printer, PrintDialogViewModel printDialog, PersonChoosingViewModel personChoosing, 
        PersonSourceViewModel personSource, NavigationZoomViewModel navigationZoom,
        SceneViewModel scene, WaitingViewModel waiting ) 
    {
        Printer = printer;
        PrintDialog = printDialog;
        PersonChoosing = personChoosing;
        PersonSource = personSource;
        NavigationZoom = navigationZoom;
        Scene = scene;
        Waiting = waiting;
    }
}

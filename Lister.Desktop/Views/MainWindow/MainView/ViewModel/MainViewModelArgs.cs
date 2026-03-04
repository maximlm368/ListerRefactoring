using Lister.Desktop.Views.DialogMessageWindows.PrintDialog.ViewModel;
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

    internal DocumentOutsider Printer { get; set; }
    internal PrintDialogViewModel PrintDialog { get; set; }
    internal PersonChoosingViewModel PersonChoosing { get; set; }
    internal PersonSourceViewModel PersonSource { get; set; }
    internal SceneViewModel Scene { get; set; }
    //internal NavigatorViewModel Navigator { get; set; }
    //internal ZoomerViewModel Zoomer { get; set; }
    internal WaitingViewModel Waiting { get; set; }

    internal MainViewModelArgs ( DocumentOutsider printer, PrintDialogViewModel printDialog, PersonChoosingViewModel personChoosing,
        PersonSourceViewModel personSource, SceneViewModel scene, WaitingViewModel waiting 
    ) 
    {
        Printer = printer;
        PrintDialog = printDialog;
        PersonChoosing = personChoosing;
        PersonSource = personSource;
        Scene = scene;
        Waiting = waiting;
    }
}

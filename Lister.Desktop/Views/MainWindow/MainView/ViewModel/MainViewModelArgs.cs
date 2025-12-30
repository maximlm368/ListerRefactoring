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
    internal string? OsName { get; set; }
    internal string? SuggestedFileNames { get; set; }
    internal string? SaveTitle { get; set; }
    internal string? IncorrectXSLX { get; set; }
    internal string? BuildingLimitExhaustedMessage { get; set; }
    internal string? FileIsOpenMessage { get; set; }
    internal string? FileIsTooBigMessage { get; set; }

    internal Printer? Printer { get; set; }
    internal PrintDialogViewModel? PrintDialogViewModel { get; set; }
    internal PersonChoosingViewModel? PersonChoosingViewModel { get; set; }
    internal PersonSourceViewModel? PersonSourceViewModel { get; set; }
    internal BadgesBuildingViewModel? BadgesBuildingViewModel { get; set; }
    internal NavigationZoomViewModel? NavigationZoomViewModel { get; set; }
    internal SceneViewModel? SceneViewModel { get; set; }
    internal WaitingViewModel? WaitingViewModel { get; set; }
}

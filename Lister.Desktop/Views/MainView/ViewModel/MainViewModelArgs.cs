using Lister.Desktop.Infrastructure;
using Lister.Desktop.Views.MainView.Parts.PersonSource.ViewModel;
using Lister.Desktop.Views.MainView.Parts.PersonChoosing.ViewModel;
using Lister.Desktop.Views.MainView.Parts.Scene.ViewModel;
using Lister.Desktop.Views.WaitingView.ViewModel;

namespace Lister.Desktop.Views.MainView.ViewModel;

internal class MainViewModelArgs
{
    internal string OsName { get; set; } = string.Empty;
    internal string SuggestedFileNames { get; set; } = string.Empty;
    internal string SaveTitle { get; set; } = string.Empty;
    internal string IncorrectXSLX { get; set; } = string.Empty;
    internal string BuildingLimitExhaustedMessage { get; set; } = string.Empty;
    internal string FileIsOpenMessage { get; set; } = string.Empty;

    internal PrintingManager LeadingOutside { get; set; }
    internal PersonChoosingViewModel PersonChoosing { get; set; }
    internal PersonSourceViewModel PersonSource { get; set; }
    internal SceneViewModel Scene { get; set; }
    internal WaitingViewModel Waiting { get; set; }

    internal MainViewModelArgs ( PrintingManager leadingOutside, PersonChoosingViewModel personChoosing,
        PersonSourceViewModel personSource, SceneViewModel scene, WaitingViewModel waiting 
    ) 
    {
        LeadingOutside = leadingOutside;
        PersonChoosing = personChoosing;
        PersonSource = personSource;
        Scene = scene;
        Waiting = waiting;
    }
}

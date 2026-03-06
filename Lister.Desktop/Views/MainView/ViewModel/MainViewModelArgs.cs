using Lister.Core.Document;
using Lister.Desktop.Entities;
using Lister.Desktop.Views.MainView.Parts.PersonChoosing.ViewModel;
using Lister.Desktop.Views.MainView.Parts.PersonSource.ViewModel;
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

    internal DocumentProcessor DocumentProcessor { get; set; }
    internal PrintingActivator PrintingManager { get; set; }
    internal PersonChoosingViewModel PersonChoosing { get; set; }
    internal PersonSourceViewModel PersonSource { get; set; }
    internal SceneViewModel Scene { get; set; }
    internal WaitingViewModel Waiting { get; set; }

    internal MainViewModelArgs ( PrintingActivator printingManager, PersonChoosingViewModel personChoosing,
        PersonSourceViewModel personSource, SceneViewModel scene, WaitingViewModel waiting, DocumentProcessor documentProcessor
    ) 
    {
        PrintingManager = printingManager;
        PersonChoosing = personChoosing;
        PersonSource = personSource;
        Scene = scene;
        Waiting = waiting;
        DocumentProcessor = documentProcessor;
    }
}

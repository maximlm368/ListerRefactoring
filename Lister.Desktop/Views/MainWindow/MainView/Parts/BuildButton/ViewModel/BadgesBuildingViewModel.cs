using ReactiveUI;
using Lister.Desktop.Views.MainWindow.EditionView.ViewModel;
using Lister.Desktop.Views.MainWindow.WaitingView.ViewModel;

namespace Lister.Desktop.Views.MainWindow.MainView.Parts.BuildButton.ViewModel;

public class BadgesBuildingViewModel : ReactiveObject
{
    public static bool BuildingOccured { get; private set; }

    private WaitingViewModel _waitingVM;

    private bool _buildingIsPossible;
    internal bool BuildingIsPossible
    {
        set
        {
            this.RaiseAndSetIfChanged( ref _buildingIsPossible, value, nameof( BuildingIsPossible ) );
        }
        get
        {
            return _buildingIsPossible;
        }
    }

    private bool _buildButtonIsTapped;
    internal bool BuildButtonIsTapped
    {
        private set
        {
            if (value)
            {
                this.RaiseAndSetIfChanged( ref _buildButtonIsTapped, value, nameof( BuildButtonIsTapped ) );
            }
            else
            {
                _buildButtonIsTapped = false;
            }
        }
        get
        {
            return _buildButtonIsTapped;
        }
    }


    internal void TryToEnableBadgeCreation(bool shouldEnable)
    {
        BuildingIsPossible = shouldEnable;
    }


    //internal void ChangeAccordingTheme(string theme)
    //{
    //    SolidColorBrush foundColor = new SolidColorBrush ( MainWindow.black );
    //    SolidColorBrush unfoundColor = new SolidColorBrush ( new Color ( 100, 0, 0, 0 ) );

    //    if ( theme == "Dark" )
    //    {
    //        foundColor = new SolidColorBrush ( MainWindow.white );
    //        unfoundColor = new SolidColorBrush ( new Color ( 100, 255, 255, 255 ) );
    //    }

    //    ObservableCollection<TemplateViewModel> templates = new ();

    //    foreach ( TemplateName name in _templateNames )
    //    {
    //        SolidColorBrush brush;

    //        if ( name.IsFound )
    //        {
    //            brush = foundColor;
    //        }
    //        else
    //        {
    //            brush = unfoundColor;
    //        }

    //        templates.Add ( new TemplateViewModel ( name, brush ) );
    //    }

    //    Templates = templates;
    //}


    internal void BuildBadges()
    {
        BuildButtonIsTapped = true;

        BuildingOccured = true;

        BadgeEditorViewModel.BackingToMainViewEvent += () =>
        {
            BuildingOccured = false;
        };

        BuildButtonIsTapped = false;
    }
}




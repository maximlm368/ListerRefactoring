using Lister.Desktop.Views.MainWindow.WaitingView.ViewModel;
using ReactiveUI;

namespace Lister.Desktop.Views.MainWindow.MainView.Parts.BuildButton.ViewModel;

public class BadgesBuildingViewModel : ReactiveObject
{
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


    internal void BuildBadges()
    {
        BuildButtonIsTapped = true;
        BuildButtonIsTapped = false;
    }
}




using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Lister.Desktop.Views.MainWindow.MainView.Parts.BuildButton.ViewModel;

public partial class BadgesBuildingViewModel : ObservableObject
{
    [ObservableProperty]
    private bool _isEnabled;

    private bool _buildButtonIsTapped;
    internal bool BuildButtonIsTapped
    {
        get
        {
            return _buildButtonIsTapped;
        }

        private set
        {
            _buildButtonIsTapped = value;

            if ( _buildButtonIsTapped )
            {
                OnPropertyChanged ();
            }
        }
    }

    [RelayCommand]
    internal void BuildBadges ()
    {
        BuildButtonIsTapped = true;
        BuildButtonIsTapped = false;
    }
}

using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Platform.Storage;
using Lister.ViewModels;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Lister.Views
{
    public partial class ModernMainView : UserControl
    {
        private double _widthDelta;
        private double _heightDelta;
        internal List<BadgeViewModel> IncorrectBadges { get; private set; }
        

        public ModernMainView ()
        {
            InitializeComponent ();
            IncorrectBadges = new List<BadgeViewModel> ();
            
        }


        internal void ReleaseRunner () 
        {
            personChoosing.ReleasePressed ();
        }


        internal void CloseCustomCombobox ()
        {
            personChoosing.CloseCustomCombobox ();
        }


        internal void ChangeSize ( double widthDifference, double heightDifference )
        {
            scene. workArea.Width -= widthDifference;
            _widthDelta -= widthDifference;
            scene.workArea.Height -= heightDifference;
            _heightDelta -= heightDifference;
            personChoosing.AdjustComboboxWidth (widthDifference);
        }


        internal void SetIncorrectBadges ( ObservableCollection <BadgeViewModel> incorrects )
        {
            ModernMainViewModel vm = DataContext as ModernMainViewModel;
            
            if ( vm != null ) 
            {
                vm.SetIncorrectBadges ( incorrects );
            }
        }
    }
}





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
        internal List<BadgeViewModel> IncorrectBadges { get; private set; }


        public ModernMainView ()
        {
            InitializeComponent ();
            IncorrectBadges = new List<BadgeViewModel> ();
            
        }


        internal void ReleaseRunner () 
        {
            personChoosing.ReleaseRunner ();
        }


        internal void CloseCustomCombobox ()
        {
            personChoosing.CloseCustomCombobox ();
        }


        internal void ChangeSize ( double widthDifference, double heightDifference )
        {
            scene. workArea.Width -= widthDifference;
            scene. workArea.Height -= heightDifference;
            personChoosing.AdjustComboboxWidth (widthDifference);
        }
    }
}





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



        private void SetChildren ( )
        {
            zoomNavigation.PassNeighbours (personSource, personChoosing, templateChoosing, scene);
            scene.PassNeighbours (personSource, personChoosing, zoomNavigation, templateChoosing);
            personChoosing.PassNeighbours (personSource, scene, zoomNavigation, templateChoosing);
            personSource.PassNeighbours (scene, personChoosing, zoomNavigation, templateChoosing);
            templateChoosing.PassNeighbours (personSource, personChoosing, zoomNavigation, scene);
        }

    }
}





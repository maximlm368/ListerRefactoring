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
        public ModernMainView ()
        {
            InitializeComponent ();

            zoomNavigation.PassNeighbours (personSource, personChoosing, templateChoosing, scene);
            scene.PassNeighbours (personSource, personChoosing, zoomNavigation, templateChoosing);
            personChoosing.PassNeighbours (personSource, scene, zoomNavigation, templateChoosing);
            personSource.PassNeighbours (scene, personChoosing, zoomNavigation, templateChoosing);
            templateChoosing.PassNeighbours (personSource, personChoosing, zoomNavigation, scene);
        }


       
    }
}

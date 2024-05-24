using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lister.ViewModels
{
    internal class ModernMainViewModel : ViewModelBase
    {
        private PersonSourceViewModel _personSourceVM;
        private PersonChoosingViewModel _personChoosingVM;
        private TemplateChoosingViewModel _templateChoosingVM;
        private ZoomNavigationViewModel _zoomNavigationVM;
        private SceneViewModel _sceneVM;


        internal ModernMainViewModel() 
        {
        
        }

    }
}

using ContentAssembler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lister.ViewModels
{
    public class ModernMainViewModel : ViewModelBase
    {
        private PersonSourceViewModel _personSourceVM;
        private PersonChoosingViewModel _personChoosingVM;
        private TemplateChoosingViewModel _templateChoosingVM;
        private ZoomNavigationViewModel _zoomNavigationVM;
        private SceneViewModel _sceneVM;


        public ModernMainViewModel ( ) 
        {
            //_personChoosingVM = new PersonChoosingViewModel (singleTypeDocumentAssembler, pageSize);
            //_personSourceVM = new PersonSourceViewModel (singleTypeDocumentAssembler, pageSize, _personChoosingVM);
            //_templateChoosingVM = new TemplateChoosingViewModel ( singleTypeDocumentAssembler, pageSize );
            //_zoomNavigationVM = new ZoomNavigationViewModel (singleTypeDocumentAssembler, pageSize);
            //_sceneVM = new SceneViewModel (singleTypeDocumentAssembler, pageSize);
        }

    }
}
//  IUniformDocumentAssembler singleTypeDocumentAssembler, ContentAssembler.Size pageSize
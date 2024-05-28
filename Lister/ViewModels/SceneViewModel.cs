using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ContentAssembler;
using ReactiveUI;

namespace Lister.ViewModels
{
    public class SceneViewModel : ViewModelBase
    {
        private VMPage vPage;
        internal VMPage VisiblePage
        {
            get { return vPage; }
            set
            {
                this.RaiseAndSetIfChanged (ref vPage, value, nameof (VisiblePage));
            }
        }


        public SceneViewModel ( IUniformDocumentAssembler singleTypeDocumentAssembler, ContentAssembler.Size pageSize ) 
        {
        
        }

    }
}

using ContentAssembler;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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


        public ModernMainViewModel ( PersonSourceViewModel personSourceVM, PersonChoosingViewModel personChoosingVM,
                                     TemplateChoosingViewModel templateChoosingVM, ZoomNavigationViewModel zoomNavigationVM,
                                     SceneViewModel sceneVM )
        {
            _personChoosingVM = personChoosingVM;
            _personSourceVM = personSourceVM;
            _templateChoosingVM = templateChoosingVM;
            _zoomNavigationVM = zoomNavigationVM;
            _sceneVM = sceneVM;
        }


        internal void ResetIncorrects ( )
        {
            _sceneVM.ResetIncorrects ();
        }

    }


}
//  IUniformDocumentAssembler singleTypeDocumentAssembler, ContentAssembler.Size pageSize

//public override void Send ( string message, List<object> args, LittleViewModel littleVM )
//{
//    if ( _personChoosingVM == (littleVM as PersonChoosingViewModel) ) 
//    {

//    }
//}

//public abstract class Mediator : ViewModelBase
//{
//    public abstract void Send ( string message, List<object> ? args, LittleViewModel littleVM );
//}



//public abstract class LittleViewModel : ViewModelBase
//{
//    protected Mediator _mediator;


//    public void SetMediator ( Mediator mediator )
//    {
//        this._mediator = mediator;
//    }


//    public virtual void Send ( string message, List<object> ? args )
//    {
//        _mediator.Send (message, args, this);
//    }


//    public abstract void Notify ( string message, List<object> ? args );
//}
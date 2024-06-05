using Avalonia;
using ContentAssembler;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Avalonia.Controls;
using Avalonia.Media;
using System.Windows.Input;
using System.Text;
using System.Net.WebSockets;
using System.ComponentModel;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Lister.Views;
using Lister.Extentions;
using System.Collections.ObjectModel;
using static QuestPDF.Helpers.Colors;
using Avalonia.Controls.Shapes;
using DynamicData;
using ReactiveUI;
using Avalonia.Input;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Buffers.Binary;
using static Lister.ViewModels.MainViewModel;
using System.Reflection;
using Microsoft.Win32;
using ExtentionsAndAuxiliary;

namespace Lister.ViewModels
{
    public class ZoomNavigationViewModel : ViewModelBase
    {
        private SceneViewModel sc;
        internal SceneViewModel SceneVM
        {
            get { return sc; }
            set
            {
                this.RaiseAndSetIfChanged (ref sc, value, nameof (SceneVM));
            }
        }

        private int vpN;
        internal int VisiblePageNumber
        {
            get { return vpN; }
            set
            {
                this.RaiseAndSetIfChanged (ref vpN, value, nameof (VisiblePageNumber));
            }
        }


        public ZoomNavigationViewModel (IUniformDocumentAssembler singleTypeDocumentAssembler, SceneViewModel sceneViewModel ) 
        {
            SceneVM = sceneViewModel;
        }


        internal void VisualiseNextPage ()
        {
            VisiblePageNumber = SceneVM.VisualiseNextPage ();
        }


        internal void VisualisePreviousPage ()
        {
            VisiblePageNumber = SceneVM.VisualisePreviousPage ();
        }


        internal void VisualiseLastPage ()
        {
            VisiblePageNumber = SceneVM.VisualiseLastPage ();
        }


        internal void VisualiseFirstPage ()
        {
            VisiblePageNumber = SceneVM.VisualiseFirstPage ();
        }


        internal void VisualisePageWithNumber ( int pageNumber )
        {
            VisiblePageNumber = SceneVM.VisualisePageWithNumber (pageNumber);
        }


        internal int GetPageCount () 
        {
            return SceneVM.GetPageCount ();
        }


        internal void ZoomOn ( short step )
        {
            SceneVM.ZoomOn ( step );
        }


        internal void ZoomOut ( short step )
        {
            SceneVM.ZoomOut ( step );
        }
    }



    public class MediatorNullException : Exception { }
}


//private void CheckMediator ()
//{
//    if ( _mediator == null )
//    {
//        throw new MediatorNullException ();
//    }
//}

//private string zoomDV;
//internal string ZoomDegreeInView
//{
//    get { return zoomDV; }
//    set
//    {
//        this.RaiseAndSetIfChanged (ref zoomDV, value, nameof (ZoomDegreeInView));
//    }
//}
//private double zoomDegree;


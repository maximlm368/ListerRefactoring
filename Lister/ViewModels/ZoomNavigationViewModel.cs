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
        private SceneViewModel _sceneVM;

        private int vpN;
        internal int VisiblePageNumber
        {
            get { return vpN; }
            set
            {
                this.RaiseAndSetIfChanged (ref vpN, value, nameof (VisiblePageNumber));
            }
        }

        private string zoomDV;
        internal string ZoomDegreeInView
        {
            get { return zoomDV; }
            set
            {
                this.RaiseAndSetIfChanged (ref zoomDV, value, nameof (ZoomDegreeInView));
            }
        }
        private double zoomDegree;


        public ZoomNavigationViewModel (IUniformDocumentAssembler singleTypeDocumentAssembler, ContentAssembler.Size pageSize,
                                        SceneViewModel sceneViewModel ) 
        {
            _sceneVM = sceneViewModel;
        }


        internal void VisualiseNextPage ()
        {
            VisiblePageNumber = _sceneVM.VisualiseNextPage ();
        }


        internal void VisualisePreviousPage ()
        {
            VisiblePageNumber = _sceneVM.VisualisePreviousPage ();
        }


        internal void VisualiseLastPage ()
        {
            VisiblePageNumber = _sceneVM.VisualiseLastPage ();
        }


        internal void VisualiseFirstPage ()
        {
            VisiblePageNumber = _sceneVM.VisualiseFirstPage ();
        }


        internal void VisualisePageWithNumber ( int pageNumber )
        {
            VisiblePageNumber = _sceneVM.VisualisePageWithNumber (pageNumber);
        }


        internal int GetPageCount () 
        {
            return _sceneVM.GetPageCount ();
        }


        internal void ZoomOnDocument ( short step )
        {
            _sceneVM.ZoomOnDocument ( step );
        }


        internal void ZoomOutDocument ( short step )
        {
            _sceneVM.ZoomOutDocument ( step );
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


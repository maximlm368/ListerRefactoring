using Lister.Views;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AnimatedImage.Avalonia;
using Avalonia;

namespace Lister.ViewModels
{
    public partial class WaitingViewModel : ViewModelBase
    {
        private WaitingView ? _view;
        private double _canvasTop = 80;
        private double _canvasLeft = 265;
        private double _canvasHeight = 467;
        private double _canvasWidth = 830;
        private readonly double _canvasHiddenVerticalMargin = 4;
        private double _canvasShownVerticalMargin = -460;
        private double _imageHeight = 300;

        private AnimatedImageSource _gifSource;
        private AnimatedImageSource ? gS;
        public AnimatedImageSource ? GifSource
        {
            get { return gS; }
            private set 
            { 
                this.RaiseAndSetIfChanged (ref gS, value, nameof(GifSource)); 
            }
        }

        private double cT;
        public double CanvasTop
        {
            get { return cT; }
            private set
            {
                this.RaiseAndSetIfChanged (ref cT, value, nameof (CanvasTop));
            }
        }

        private double cL;
        public double CanvasLeft
        {
            get { return cL; }
            private set
            {
                this.RaiseAndSetIfChanged (ref cL, value, nameof (CanvasLeft));
            }
        }

        private double cH;
        public double CanvasHeight
        {
            get { return cH; }
            private set
            {
                this.RaiseAndSetIfChanged (ref cH, value, nameof (CanvasHeight));
            }
        }

        private double cW;
        public double CanvasWidth
        {
            get { return cW; }
            private set
            {
                this.RaiseAndSetIfChanged (ref cW, value, nameof (CanvasWidth));
            }
        }

        private double iH;
        public double ImageHeight
        {
            get { return iH; }
            private set
            {
                this.RaiseAndSetIfChanged (ref iH, value, nameof (ImageHeight));
            }
        }

        private bool imV;
        public bool ImageIsVisible
        {
            get { return imV; }
            private set
            {
                this.RaiseAndSetIfChanged (ref imV, value, nameof (ImageIsVisible));
            }
        }

        private double pW;
        public double ProgressWidth
        {
            get { return pW; }
            private set
            {
                this.RaiseAndSetIfChanged (ref pW, value, nameof (ProgressWidth));
            }
        }

        private Thickness mg;
        public Thickness Margin
        {
            get { return mg; }
            private set
            {
                this.RaiseAndSetIfChanged (ref mg, value, nameof (Margin));
            }
        }


        public WaitingViewModel () 
        {
            Margin = new Thickness (0, _canvasHiddenVerticalMargin);
        }


        internal void HandleDialogOpenig ()
        {
            CanvasHeight = _canvasHeight;
            CanvasWidth = _canvasWidth;
            Margin = new Thickness (0, _canvasShownVerticalMargin);
            CanvasTop = _canvasTop;
            CanvasLeft = _canvasLeft;
        }


        internal void HandleDialogClosing ()
        {
            Margin = new Thickness (0, _canvasHiddenVerticalMargin);
        }


        public void Show ( )
        {
            CanvasHeight = _canvasHeight;
            CanvasWidth = _canvasWidth;
            Margin = new Thickness (0, _canvasShownVerticalMargin);
            CanvasTop = _canvasTop;
            CanvasLeft = _canvasLeft;

            if ( _gifSource == null )
            {
                string waintingImageIriString = App.ResourceDirectoryUri + "Icons/Loading.gif";
                _gifSource = new AnimatedImageSourceUri (new Uri (waintingImageIriString));
            }

            GifSource = _gifSource;
        }


        public void Hide ()
        {
            Margin = new Thickness (0, _canvasHiddenVerticalMargin);
            GifSource = null;
        }


        public void ChangeSize ( double heightDiff, double widthDiff )
        {
            CanvasWidth -= widthDiff;
            _canvasWidth -= widthDiff;
            CanvasHeight -= heightDiff;
            _canvasHeight -= heightDiff;

            CanvasTop -= heightDiff/2;
            _canvasTop -= heightDiff/2;
            CanvasLeft -= widthDiff/2;
            _canvasLeft -= widthDiff/2;

            _canvasShownVerticalMargin += heightDiff;

            if ( ModernMainViewModel.MainViewIsWaiting ) 
            {
                Margin = new Thickness (0, _canvasShownVerticalMargin);
            }
        }
    }
}

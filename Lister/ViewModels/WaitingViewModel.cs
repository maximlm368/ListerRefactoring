using Lister.Views;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AnimatedImage.Avalonia;

namespace Lister.ViewModels
{
    public class WaitingViewModel : ViewModelBase
    {
        private WaitingView _view;
        private double _canvasTop = 80;
        private double _canvasLeft = 250;
        private double _canvasHeight = 467;
        private double _imageHeight = 300;

        private AnimatedImageSource _gifSource;
        public AnimatedImageSource GifSource
        {
            get => _gifSource;
            set => this.RaiseAndSetIfChanged (ref _gifSource, value);
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


        public WaitingViewModel () 
        {
            CanvasHeight = 467;
            ImageHeight = 0;
            ImageIsVisible = true;
            CanvasLeft = _canvasLeft;
            CanvasTop = _canvasTop;

            // GifSource = new AnimatedImageSourceUri (new Uri ("avares://Lister/Assets/Loading.gif"));

            string waintingImageIriString = App.ResourceDirectoryUri + "Loading.gif";
            GifSource = new AnimatedImageSourceUri (new Uri (waintingImageIriString));

            int dfdf = 0;
        }

        public void Show ()
        {
            //CanvasHeight = _canvasHeight;
            //ImageHeight = _imageHeight;
            //string waintingImageIriString = App.ResourceDirectoryUri + "Loading.gif";
            //GifSource = new AnimatedImageSourceUri (new Uri (waintingImageIriString));

            GifSource = new AnimatedImageSourceUri (new Uri ("avares://Lister/Assets/Loading.gif"));
        }


        public void Hide ()
        {
            CanvasHeight = 0;
            ImageHeight = 0;
        }


        //public void PassView ( WaitingView view )
        //{
        //    _view = view;
        //}
    }
}

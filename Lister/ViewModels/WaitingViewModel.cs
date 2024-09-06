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

        private string im;
        public string Image
        {
            get { return im; }
            private set
            {
                this.RaiseAndSetIfChanged (ref im, value, nameof (Image));
            }
        }


        public WaitingViewModel () 
        {
            CanvasHeight = 467;
            ImageHeight = 0;
            ImageIsVisible = false;
            CanvasLeft = _canvasLeft;
            CanvasTop = _canvasTop;
            Image = "D:\\MML\\Lister\\Lister.Desktop\\bin\\Debug\\net8.0\\win-x64\\Resources\\Loading.gif";


            //ImageBehavior.SetAnimatedSource(_view, )

        }


        public void Show ()
        {
            CanvasHeight = _canvasHeight;
            ImageHeight = _imageHeight;
        }


        public void Hide ()
        {
            CanvasHeight = 0;
            ImageHeight = 0;
        }


        public void PassView ( WaitingView view )
        {
            _view = view;

            //var fd = new AnimatedImageSource ();

            //ImageBehavior.SetAnimatedSource (view, )
        }
    }
}

using Avalonia.Media;
using Avalonia.Media.Imaging;
using ExCSS;
using Lister.Extentions;
using Lister.ViewModels;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lister.ViewModels
{
    internal class BadgeCorrectnessViewModel : ViewModelBase
    {
        private static string _correctnessIcon = "GreenCheckMarker.jpg";
        private static string _incorrectnessIcon = "RedCross.png";
        private static readonly Bitmap _correctIcon;
        private static readonly Bitmap _incorrectIcon;


        private bool cr;
        internal bool Correctness
        {
            get { return cr; }
            private set
            {
                this.RaiseAndSetIfChanged (ref cr, value, nameof (Correctness));
            }
        }

        private Bitmap crI;
        internal Bitmap CorrectnessIcon
        {
            get { return crI; }
            private set
            {
                this.RaiseAndSetIfChanged (ref crI, value, nameof (CorrectnessIcon));
            }
        }

        private IBrush clr;
        internal IBrush BorderColor
        {
            get { return clr; }
            set
            {
                this.RaiseAndSetIfChanged (ref clr, value, nameof (BorderColor));
            }
        }

        private string bPN;
        internal string BoundPersonName
        {
            get { return bPN; }
            set
            {
                this.RaiseAndSetIfChanged (ref bPN, value, nameof (BoundPersonName));
            }
        }

        private Avalonia.Media.FontWeight bFW;
        internal Avalonia.Media.FontWeight BoundFontWeight
        {
            get { return bFW; }
            set
            {
                this.RaiseAndSetIfChanged (ref bFW, value, nameof (BoundFontWeight));
            }
        }

        private double iO;
        internal double IconOpacity
        {
            get { return iO; }
            set
            {
                this.RaiseAndSetIfChanged (ref iO, value, nameof (IconOpacity));
            }
        }

        internal BadgeViewModel BoundBadge { get; private set; }


        static BadgeCorrectnessViewModel ( )
        {
            string correctUriString = App.ResourceDirectoryUri + _correctnessIcon;
            string incorrectUriString = App.ResourceDirectoryUri + _incorrectnessIcon;

            Uri correctUri = new Uri (correctUriString);
            Uri incorrectUri = new Uri (incorrectUriString);

            _correctIcon = ImageHelper.LoadFromResource (correctUri);
            _incorrectIcon = ImageHelper.LoadFromResource (incorrectUri);
        }


        internal BadgeCorrectnessViewModel ( bool isCorrect, BadgeViewModel boundBadge ) 
        {
            BoundBadge = boundBadge;
            BoundPersonName = boundBadge.BadgeModel. Person. StringPresentation;
            BoundFontWeight = Avalonia.Media.FontWeight.Normal;
            IconOpacity = 0.5;

            if ( isCorrect )
            {
                Correctness = true;
                CorrectnessIcon = _correctIcon;
            }
            else
            {
                Correctness = false;
                CorrectnessIcon = _incorrectIcon;
            }
            
            BorderColor = new SolidColorBrush (new Avalonia.Media.Color (255, 255, 255, 255));
        }


        internal void SwitchCorrectness ( )
        {
            if ( Correctness )
            {
                Correctness = false;
                CorrectnessIcon = _incorrectIcon;
            }
            else
            {
                Correctness = true;
                CorrectnessIcon = _correctIcon;
            }
        }
    }
}

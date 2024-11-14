using Avalonia.Media;
using Avalonia.Media.Imaging;
using ExCSS;
using Lister.Extentions;
using Lister.ViewModels;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Lister.ViewModels
{
    internal class BadgeCorrectnessViewModel : ViewModelBase
    {
        private static string _correctnessIcon = "Icons/GreenCheckMarker.jpg";
        private static string _incorrectnessIcon = "Icons/RedCross.png";
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

        private double pE;
        internal double PersonNameExpending
        {
            get { return pE; }
            set
            {
                this.RaiseAndSetIfChanged (ref pE, value, nameof (PersonNameExpending));
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


        internal BadgeCorrectnessViewModel ( bool isCorrect, BadgeViewModel boundBadge
                                           , double widthLimit, int [] remotableGlyphRange ) 
        {
            BoundBadge = boundBadge;
            BoundFontWeight = Avalonia.Media.FontWeight.Normal;

            CalcStringPresentation ( widthLimit, remotableGlyphRange );
            
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


        internal void CalcStringPresentation ( double widthLimit, int [] remotableGlyphRange )
        {
            string personPresentation = BoundBadge. BadgeModel. Person. StringPresentation;

            int index = Math.Min (personPresentation.Length, remotableGlyphRange [1]);

            for ( ; index > remotableGlyphRange [0]; index-- )
            {
                FormattedText formatted = new FormattedText (personPresentation
                                                           , System.Globalization.CultureInfo.CurrentCulture
                                                           , FlowDirection.LeftToRight, Typeface.Default, 16, null);

                formatted.SetFontWeight (this.BoundFontWeight);

                if ( formatted.Width <= widthLimit )
                {
                    break;
                }
                else
                {
                    personPresentation = personPresentation.Remove (index) + "...";
                }
            }

            BoundPersonName = personPresentation;
        }
    }
}

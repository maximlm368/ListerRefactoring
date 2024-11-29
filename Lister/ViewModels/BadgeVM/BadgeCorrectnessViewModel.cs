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
        private static string _correctnessIconPath = "Icons/GreenCheckMarker.jpg";
        private static string _incorrectnessIconPath = "Icons/RedCross.png";
        private static readonly Bitmap _correctIcon;
        private static readonly Bitmap _incorrectIcon;


        private bool _correctness;
        internal bool Correctness
        {
            get { return _correctness; }
            private set
            {
                this.RaiseAndSetIfChanged (ref _correctness, value, nameof (Correctness));
            }
        }

        private Bitmap _correctnessIcon;
        internal Bitmap CorrectnessIcon
        {
            get { return _correctnessIcon; }
            private set
            {
                this.RaiseAndSetIfChanged (ref _correctnessIcon, value, nameof (CorrectnessIcon));
            }
        }

        private IBrush _borderColor;
        internal IBrush BorderColor
        {
            get { return _borderColor; }
            set
            {
                this.RaiseAndSetIfChanged (ref _borderColor, value, nameof (BorderColor));
            }
        }

        private string _boundPersonName;
        internal string BoundPersonName
        {
            get { return _boundPersonName; }
            set
            {
                this.RaiseAndSetIfChanged (ref _boundPersonName, value, nameof (BoundPersonName));
            }
        }

        private double _personNameExpending;
        internal double PersonNameExpending
        {
            get { return _personNameExpending; }
            set
            {
                this.RaiseAndSetIfChanged (ref _personNameExpending, value, nameof (PersonNameExpending));
            }
        }

        private Avalonia.Media.FontWeight _boundFontWeight;
        internal Avalonia.Media.FontWeight BoundFontWeight
        {
            get { return _boundFontWeight; }
            set
            {
                this.RaiseAndSetIfChanged (ref _boundFontWeight, value, nameof (BoundFontWeight));
            }
        }

        private double _iconOpacity;
        internal double IconOpacity
        {
            get { return _iconOpacity; }
            set
            {
                this.RaiseAndSetIfChanged (ref _iconOpacity, value, nameof (IconOpacity));
            }
        }

        internal BadgeViewModel BoundBadge { get; private set; }


        static BadgeCorrectnessViewModel ( )
        {
            string correctUriString = App.ResourceDirectoryUri + _correctnessIconPath;
            string incorrectUriString = App.ResourceDirectoryUri + _incorrectnessIconPath;

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

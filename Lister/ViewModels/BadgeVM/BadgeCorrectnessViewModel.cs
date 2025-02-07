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
        private static readonly string _correctIcon;
        private static readonly string _incorrectIcon;
        private static SolidColorBrush _focusedBackground;
        private static SolidColorBrush _defaultBackground;

        private double _extendedScrollableMaxIconWidth;
        private double _mostExtendedMaxIconWidth;
        private double _shrinkedIconWidth;

        private bool _correctness;
        internal bool Correctness
        {
            get { return _correctness; }
            private set
            {
                this.RaiseAndSetIfChanged (ref _correctness, value, nameof (Correctness));
            }
        }

        private string _correctnessIcon;
        internal string CorrectnessIcon
        {
            get { return _correctnessIcon; }
            private set
            {
                if ( value == _correctIcon )
                {
                    byte red = 0x3a;
                    byte green = 0x81;
                    byte blue = 0x3A;

                    CorrectnessColor = new SolidColorBrush (new Avalonia.Media.Color (255, red, green, blue));
                }
                else 
                {
                    byte red = 0xd2;
                    byte green = 0x36;
                    byte blue = 0x50;

                    CorrectnessColor = new SolidColorBrush (new Avalonia.Media.Color (255, red, green, blue));
                }

                this.RaiseAndSetIfChanged (ref _correctnessIcon, value, nameof (CorrectnessIcon));
            }
        }

        private SolidColorBrush _background;
        internal SolidColorBrush Background
        {
            get { return _background; }
            private set
            {
                this.RaiseAndSetIfChanged (ref _background, value, nameof (Background));
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

        private double _width;
        internal double Width
        {
            get { return _width; }
            set
            {
                InsideBorderWidth = value - 2;
                this.RaiseAndSetIfChanged (ref _width, value, nameof (Width));
            }
        }

        private double _insideBorderWidth;
        internal double InsideBorderWidth
        {
            get { return _insideBorderWidth; }
            set
            {
                this.RaiseAndSetIfChanged (ref _insideBorderWidth, value, nameof (InsideBorderWidth));
            }
        }

        private Avalonia.Media.FontWeight _boundFontWeight;
        internal Avalonia.Media.FontWeight BoundFontWeight
        {
            get { return _boundFontWeight; }
            set
            {
                if ( value == Avalonia.Media.FontWeight.Bold )
                {
                    Background = _focusedBackground;
                }
                else 
                {
                    Background = _defaultBackground;
                }

                this.RaiseAndSetIfChanged (ref _boundFontWeight, value, nameof (BoundFontWeight));
            }
        }

        private SolidColorBrush _correctnessColor;
        internal SolidColorBrush CorrectnessColor
        {
            get { return _correctnessColor; }
            private set
            {
                this.RaiseAndSetIfChanged (ref _correctnessColor, value, nameof (CorrectnessColor));
            }
        }

        internal BadgeViewModel BoundBadge { get; private set; }


        static BadgeCorrectnessViewModel ()
        {
            _correctIcon = "\uf00c";
            _incorrectIcon = "\uf00d";
            _focusedBackground = new SolidColorBrush (new Avalonia.Media.Color (255, 186, 220, 248));
            _defaultBackground = new SolidColorBrush (new Avalonia.Media.Color (255, 238, 238, 238));
        }


        internal BadgeCorrectnessViewModel ( BadgeViewModel boundBadge, double extendedScrollableWidth, double shortWidth
                                            , double widthLimit, bool isExtended ) 
        {
            BoundBadge = boundBadge;
            BoundFontWeight = Avalonia.Media.FontWeight.Normal;

            CalcStringPresentation ( widthLimit );

            if ( boundBadge.IsCorrect )
            {
                Correctness = true;
                CorrectnessIcon = _correctIcon;
            }
            else
            {
                Correctness = false;
                CorrectnessIcon = _incorrectIcon;
            }

            _extendedScrollableMaxIconWidth = extendedScrollableWidth;
            _shrinkedIconWidth = shortWidth;

            if ( isExtended )
            {
                Width = _extendedScrollableMaxIconWidth;
            }
            else 
            {
                Width = _shrinkedIconWidth;
            }
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


        internal void CalcStringPresentation ( double widthLimit )
        {
            string tail = "...";
            string personPresentation = BoundBadge.BadgeModel.Person.StringPresentation;

            FormattedText formatted = new FormattedText (personPresentation, System.Globalization.CultureInfo.CurrentCulture
                                           , FlowDirection.LeftToRight, Typeface.Default, 16, null);

            formatted.SetFontWeight (this.BoundFontWeight);

            if ( formatted.Width <= widthLimit )
            {
                BoundPersonName = personPresentation;
                return;
            }
            else
            {
                personPresentation = personPresentation.Substring (0, personPresentation.Length - 1) + tail;
            }

            for ( int index = personPresentation.Length - 1;   index > 0;   index-- )
            {
                string subStr = personPresentation.Substring (0, (index - 4)) + tail;

                formatted = new FormattedText (subStr, System.Globalization.CultureInfo.CurrentCulture
                                                           , FlowDirection.LeftToRight, Typeface.Default, 16, null);

                formatted.SetFontWeight (this.BoundFontWeight);

                if ( formatted.Width <= widthLimit )
                {
                    personPresentation = subStr;
                    break;
                }
            }

            BoundPersonName = personPresentation;
        }
    }
}

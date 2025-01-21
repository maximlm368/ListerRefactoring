using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Globalization;
using Avalonia.Media;
using ReactiveUI;
using ContentAssembler;
using QuestPDF.Infrastructure;
using ExtentionsAndAuxiliary;
using System.Reflection;
using Avalonia.Controls.Shapes;
using static System.Net.Mime.MediaTypeNames;
using Avalonia;
using System.Reflection.Emit;
using Avalonia.Controls;
using Avalonia.Fonts.Inter;
using Avalonia.Media.Fonts;
using Avalonia.Platform;
using SkiaSharp;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Web.Services.Description;

namespace Lister.ViewModels
{
    public class TextLineViewModel : BadgeMember
    {
        private static readonly double _maxFontSizeLimit = 30;
        private static readonly double _minFontSizeLimit = 6;
        private static readonly double _divider = 4;
        private static readonly double _parentToChildCoeff = 2.5;
        private static TextBlock _textBlock;

        public string FontFile { get; private set; }
        public byte Red { get; private set; }
        public byte Green { get; private set; }
        public byte Blue { get; private set; }

        private string _alignmentName;

        internal TextualAtom DataSource { get; private set; }

        private HorizontalAlignment _alignment;
        internal HorizontalAlignment Alignment
        {
            get { return _alignment; }
            private set
            {
                this.RaiseAndSetIfChanged (ref _alignment, value, nameof (Alignment));
            }
        }

        private Thickness _padding;
        internal Thickness Padding
        {
            get { return _padding; }
            private set
            {
                this.RaiseAndSetIfChanged (ref _padding, value, nameof (Padding));
            }
        }

        private double _fontSize;
        internal double FontSize
        {
            get { return _fontSize; }
            private set
            {
                this.RaiseAndSetIfChanged (ref _fontSize, value, nameof (FontSize));
            }
        }

        private Avalonia.Media.FontFamily _fontFamily;
        internal Avalonia.Media.FontFamily FontFamily
        {
            get { return _fontFamily; }
            private set
            {
                this.RaiseAndSetIfChanged (ref _fontFamily, value, nameof (FontFamily));
            }
        }

        private Avalonia.Media.FontWeight _fontWeight;
        internal Avalonia.Media.FontWeight FontWeight
        {
            get { return _fontWeight; }
            private set
            {
                this.RaiseAndSetIfChanged (ref _fontWeight, value, nameof (FontWeight));
            }
        }

        private string _content;
        internal string Content
        {
            get { return _content; }
            set
            {
                this.RaiseAndSetIfChanged (ref _content, value, nameof (Content));
                SetUsefullWidth ();
            }
        }

        private IBrush _foreground;
        internal IBrush Foreground
        {
            get { return _foreground; }
            private set
            {
                this.RaiseAndSetIfChanged (ref _foreground, value, nameof (Foreground));
            }
        }

        private IBrush _background;
        internal IBrush Background
        {
            get { return _background; }
            set
            {
                this.RaiseAndSetIfChanged (ref _background, value, nameof (Background));
            }
        }

        private bool _isSplitable;
        internal bool IsSplitable
        {
            get { return _isSplitable; }
            private set
            {
                this.RaiseAndSetIfChanged (ref _isSplitable, value, nameof (IsSplitable));
            }
        }

        private double _usefullWidth;
        internal double UsefullWidth
        {
            get { return _usefullWidth; }
            set
            {
                this.RaiseAndSetIfChanged (ref _usefullWidth, value, nameof (UsefullWidth));
            }
        }

        private double _usefullHeight;
        internal double UsefullHeight
        {
            get { return _usefullHeight; }
            set
            {
                this.RaiseAndSetIfChanged (ref _usefullHeight, value, nameof (UsefullHeight));
            }
        }

        internal bool isBorderViolent = false;
        internal bool isOverLayViolent = false;

        public TextLineViewModel ( TextualAtom text )
        {
            _alignmentName = text.Alignment;
            DataSource = text;

            bool fontSizeIsOutOfRange = ( text.FontSize < _minFontSizeLimit )   ||   ( text.FontSize > _maxFontSizeLimit );

            if ( fontSizeIsOutOfRange ) 
            {
            
            }

            FontSize = text.FontSize;
            FontFile = text.FontFile;

            FontWeight = GetFontWeight (text.FontWeight);
            string fontName = text.FontName;
            FontFamily = new Avalonia.Media.FontFamily (fontName);

            Content = text.Content;
            IsSplitable = text.IsSplitable;

            byte red = text.ForegroundRGB [0];
            byte green = text.ForegroundRGB [1];
            byte blue = text.ForegroundRGB [2];
            Red = red;
            Green = green;
            Blue = blue;

            SolidColorBrush foreground = new SolidColorBrush (new Avalonia.Media.Color (255, red, green, blue));
            Foreground = foreground;

            double correctHeight = FontSize * _parentToChildCoeff;

            SetUsefullWidth ();

            Avalonia.Media.Color color = 
                               new Avalonia.Media.Color (255, text.OutlineRGB [0], text.OutlineRGB [1], text.OutlineRGB [2]);
            SolidColorBrush brush = new SolidColorBrush (color);

            SetYourself (text.Width, FontSize, text.TopOffset, text.LeftOffset, brush);
            SetAlignment (text.Alignment);

            Height = text.Height;
            Padding = GetPadding ();
        }


        private TextLineViewModel ( TextLineViewModel source )
        {
            _alignmentName = source._alignmentName;
            DataSource = source.DataSource;
            FontSize = source.FontSize;
            FontFamily = source.FontFamily;
            FontFile = source.FontFile;
            FontWeight = source.FontWeight;
            Content = source.Content;
            IsSplitable = source.IsSplitable;
            Padding = new Thickness (0, -FontSize / _divider);
            UsefullWidth = source.UsefullWidth;
            UsefullHeight = source.UsefullHeight;
            Foreground = source.Foreground;
            Red = source.Red;
            Green = source.Green;
            Blue = source.Blue;
            Background = source.Background;

            SetYourself (source.UsefullWidth, source.Height, source.TopOffset
                                                                  , source.LeftOffset, source.outlineColorStorage);
            Padding = GetPadding ();
        }


        public static double CalculateWidth ( string content, TextualAtom demensions )
        {
            FormattedText formatted = new FormattedText (content
                                                       , System.Globalization.CultureInfo.CurrentCulture
                                                       , FlowDirection.LeftToRight, Typeface.Default
                                                       , demensions.FontSize, null);

            formatted.SetFontWeight (GetFontWeight (demensions.FontWeight));
            formatted.SetFontSize (demensions.FontSize);
            formatted.SetFontFamily (new Avalonia.Media.FontFamily (demensions.FontName));

            return formatted.Width;
        }


        private Thickness GetPadding ()
        {
            UsefullHeight = HeightWithBorder;
            double paddingTop = ( Height - FontSize ) / 4;
            

            //return new Thickness (0, 1);
            return new Thickness (0, -3);
        }


        private static Avalonia.Media.FontWeight GetFontWeight ( string weightName )
        {
            Avalonia.Media.FontWeight weight = Avalonia.Media.FontWeight.Normal;

            if ( weightName == "Bold" )
            {
                weight = Avalonia.Media.FontWeight.Bold;
            }
            else if ( weightName == "Thin" )
            {
                weight = Avalonia.Media.FontWeight.Thin;
            }

            return weight;
        }


        private static Avalonia.Media.FontFamily GetFontFamilyByName ( string fontName )
        {
            return new Avalonia.Media.FontFamily (fontName);
        }


        private void SetUsefullWidth ()
        {
            FormattedText formatted = new FormattedText (Content
                                                       , System.Globalization.CultureInfo.CurrentCulture
                                                       , FlowDirection.LeftToRight, Typeface.Default
                                                       , FontSize, null);

            formatted.SetFontWeight (FontWeight);
            formatted.SetFontSize (FontSize);
            formatted.SetFontFamily (FontFamily);

            UsefullWidth = formatted.Width;
            UsefullHeight = HeightWithBorder;
        }


        internal TextLineViewModel Clone ()
        {
            TextLineViewModel clone = new TextLineViewModel (this);
            return clone;
        }


        internal void ZoomOn ( double coefficient )
        {
            base.ZoomOn (coefficient);
            FontSize *= coefficient;
            UsefullWidth *= coefficient;
            UsefullHeight *= coefficient;
            Padding = new Thickness (Padding.Left * coefficient, Padding.Top * coefficient);
        }


        internal void ZoomOut ( double coefficient )
        {
            base.ZoomOut (coefficient);
            FontSize /= coefficient;
            UsefullWidth /= coefficient;
            UsefullHeight /= coefficient;
            Padding = new Thickness (Padding.Left / coefficient, Padding.Top / coefficient);
        }


        internal void Increase ( double additable )
        {
            double oldFontSize = FontSize;
            FontSize += additable;

            if ( Math.Round(FontSize / additable) > _maxFontSizeLimit )
            {
                FontSize = oldFontSize;
                return;
            }

            SetUsefullWidth ();
            double proportion = FontSize / oldFontSize;
            Height *= proportion;
            Padding = GetPadding ();
        }


        internal void Reduce ( double subtractable )
        {
            double oldFontSize = FontSize;
            FontSize -= subtractable;

            if ( Math.Round(FontSize / subtractable) < _minFontSizeLimit ) 
            {
                FontSize = oldFontSize;
                return;
            }

            SetUsefullWidth ();
            double proportion = oldFontSize / FontSize;
            Height /= proportion;
            Padding = GetPadding ();
        }


        private void SetAlignment ( string alignmentName ) 
        {
            if ( Width <= UsefullWidth )
            {
                return;
            }

            if ( alignmentName == "Right" )
            {
                LeftOffset += ( Width - Math.Ceiling(UsefullWidth) );
            }
            else if ( alignmentName == "Center" )
            {
                LeftOffset += ( Width - UsefullWidth ) / 2;
            }
        }


        internal List <TextLineViewModel> SplitYourself ( List<string> splittedContents, double scale, double layoutWidth )
        {
            List <TextLineViewModel> result = new List <TextLineViewModel>();
            double splitableLineLeftOffset = LeftOffset;
            double offsetInQueue = LeftOffset;

            foreach ( string content   in   splittedContents )
            {
                TextLineViewModel newLine = new TextLineViewModel (this.DataSource);

                if ( scale > 1 )
                {
                    newLine.ZoomOn (scale);
                }
                else if ( scale < 1 ) 
                {
                    newLine.ZoomOut (scale);
                }
                
                newLine.LeftOffset = offsetInQueue;
                newLine.ReplaceContent (content);

                if ( newLine.LeftOffset >= layoutWidth - 10 ) 
                {
                    newLine.LeftOffset = splitableLineLeftOffset;
                }
                
                newLine.TopOffset = TopOffset;
                offsetInQueue += newLine.UsefullWidth + scale;
                result.Add (newLine);
            }

            return result;
        }


        private void ReplaceContent ( string content )
        {
            if ( content == null ) 
            {
                content = string.Empty;
            }

            Content = content;
            SetUsefullWidth ();
        }
    }
}


//public static double CalculateWidth ( string content, TextualAtom demensions )
//{
//    //TextBlock tb = new TextBlock ();
//    //tb.LetterSpacing = 0;
//    //tb.Text = content;
//    //tb.FontSize = demensions.FontSize;

//    //tb.FontWeight = GetFontWeight (demensions.FontWeight);
//    //string fontName = demensions.FontName;
//    //tb.FontFamily = new FontFamily (fontName);

//    //Avalonia.Size size = new Avalonia.Size (double.PositiveInfinity, double.PositiveInfinity);
//    //tb.Measure (size);
//    //tb.Arrange (new Rect ());


//    FormattedText formatted = new FormattedText (content
//                                               , System.Globalization.CultureInfo.CurrentCulture
//                                               , FlowDirection.LeftToRight, Typeface.Default
//                                               , demensions.FontSize, null);

//    formatted.SetFontWeight (GetFontWeight (demensions.FontWeight));
//    formatted.SetFontSize (demensions.FontSize);
//    formatted.SetFontFamily ("Kramola");

//    double wd = formatted.Width;

//    return wd;

//    //return tb.DesiredSize.Width;
//}
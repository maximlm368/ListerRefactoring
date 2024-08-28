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

namespace Lister.ViewModels
{
    public class TextLineViewModel : BadgeMember
    {
        private static readonly double _maxFontSizeLimit = 30;
        private static readonly double _minFontSizeLimit = 6;
        private static readonly double _divider = 4;
        private static readonly double _parentToChildCoeff = 2.5;

        public string FontFile { get; private set; }
        public byte Red { get; private set; }
        public byte Green { get; private set; }
        public byte Blue { get; private set; }

        private string _alignmentName;

        internal TextualAtom DataSource { get; private set; }

        private HorizontalAlignment al;
        internal HorizontalAlignment Alignment
        {
            get { return al; }
            private set
            {
                this.RaiseAndSetIfChanged (ref al, value, nameof (Alignment));
            }
        }

        private Thickness pd;
        internal Thickness Padding
        {
            get { return pd; }
            private set
            {
                this.RaiseAndSetIfChanged (ref pd, value, nameof (Padding));
            }
        }

        private double fs;
        internal double FontSize
        {
            get { return fs; }
            private set
            {
                this.RaiseAndSetIfChanged (ref fs, value, nameof (FontSize));
            }
        }

        private FontFamily ff;
        internal FontFamily FontFamily
        {
            get { return ff; }
            private set
            {
                this.RaiseAndSetIfChanged (ref ff, value, nameof (FontFamily));
            }
        }

        private Avalonia.Media.FontWeight fW;
        internal Avalonia.Media.FontWeight FontWeight
        {
            get { return fW; }
            private set
            {
                this.RaiseAndSetIfChanged (ref fW, value, nameof (FontWeight));
            }
        }

        private string cn;
        internal string Content
        {
            get { return cn; }
            set
            {
                this.RaiseAndSetIfChanged (ref cn, value, nameof (Content));

                //Typeface face = new Typeface (FontFamily, FontStyle.Normal, FontWeight);
                //FormattedText formatted = new FormattedText (Content, CultureInfo.CurrentCulture
                //                                                    , FlowDirection.LeftToRight, face, FontSize, null);
                //UsefullWidth = formatted.WidthIncludingTrailingWhitespace;

                SetWidth ();
            }
        }

        private IBrush fG;
        internal IBrush Foreground
        {
            get { return fG; }
            private set
            {
                this.RaiseAndSetIfChanged (ref fG, value, nameof (Foreground));
            }
        }

        private IBrush bG;
        internal IBrush Background
        {
            get { return bG; }
            set
            {
                this.RaiseAndSetIfChanged (ref bG, value, nameof (Background));
            }
        }

        private bool ish;
        internal bool IsSplitable
        {
            get { return ish; }
            private set
            {
                this.RaiseAndSetIfChanged (ref ish, value, nameof (IsSplitable));
            }
        }

        private double uW;
        internal double UsefullWidth
        {
            get { return uW; }
            set
            {
                this.RaiseAndSetIfChanged (ref uW, value, nameof (UsefullWidth));
            }
        }

        internal bool isBorderViolent = false;
        internal bool isOverLayViolent = false;


        public TextLineViewModel ( TextualAtom text )
        {
            _alignmentName = text.Alignment;
            DataSource = text;
            FontSize = text.FontSize;
            FontFile = text.FontFile;

            // string fontUriString = App.ResourceDirectoryUri + text.FontFile + "#Kramola";
            string fontUriString = "file:///D:\\MML\\Lister\\Lister.Desktop\\bin\\Debug\\net8.0\\win-x64\\Resources\\Pushkin.ttf";
            Uri fontUri = new Uri (fontUriString);
            //string fontName = text.FontFile.Substring (0, text.FontFile.Length - 4);
            //fontName = "Pushkin";
            //var uri = "avares://Assets/Pushkin.ttf";
            //FontFamily = new FontFamily (fontName);
            //var fm = FontManager.Current;
            //FontManager.Current.AddFontCollection (new EmbeddedFontCollection (fontUri, fontUri));


            FontWeight = GetFontWeight (text.FontWeight);
            string fontName = text.FontName;
            FontFamily = new FontFamily (fontName);
            //FontFamily = new FontFamily (fontUri, fontName);

            Content = text.Content;
            IsSplitable = text.IsSplitable;

            byte red = text.Foreground [0];
            byte green = text.Foreground [1];
            byte blue = text.Foreground [2];
            Red = red;
            Green = green;
            Blue = blue;

            SolidColorBrush foreground = new SolidColorBrush (new Avalonia.Media.Color (255, red, green, blue));
            Foreground = foreground;
            double correctHeight = FontSize * _parentToChildCoeff;

            SetWidth ();
            SetYourself (text.Width, correctHeight, text.TopOffset, text.LeftOffset);
            SetAlignment (text.Alignment);

            Padding = SetPadding ();
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
            Foreground = source.Foreground;
            Red = source.Red;
            Green = source.Green;
            Blue = source.Blue;
            Background = source.Background;

            SetYourself (source.Width, source.Height, source.TopOffset, source.LeftOffset);

            Padding = SetPadding ();
        }


        public static double CalculateWidth ( string content, TextualAtom demensions )
        {
            TextBlock tb = new ();
            tb.LetterSpacing = 0;
            tb.Text = content;
            tb.FontSize = demensions.FontSize;

            //string fontUriString = App.ResourceDirectoryUri + demensions.FontFile;
            //Uri fontUri = new Uri (fontUriString);
            //string fontName = demensions.FontFile.Substring (0, demensions.FontFile.Length - 4);
            //tb.FontFamily = 
            //tb.FontWeight = Avalonia.Media.FontWeight.Bold;

            tb.FontWeight = GetFontWeight (demensions.FontWeight);
            string fontName = demensions.FontName;
            tb.FontFamily = new FontFamily (fontName);

            Avalonia.Size size = new Avalonia.Size (double.PositiveInfinity, double.PositiveInfinity);
            tb.Measure (size);
            tb.Arrange (new Rect ());

            return tb.DesiredSize.Width;
        }


        private Thickness SetPadding ()
        {
            Thickness padding;
            double verticalPadding = ( FontSize - Height ) / 2;
            
            padding = new Thickness (0, verticalPadding);

            return padding;
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


        private static Avalonia.Media.FontFamily GetFontFamily ( string fontName )
        {
            Avalonia.Media.FontFamily font = new FontFamily(fontName);

            return font;
        }


        private void SetWidth ()
        {
            TextBlock tb = new ();
            tb.LetterSpacing = 0;
            tb.Text = Content;
            tb.FontSize = FontSize;
            tb.FontFamily = FontFamily;
            tb.FontWeight = FontWeight;
            
            Avalonia.Size size = new Avalonia.Size (double.PositiveInfinity, double.PositiveInfinity);
            tb.Measure (size);
            tb.Arrange (new Rect ());
            UsefullWidth = tb.DesiredSize. Width;
        }


        internal TextLineViewModel Clone ()
        {
            TextLineViewModel clone = new TextLineViewModel (this);
            return clone;
        }


        internal void ZoomOn ( double coefficient, bool isToStandard )
        {
            base.ZoomOn (coefficient);
            FontSize *= coefficient;
            UsefullWidth *= coefficient;
        }


        internal void ZoomOut ( double coefficient, bool isToStandard )
        {
            base.ZoomOut (coefficient);
            FontSize /= coefficient;
            UsefullWidth /= coefficient;
        }


        //private void SetStandardDimensions ()
        //{
        //    FontSize = _standardFontSize;
        //    UsefullWidth = _standardUsefullWidth;
        //    TopOffset = _standardTopOffset;
        //    Padding =
        //        new Thickness (_standardPadding.Left, _standardPadding.Top, _standardPadding.Right, _standardPadding.Bottom);
        //}


        internal void Increase ( double additable )
        {
            double oldFontSize = FontSize;
            FontSize += additable;

            if ( Math.Round(FontSize / additable) > _maxFontSizeLimit )
            {
                FontSize = oldFontSize;
                return;
            }

            SetWidth ();
            double proportion = FontSize / oldFontSize;
            Height *= proportion;
            Padding = SetPadding ();
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

            SetWidth ();
            double proportion = oldFontSize / FontSize;
            Height /= proportion;
            Padding = SetPadding ();
        }


        private void SetAlignment ( string alignmentName ) 
        {
            if ( Width <= UsefullWidth )
            {
                return;
            }

            if ( alignmentName == "Right" )
            {
                LeftOffset += ( Width - UsefullWidth );
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
                    newLine.ZoomOn (scale, false);
                }
                else if ( scale < 1 ) 
                {
                    newLine.ZoomOut (scale, false);
                }
                
                newLine.LeftOffset = offsetInQueue;
                newLine.ReplaceContent (content);

                if ( newLine.LeftOffset >= layoutWidth - 10 ) 
                {
                    newLine.LeftOffset = splitableLineLeftOffset;
                }
                
                newLine.TopOffset = TopOffset;
                offsetInQueue += newLine.UsefullWidth;
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
            SetWidth ();
        }
    }
}

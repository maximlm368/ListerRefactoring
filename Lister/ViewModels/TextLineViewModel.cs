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

namespace Lister.ViewModels
{
    public class TextLineViewModel : BadgeMember
    {
        private static readonly double _maxFontSizeLimit = 30;
        private static readonly double _minFontSizeLimit = 6;
        private static readonly double _divider = 4;
        private static readonly double _parentToChildCoeff = 2.5;
        internal static double FrameSpanOnEnd { get; private set; }


        static TextLineViewModel () 
        {
            FrameSpanOnEnd = 2;
        }


        private TextualAtom _dataSource;

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

                Typeface face = new Typeface (FontFamily, FontStyle.Normal, FontWeight);
                FormattedText formatted = new FormattedText (Content, CultureInfo.CurrentCulture
                                                                    , FlowDirection.LeftToRight, face, FontSize, null);
                UsefullWidth = formatted.Width + +( FrameSpanOnEnd * _scale );
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
            private set
            {
                this.RaiseAndSetIfChanged (ref uW, value, nameof (UsefullWidth));
            }
        }

        internal bool isBorderViolent = false;
        internal bool isOverLayViolent = false;

        public TextLineViewModel ( TextualAtom text )
        {
            _dataSource = text;
            FontSize = text.FontSize;
            FontFamily = new FontFamily (text.FontFamily);
            FontWeight = Avalonia.Media.FontWeight.Bold;
            Content = text.Content;
            IsSplitable = text.IsSplitable;

            byte red = text.Foreground [0];
            byte green = text.Foreground [1];
            byte blue = text.Foreground [2];

            SolidColorBrush foreground = new SolidColorBrush (new Color (255, red, green, blue));
            Foreground = foreground;

            Typeface face = new Typeface (FontFamily, FontStyle.Normal, FontWeight);
            FormattedText formatted = new FormattedText (Content, CultureInfo.CurrentCulture
                                                                , FlowDirection.LeftToRight, face, FontSize, null);
            //UsefullWidth = formatted.Width + FrameSpanOnEnd;

            double correctHeight = FontSize * _parentToChildCoeff;

            SetYourself (text.Width, correctHeight, text.TopOffset, text.LeftOffset);
            SetAlignment (text.Alignment);

            Padding = SetPadding ();
        }


        private TextLineViewModel ( TextLineViewModel source )
        {
            _dataSource = source._dataSource;
            FontSize = source.FontSize;
            FontFamily = new FontFamily (source._dataSource.FontFamily);
            FontWeight = source.FontWeight;
            Content = source.Content;
            IsSplitable = source.IsSplitable;
            Padding = new Thickness (0, -FontSize / _divider);
            //UsefullWidth = source.UsefullWidth;
            Foreground = source.Foreground;
            Background = source.Background;

            SetYourself (source.Width, source.Height, source.TopOffset, source.LeftOffset);

            Padding = SetPadding ();
        }


        private Thickness SetPadding ()
        {
            Thickness padding;
            double verticalPadding = ( FontSize - Height ) / 2;
            double df = Height / FontSize;
            padding = new Thickness (0, verticalPadding);

            return padding;
        }


        internal TextLineViewModel Clone ()
        {
            TextLineViewModel clone = new TextLineViewModel (this);
            return clone;
        }


        internal void ZoomOn ( double coefficient )
        {
            base.ZoomOn (coefficient);
            double degree = Math.Log (coefficient, 1.25);
            double coef = coefficient * Math.Pow (1.001, degree);

            //if ( coefficient == 2.5 ) 
            //{
            //    coef = coefficient * Math.Pow(1.001, degree);
            //}

            FontSize *= coef;

            Typeface face = new Typeface (FontFamily, FontStyle.Normal, Avalonia.Media.FontWeight.Bold);
            FormattedText formatted = new FormattedText (Content, CultureInfo.CurrentCulture
                                                                , FlowDirection.LeftToRight, face, FontSize, null);
            UsefullWidth = formatted.WidthIncludingTrailingWhitespace + ( FrameSpanOnEnd * _scale );

            //UsefullWidth *= coef;
           
            Padding = SetPadding ();
        }


        internal void ZoomOut ( double coefficient )
        {
            base.ZoomOut (coefficient);
            double degree = Math.Log (coefficient, 1.25);
            double coef = coefficient * Math.Pow (1.001, degree);

            //if ( coefficient == 2.5 )
            //{
            //    coef = coefficient * 1.006;
            //}

            FontSize /= coef;

            Typeface face = new Typeface (FontFamily, FontStyle.Normal, Avalonia.Media.FontWeight.Bold);
            FormattedText formatted = new FormattedText (Content, CultureInfo.CurrentCulture
                                                                , FlowDirection.LeftToRight, face, FontSize, null);
            UsefullWidth = formatted.WidthIncludingTrailingWhitespace + ( FrameSpanOnEnd * _scale );

            //UsefullWidth /= coef;
            
            Padding = SetPadding ();
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

            Typeface face = new Typeface (FontFamily, FontStyle.Normal, Avalonia.Media.FontWeight.Bold);
            FormattedText formatted = new FormattedText (Content, CultureInfo.CurrentCulture
                                                                , FlowDirection.LeftToRight, face, FontSize, null);
            UsefullWidth = formatted.WidthIncludingTrailingWhitespace + (FrameSpanOnEnd * _scale);
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

            Typeface face = new Typeface (FontFamily, FontStyle.Normal, Avalonia.Media.FontWeight.Bold);
            FormattedText formatted = new FormattedText (Content, CultureInfo.CurrentCulture
                                                                , FlowDirection.LeftToRight, face, FontSize, null);
            UsefullWidth = formatted.WidthIncludingTrailingWhitespace + (FrameSpanOnEnd * _scale);
            double proportion = FontSize / oldFontSize;
            Height *= proportion;
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
            if ( alignmentName == "Center" )
            {
                LeftOffset += ( Width - UsefullWidth ) / 2;
            }
        }


        internal List <TextLineViewModel> SplitYourself ( List<string> splittedContents, double scale, double layoutWidth )
        {
            List <TextLineViewModel> result = new List <TextLineViewModel>();
            double previousLeftOffset = LeftOffset;

            foreach ( string content   in   splittedContents )
            {
                TextLineViewModel newLine = new TextLineViewModel (this._dataSource);
                newLine.ZoomOn (scale);
                newLine.ReplaceContent (content);
                newLine.LeftOffset = previousLeftOffset;

                if ( newLine.LeftOffset >= layoutWidth - 2 ) 
                {
                    newLine.LeftOffset = LeftOffset;
                }
                
                newLine.TopOffset = TopOffset;
                previousLeftOffset += newLine.Width;
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

            Typeface face = new Typeface (new FontFamily ("arial"), FontStyle.Normal, Avalonia.Media.FontWeight.Bold);
            FormattedText formatted = new FormattedText (content, CultureInfo.CurrentCulture
                                                                , FlowDirection.LeftToRight, face, FontSize, null);
            Content = content;
            Width = formatted.WidthIncludingTrailingWhitespace;
        }
    }

}

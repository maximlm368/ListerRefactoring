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

namespace Lister.ViewModels
{
    public class TextLineViewModel : BadgeMember
    {
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

        private string cn;
        internal string Content
        {
            get { return cn; }
            private set
            {
                this.RaiseAndSetIfChanged (ref cn, value, nameof (Content));
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
        internal bool isWidthViolent = false;


        public TextLineViewModel ( TextualAtom text )
        {
            _dataSource = text;
            SetAlignmentByString (text.Alignment);
            FontSize = text.FontSize;
            FontFamily = new FontFamily (text.FontFamily);
            //FontFamily = new FontFamily ("sans-serif");
            Content = text.Content;
            IsSplitable = text.IsSplitable;

            Typeface face = new Typeface (new FontFamily ("arial"), FontStyle.Normal, Avalonia.Media.FontWeight.Normal);
            FormattedText formatted = new FormattedText (Content, CultureInfo.CurrentCulture
                                                                , FlowDirection.LeftToRight, face, FontSize, null);
            UsefullWidth = formatted.Width + 4;

            SetYourself (text.Width, text.Height, text.TopOffset, text.LeftOffset);
        }


        private TextLineViewModel ( TextLineViewModel text )
        {
            _dataSource = text._dataSource;
            Alignment = text.Alignment;
            FontSize = text._dataSource.FontSize;
            FontFamily = new FontFamily (text._dataSource.FontFamily);
            Content = text.Content;
            IsSplitable = text.IsSplitable;
            UsefullWidth = text.UsefullWidth;

            SetYourself (text._dataSource.Width, text._dataSource.Height, text._dataSource.TopOffset
                                                                        , text._dataSource.LeftOffset);
        }


        //private void SetInherited ( TextualAtom text )
        //{
        //    Typeface face = new Typeface (FontFamily, FontStyle.Normal, Avalonia.Media.FontWeight.Normal);
        //    FormattedText formatted = new FormattedText (Content, CultureInfo.CurrentCulture
        //                                                        , FlowDirection.LeftToRight, face, FontSize, null);
        //    double usefulWidth = formatted.WidthIncludingTrailingWhitespace;

        //    if ( usefulWidth > _dataSource.Width )
        //    {
        //        isWidthViolent = true;
        //    }

        //    double height = formatted.Height;

        //    SetYourself (usefulWidth, text.Height, text.TopOffset, text.LeftOffset);
        //}


        internal TextLineViewModel GetDimensionalOriginal () 
        {
            TextLineViewModel original = new TextLineViewModel (this);
            return original;
        }


        internal void ZoomOn ( double coefficient )
        {
            FontSize *= coefficient;
            UsefullWidth *= coefficient;
            base.ZoomOn (coefficient );
        }


        internal void ZoomOut ( double coefficient )
        {
            FontSize /= coefficient;
            UsefullWidth /= coefficient;
            base.ZoomOut (coefficient);
        }


        internal void Increase ( double additable )
        {
            FontSize += additable;
            Typeface face = new Typeface (new FontFamily ("arial"), FontStyle.Normal, Avalonia.Media.FontWeight.Normal);
            FormattedText formatted = new FormattedText (Content, CultureInfo.CurrentCulture
                                                                , FlowDirection.LeftToRight, face, FontSize, null);
            UsefullWidth = formatted.Width + 4;
            Width += additable;
            Height += additable;
        }


        internal void Reduce ( double subtractable )
        {
            FontSize -= subtractable;
            Typeface face = new Typeface (new FontFamily ("arial"), FontStyle.Normal, Avalonia.Media.FontWeight.Normal);
            FormattedText formatted = new FormattedText (Content, CultureInfo.CurrentCulture
                                                                , FlowDirection.LeftToRight, face, FontSize, null);
            UsefullWidth = formatted.Width + 4;
            Width -= subtractable;
            Height -= subtractable;
        }


        private void SetAlignmentByString ( string alignmentName ) 
        {
            if( alignmentName == "Left" ) 
            {
                Alignment = HorizontalAlignment.Left; 
            }
            if ( alignmentName == "Right" )
            {
                Alignment = HorizontalAlignment.Right;
            }
            if ( alignmentName == "Center" )
            {
                Alignment = HorizontalAlignment.Center;
            }
            else
            {
                Alignment = HorizontalAlignment.Center;
            }
        }


        internal List <TextLineViewModel> SplitYourself ( List<string> splittedContents, double scale, double layoutWidth )
        {
            List <TextLineViewModel> result = new List <TextLineViewModel>();
            double previousLeftOffset = LeftOffset;

            foreach ( string content   in   splittedContents )
            {
                TextLineViewModel newLine = new TextLineViewModel (this);
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

            Typeface face = new Typeface (new FontFamily ("arial"), FontStyle.Normal, Avalonia.Media.FontWeight.Normal);
            FormattedText formatted = new FormattedText (content, CultureInfo.CurrentCulture
                                                                , FlowDirection.LeftToRight, face, FontSize, null);
            Content = content;
            Width = formatted.WidthIncludingTrailingWhitespace;
            UsefullWidth = formatted.Width + 4;
        }
    }




}

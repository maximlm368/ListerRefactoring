using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Avalonia.Media;
using ReactiveUI;
using ContentAssembler;

namespace Lister.ViewModels
{
    public class TextLineViewModel : BadgeMember
    {
        private TextualAtom _dataSource;

        private string al;
        internal string Alignment
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
        internal bool IsShiftableBelow
        {
            get { return ish; }
            private set
            {
                this.RaiseAndSetIfChanged (ref ish, value, nameof (IsShiftableBelow));
            }
        }


        public TextLineViewModel ( TextualAtom text )
        {
            _dataSource = text;
            Alignment = text.Alignment;
            FontSize = text.FontSize;
            FontFamily = new FontFamily(text.FontFamily);
            Content = text.Content;
            IsShiftableBelow = text.IsShiftableBelow;

            SetYourself (text.Width, text.Height, text.TopOffset, text.LeftOffset);
        }


        private TextLineViewModel ( TextLineViewModel text )
        {
            _dataSource = text._dataSource;
            Alignment = text._dataSource.Alignment;
            FontSize = text._dataSource.FontSize;
            FontFamily = new FontFamily (text._dataSource.FontFamily);
            Content = text.Content;
            IsShiftableBelow = text.IsShiftableBelow;

            SetYourself (text._dataSource.Width, text._dataSource.Height, text._dataSource.TopOffset
                                                                        , text._dataSource.LeftOffset);
        }


        internal TextLineViewModel GetDimensionalOriginal () 
        {
            TextLineViewModel original = new TextLineViewModel (this);
            return original;
        }


        internal void ZoomOn ( double coefficient )
        {
            FontSize *= coefficient;
            base.ZoomOn (coefficient );
        }


        internal void ZoomOut ( double coefficient )
        {
            FontSize /= coefficient;
            base.ZoomOut (coefficient);
        }
    }




}

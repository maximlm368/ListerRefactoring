using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using ReactiveUI;
using ContentAssembler;

namespace Lister.ViewModels
{
    public class TextLineViewModel : BadgeMember
    {
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

        private string ff;
        internal string FontFamily
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
            Alignment = text.Alignment;
            FontSize = text.FontSize;
            FontFamily = text.FontFamily;
            Content = text.Content;
            IsShiftableBelow = text.IsShiftableBelow;

            SetYourself (text.Width, text.Height, text.TopOffset, text.LeftOffset);
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

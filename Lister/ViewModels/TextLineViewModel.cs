using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using ReactiveUI;

namespace Lister.ViewModels
{
    internal class TextLineViewModel : VisibleMember
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


        internal TextLineViewModel ( double width, double height, double topOffset, double leftOffset, string alignment
                           , double fontSize, string fontFamily, string content, bool isShiftableBelow )
        {
            
            Alignment = alignment;
            FontSize = fontSize;
            FontFamily = fontFamily;
            Content = content;
            IsShiftableBelow = isShiftableBelow;

            SetYourself (width, height, topOffset, leftOffset);
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

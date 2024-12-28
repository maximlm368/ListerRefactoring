using Avalonia;
using Avalonia.Media;
using ContentAssembler;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lister.ViewModels
{
    public class ShapeViewModel : BoundToTextLine
    {
        internal ShapeKind Kind { get; private set; }

        private Thickness _outlineThickness;
        internal Thickness OutlineThickness
        {
            get { return _outlineThickness; }
            private set
            {
                this.RaiseAndSetIfChanged (ref _outlineThickness, value, nameof (OutlineThickness));
            }
        }

        private SolidColorBrush _fillColor;
        internal SolidColorBrush FillColor
        {
            get { return _fillColor; }
            private set
            {
                if ( value != null )
                {
                    Red = value.Color.R;
                    Green = value.Color.G;
                    Blue = value.Color.B;
                }

                this.RaiseAndSetIfChanged (ref _fillColor, value, nameof (FillColor));
            }
        }

        private byte _red;
        internal byte Red
        {
            get { return _red; }
            private set { _red = value; }
        }

        private byte _green;
        internal byte Green
        {
            get { return _green; }
            private set { _green = value; }
        }

        private byte _blue;
        internal byte Blue
        {
            get { return _blue; }
            private set { _blue = value; }
        }


        public ShapeViewModel ( int id, InsideShape shape )
        {
            Id = id;
            Kind = shape.Kind;
            OutlineThickness = new Thickness (shape.StrokeThickness, shape.StrokeThickness);
            FillColor = new SolidColorBrush (new Color (255, shape.FillRGB [0], shape.FillRGB [1], shape.FillRGB [2]));
            Binding = shape.BindingName;
            IsAboveOfBinding = shape.IsAboveOfBinding;

            Color color = new Color (255, shape.OutlineRGB [0], shape.OutlineRGB [1], shape.OutlineRGB [2]);
            SolidColorBrush brush = new SolidColorBrush (color);

            SetYourself (shape.Width, shape.Height, shape.TopOffset, shape.LeftOffset, brush);
        }


        public ShapeViewModel ( ShapeViewModel prototype )
        {
            Id = prototype.Id;
            Kind = prototype.Kind;
            OutlineThickness = prototype.OutlineThickness;
            FillColor = new SolidColorBrush (new Color (255, prototype.Red, prototype.Blue, prototype.Green));
            Binding = prototype.Binding;
            IsAboveOfBinding = prototype.IsAboveOfBinding;

            SetYourself (prototype.Width, prototype.Height, prototype.TopOffset, prototype.LeftOffset
                                                                        , prototype.outlineColorStorage);
        }


        internal void ZoomOn ( double coefficient )
        {
            base.ZoomOn (coefficient);
        }


        internal void ZoomOut ( double coefficient )
        {
            base.ZoomOut (coefficient);
        }


        internal ShapeViewModel Clone ( )
        {
            return new ShapeViewModel ( this );
        }

    }
}
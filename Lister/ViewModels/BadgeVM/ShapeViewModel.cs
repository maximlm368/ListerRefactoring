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
    public class RectangleViewModel : ShapeViewModel
    {
        public RectangleViewModel ( InsideShape image ) : base (image) { }
    }



    public class EllipseViewModel : ShapeViewModel
    {
        public EllipseViewModel ( InsideShape image ) : base (image) { }
    }



    public class ShapeViewModel : BoundToMember
    {
        internal ShapeKind Kind { get; private set; }

        private SolidColorBrush _outlineRGB;
        internal SolidColorBrush OutlineRGB
        {
            get { return _outlineRGB; }
            private set
            {
                this.RaiseAndSetIfChanged (ref _outlineRGB, value, nameof (OutlineRGB));
            }
        }

        private Thickness _outlineThickness;
        internal Thickness OutlineThickness
        {
            get { return _outlineThickness; }
            private set
            {
                this.RaiseAndSetIfChanged (ref _outlineThickness, value, nameof (OutlineThickness));
            }
        }

        private SolidColorBrush _fillRGB;
        internal SolidColorBrush FillRGB
        {
            get { return _fillRGB; }
            private set
            {
                this.RaiseAndSetIfChanged (ref _fillRGB, value, nameof (FillRGB));
            }
        }


        public ShapeViewModel ( InsideShape shape )
        {
            Kind = shape.Kind;
            OutlineThickness = new Thickness (shape.OutlineThickness, shape.OutlineThickness);
            OutlineRGB = new SolidColorBrush(new Color(255, shape.OutlineRGB [0], shape.OutlineRGB [1], shape.OutlineRGB [2]));
            FillRGB = new SolidColorBrush (new Color (255, shape.FillRGB [0], shape.FillRGB [1], shape.FillRGB [2]));
            Binding = shape.BindingName;

            SetYourself (shape.Width, shape.Height, shape.TopOffset, shape.LeftOffset);
        }


        internal void ZoomOn ( double coefficient )
        {
            base.ZoomOn (coefficient);
        }


        internal void ZoomOut ( double coefficient )
        {
            base.ZoomOut (coefficient);
        }

    }
}
using Avalonia;
using Avalonia.Media;
using Core.Models.Badge;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lister.ViewModels
{
    internal class ShapeViewModel : BoundToTextLine
    {
        internal ShapeType Type { get; private set; }
        internal InsideImage Model { get; private set; }
        private SolidColorBrush _fillColor;
        internal SolidColorBrush FillColor
        {
            get { return _fillColor; }
            private set
            {
                this.RaiseAndSetIfChanged (ref _fillColor, value, nameof (FillColor));
            }
        }

        internal string FillColorHexStr { get; private set; }

       
        internal ShapeViewModel ( int id, InsideShape shape )
        {
            Id = id;
            Type = shape.Type;

            Color color;
            string hexStr = shape.FillHexStr;

            if ( ! Color.TryParse(shape.FillHexStr, out color) ) 
            {
                color = new Color (255, 0, 0, 0);
                hexStr = "#000000";
            }

            FillColorHexStr = hexStr;
            FillColor = new SolidColorBrush (color);
            Binding = shape.BindingName;
            IsAboveOfBinding = shape.IsAboveOfBinding;

            SetYourself (shape.Width, shape.Height, shape.TopOffset, shape.LeftOffset);
        }


        internal ShapeViewModel ( ShapeViewModel prototype )
        {
            Id = prototype.Id;
            Type = prototype.Type;
            FillColor = prototype.FillColor;
            Binding = prototype.Binding;
            IsAboveOfBinding = prototype.IsAboveOfBinding;

            SetYourself (prototype.Width, prototype.Height, prototype.TopOffset, prototype.LeftOffset);
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
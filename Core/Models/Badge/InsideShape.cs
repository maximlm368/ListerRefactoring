namespace Core.Models.Badge
{
    public class InsideShape : BindableToAnother
    {
        public string FillHexStr { get; private set; }
        public ShapeType Type { get; private set; }

        public InsideShape ( double outlineWidth, double outlineHeight
                           , double topShiftOnBackground, double leftShiftOnBackground
                           , string fillHexStr, string kind, string? bindingName, bool isAboveOfBinding )
        {
            Width = outlineWidth;
            Height = outlineHeight;
            TopOffset = topShiftOnBackground;
            LeftOffset = leftShiftOnBackground;
            FillHexStr = fillHexStr;
            BindingName = bindingName;
            IsAboveOfBinding = isAboveOfBinding;

            Type = TranslateStrToShapeType ( kind );

            if ( Type == ShapeType.nothing ) Trash ();
        }


        private void Trash ()
        {
            Width = 0;
            Height = 0;
        }


        private ShapeType TranslateStrToShapeType ( string kind )
        {
            if ( string.IsNullOrWhiteSpace ( kind ) )
            {
                return ShapeType.nothing;
            }

            if ( kind == "rectangle" || kind == "Rectangle" )
            {
                return ShapeType.rectangle;
            }
            else if ( kind == "ellipse" || kind == "Ellipse" )
            {
                return ShapeType.ellipse;
            }

            return ShapeType.nothing;
        }
    }
}
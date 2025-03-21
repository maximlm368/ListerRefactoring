namespace Lister.Core.Models.Badge;

public class ComponentShape : BindableToAnother
{
    private string _typeName;
    public string FillHexStr { get; private set; }
    public ShapeType Type { get; private set; }

    public ComponentShape ( double width, double height
                          , double topOffset, double leftOffset
                          , string fillHexStr, string typeName, string ? bindingName, bool isAboveOfBinding )
    {
        Width = width;
        Height = height;
        TopOffset = topOffset;
        LeftOffset = leftOffset;
        FillHexStr = fillHexStr;
        Binding = bindingName;
        IsAboveOfBinding = isAboveOfBinding;
        _typeName = typeName;
        Type = TranslateStrToShapeType ( typeName );

        if ( Type == ShapeType.nothing ) Trash ();
    }


    internal ComponentShape Clone ()
    {
        return new ComponentShape( Width, Height, TopOffset, LeftOffset, FillHexStr, _typeName
                                                                       , Binding, IsAboveOfBinding );
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

        if ( (kind == "rectangle") || (kind == "Rectangle") )
        {
            return ShapeType.rectangle;
        }
        else if ( (kind == "ellipse") || (kind == "Ellipse") )
        {
            return ShapeType.ellipse;
        }

        return ShapeType.nothing;
    }
}
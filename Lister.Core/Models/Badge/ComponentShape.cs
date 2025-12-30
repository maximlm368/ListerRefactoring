namespace Lister.Core.Models.Badge;

/// <summary>
/// Represents shape placed on badge.
/// </summary>
public sealed class ComponentShape : BindableToAnother
{
    private readonly string _typeName;
    public string FillHexStr { get; private set; }
    public ShapeType Type { get; private set; }

    public ComponentShape ( double width, double height
                          , double topOffset, double leftOffset
                          , string fillHexStr, string typeName, string? bindingName, bool isAboveOfBinding )
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
        return new ComponentShape ( Width, Height, TopOffset, LeftOffset, FillHexStr, _typeName
                                                                       , Binding, IsAboveOfBinding );
    }

    private void Trash ()
    {
        Width = 0;
        Height = 0;
    }

    private static ShapeType TranslateStrToShapeType ( string shapeType )
    {
        if ( string.IsNullOrWhiteSpace ( shapeType ) )
        {
            return ShapeType.nothing;
        }

        if ( ( shapeType == "rectangle" ) || ( shapeType == "Rectangle" ) )
        {
            return ShapeType.rectangle;
        }
        else if ( ( shapeType == "ellipse" ) || ( shapeType == "Ellipse" ) )
        {
            return ShapeType.ellipse;
        }

        return ShapeType.nothing;
    }
}
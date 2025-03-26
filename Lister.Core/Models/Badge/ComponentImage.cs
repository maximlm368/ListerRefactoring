namespace Lister.Core.Models.Badge;

/// <summary>
/// Represents image placed on badge.
/// </summary>
public sealed class ComponentImage : BindableToAnother
{
    public string Path { get; private set; }

    public ComponentImage ( string path, double Width, double Height, double topOffset
                          , double leftOffset, string ? bindingName, bool isAboveOfBinding )
    {
        Path = path;
        this.Width = Width;
        this.Height = Height;
        TopOffset = topOffset;
        LeftOffset = leftOffset;
        Binding = bindingName;
        IsAboveOfBinding = isAboveOfBinding;
    }


    internal ComponentImage Clone ( ) 
    {
        return new ComponentImage ( Path, Width, Height, TopOffset, LeftOffset, Binding, IsAboveOfBinding );
    }
}
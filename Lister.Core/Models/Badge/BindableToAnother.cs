namespace Lister.Core.Models.Badge;

/// <summary>
/// Defines badge layout component that can be bound to textline geometrically with name set in "Binding".
/// </summary>
public abstract class BindableToAnother : LayoutComponentBase
{
    public int Id { get; protected set; }
    public string ? Binding { get; set; }
    public bool IsAboveOfBinding { get; set; }
}
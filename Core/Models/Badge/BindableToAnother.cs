namespace Core.Models.Badge
{
    public abstract class BindableToAnother : LayoutComponent
    {
        public string? BindingName { get; set; }
        public bool IsAboveOfBinding { get; set; }
    }
}
namespace Core.Models.Badge
{
    public abstract class BindableToAnother : LayoutComponentBase
    {
        public int Id { get; protected set; }
        public string? Binding { get; set; }
        public bool IsAboveOfBinding { get; set; }
    }
}
namespace Core.Models.Badge
{
    public abstract class LayoutComponent
    {
        public double Width { get; protected set; }
        public double Height { get; protected set; }
        public double TopOffset { get; set; }
        public double LeftOffset { get; set; }
    }
}
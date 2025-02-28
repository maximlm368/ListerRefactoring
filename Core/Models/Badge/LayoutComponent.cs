namespace Core.Models.Badge
{
    public abstract class LayoutComponent
    {
        public double Width { get; protected set; }
        public double Height { get; protected set; }
        public double TopOffset { get; set; }
        public double LeftOffset { get; set; }

        public double TopOffsetWithBorder { get; set; }
        public double LeftOffsetWithBorder { get; set; }

        public double WidthWithBorder { get; set; }
        public double HeightWithBorder { get; set; }

        public void Shift ( double verticalDelta, double horizontalDelta )
        {
            TopOffset -= verticalDelta;
            LeftOffset -= horizontalDelta;
        }
    }
}
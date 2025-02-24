namespace Core.Models.Badge
{
    public class InsideImage : BindableToAnother
    {
        public string Path { get; private set; }

        public InsideImage ( string path, double Width, double Height, double topShiftOnBackground
                          , double leftShiftOnBackground, string? bindingName, bool isAboveOfBinding )
        {
            Path = path;
            this.Width = Width;
            this.Height = Height;
            TopOffset = topShiftOnBackground;
            LeftOffset = leftShiftOnBackground;
            BindingName = bindingName;
            IsAboveOfBinding = isAboveOfBinding;
        }
    }
}
namespace Core.Models.Badge
{
    public class TextualAtom : LayoutComponent
    {
        public string Name { get; private set; }
        public int NumberToLocate { get; private set; }
        public string Alignment { get; private set; }
        public double FontSize { get; private set; }
        public string FontName { get; private set; }
        public string ForegroundHexStr { get; private set; }
        public string FontWeight { get; private set; }
        private string _content;
        public string Content
        {
            get
            {
                return _content;
            }
            set
            {
                if ( !string.IsNullOrWhiteSpace ( value ) )
                {
                    _content = value;
                    ContentIsSet = true;
                }
            }
        }
        public List<string> IncludedAtoms { get; private set; }
        public bool IsSplitable { get; private set; }
        public bool ContentIsSet { get; private set; }
        public bool isNeeded;


        public TextualAtom ( string name, double width, double height, double topOffset, double leftOffset, string alignment
                           , double fontSize, string fontName, string foregroundHexStr
                           , string fontWeight, List<string>? includedAtoms, bool isSplitable, int numberToLocate )
        {
            _content = "";
            ContentIsSet = false;
            Name = name;
            Width = width;
            Height = height;
            TopOffset = topOffset;
            LeftOffset = leftOffset;
            Alignment = alignment;
            FontSize = fontSize;
            FontName = fontName;

            ForegroundHexStr = foregroundHexStr;

            FontWeight = fontWeight;
            IncludedAtoms = includedAtoms ?? new List<string> ();
            IsSplitable = isSplitable;
            NumberToLocate = numberToLocate;
            isNeeded = true;
        }


        public TextualAtom ( TextualAtom source, double topOffset, string content )
        {
            _content = content;
            ContentIsSet = true;
            Name = source.Name;
            Width = source.Width;
            Height = source.Height;
            TopOffset = source.TopOffset;
            LeftOffset = source.LeftOffset;
            Alignment = source.Alignment;
            FontSize = source.FontSize;
            FontName = source.FontName;
            ForegroundHexStr = source.ForegroundHexStr;
            FontWeight = source.FontWeight;
            IncludedAtoms = source.IncludedAtoms ?? new List<string> ();
            IsSplitable = source.IsSplitable;
            NumberToLocate = source.NumberToLocate;
            isNeeded = true;
        }


        internal TextualAtom Clone ()
        {
            TextualAtom clone = new TextualAtom ( Name, Width, Height, TopOffset, LeftOffset, Alignment, FontSize
                                                 , FontName, ForegroundHexStr, FontWeight, IncludedAtoms, IsSplitable
                                                 , NumberToLocate );
            return clone;
        }


        internal void TrimUnneededEdgeChar ( List<char> unNeeded )
        {
            bool charsAndContentExist = unNeeded != null && unNeeded.Count > 0 && unNeeded.Count > 0;

            foreach ( char symbol in unNeeded )
            {
                _content = _content.TrimStart ( symbol );
                _content = _content.TrimEnd ( symbol );
            }
        }
    }
}
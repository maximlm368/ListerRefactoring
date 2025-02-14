using System;
using System.IO;


namespace ContentAssembler
{
    public class BadgeExeption : Exception { }



    public class Badge
    {
        public Person Person { get; private set; }
        public string BackgroundImagePath { get; private set; }
        public BadgeLayout BadgeLayout { get; private set; }


        public Badge ( Person person, string backgroundImagePath, BadgeLayout layout )
        {
            bool isArgumentNull = ( person == null )   ||   ( backgroundImagePath == null )   ||   ( layout == null );

            if ( isArgumentNull )
            {
                throw new BadgeExeption ();
            }

            Person = person;
            BackgroundImagePath = backgroundImagePath;
            BadgeLayout = layout;

            Dictionary<string, string> personProperties = Person.GetProperties ();

            BadgeLayout.SetTextualValues ( personProperties );
            BadgeLayout.TrimEdgeCharsOfAtomContent ();
        }
    }



    public class BadgeLayout
    {
        private List<double> _spans;
        public string TemplateName { get; private set; }
        public double OutlineWidth { get; private set; }
        public double OutlineHeight { get; private set; }
        public double LeftSpan { get; private set; }
        public double TopSpan { get; private set; }
        public double RightSpan { get; private set; }
        public double BottomSpan { get; private set; }
        public List <TextualAtom> TextualFields { get; private set; }
        public List <InsideImage> InsideImages { get; private set; }
        public List <InsideShape> InsideShapes { get; private set; }


        public BadgeLayout ( double outlineWidth, double outlineHeight, string templateName, List<double> ? spans
                           , List <TextualAtom> ? textualFields, List <InsideImage> ? insideImages
                           , List <InsideShape> ? insideShapes )
        {
            OutlineWidth = outlineWidth;
            OutlineHeight = outlineHeight;
            TemplateName = templateName;
            _spans = spans;

            if ( (spans != null)   &&   (spans.Count == 4) ) 
            {
                LeftSpan = spans [0];
                TopSpan = spans [1];
                RightSpan = spans [2];
                BottomSpan = spans [3];
            }
            else 
            {
                LeftSpan = 0;
                TopSpan = 0;
                RightSpan = 0;
                BottomSpan = 0;
            }

            TextualFields = textualFields ?? new List <TextualAtom> ();
            InsideImages = insideImages ?? new List <InsideImage> ();
            InsideShapes = insideShapes ?? new List <InsideShape> ();
        }


        internal BadgeLayout Clone ( ) 
        {
            List <TextualAtom> atoms = new List <TextualAtom> ();
            
            foreach ( var atom   in   TextualFields ) 
            {
                atoms.Add (atom.Clone ());
            }

            BadgeLayout clone = 
                        new BadgeLayout (OutlineWidth, OutlineHeight, TemplateName, _spans, atoms, InsideImages, InsideShapes);
            return clone;
        }


        internal void SetTextualValues ( Dictionary <string, string> personProperties )
        {
            List <TextualAtom> includibles = new ( );
            List <TextualAtom> includings = new ( );

            AllocateValues ( personProperties, includibles );
            SetComplexValuesToIncludingAtoms (includings, includibles);

            List <TextualAtom> removables = new ( );

            foreach ( TextualAtom includedAtom   in   includibles )
            {
                if ( ! includedAtom.isNeeded )
                {
                    removables.Add ( includedAtom );
                }
            }

            foreach ( TextualAtom removable   in   removables )
            {
                TextualFields.Remove (removable);
            }

            TextualFields.Sort (new TextualAtomComparer <TextualAtom> ());
        }


        private void AllocateValues ( Dictionary<string, string> personProperties, List <TextualAtom> includibles ) 
        {
            foreach ( KeyValuePair <string, string> property   in   personProperties )
            {
                foreach ( TextualAtom atom   in   TextualFields )
                {
                    if ( atom.Name == property.Key )
                    {
                        atom.Content = property.Value;
                        includibles.Add (atom);
                        break;
                    }
                }
            }
        }


        private void SetComplexValuesToIncludingAtoms ( List <TextualAtom> includings, List <TextualAtom> includibles ) 
        {
            foreach ( TextualAtom atom   in   TextualFields )
            {
                bool atomIsIncluding = ! atom.ContentIsSet;

                if ( atomIsIncluding )
                {
                    string complexContent = "";

                    foreach ( string includedAtomName   in   atom.IncludedAtoms )
                    {
                        foreach ( TextualAtom includedAtom   in   includibles )
                        {
                            bool coincide = ( includedAtom.Name == includedAtomName );

                            if ( coincide )
                            {
                                complexContent += includedAtom.Content + " ";
                                includedAtom.isNeeded = false;
                                break;
                            }
                        }
                    }

                    atom.Content = complexContent;
                    includings.Add (atom);
                }
            }
        }


        internal void TrimEdgeCharsOfAtomContent (  ) 
        {
            List<char> unNeeded = new List<char> () { ' ', '"' };

            foreach ( TextualAtom atom   in   TextualFields )
            {
                atom.TrimUnneededEdgeChar ( unNeeded );
            }
        }

    }



    public class InsideImage : BindableToAnother
    {
        public string Path { get; private set; }

        public InsideImage ( string path, double Width, double Height , double topShiftOnBackground
                          , double leftShiftOnBackground, string ? bindingName, bool isAboveOfBinding )
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



    public class InsideShape : BindableToAnother
    {
        public string FillHexStr { get; private set; }
        public ShapeType Type { get; private set; }

        public InsideShape ( double outlineWidth, double outlineHeight
                           , double topShiftOnBackground, double leftShiftOnBackground
                           , string fillHexStr, string kind, string ? bindingName, bool isAboveOfBinding)
        {
            Width = outlineWidth;
            Height = outlineHeight;
            TopOffset = topShiftOnBackground;
            LeftOffset = leftShiftOnBackground;
            FillHexStr = fillHexStr;
            BindingName = bindingName;
            IsAboveOfBinding = isAboveOfBinding;

            Type = TranslateStrToShapeType (kind);

            if ( Type == ShapeType.nothing ) Trash ();
        }


        private void Trash ( )
        {
            Width = 0;
            Height = 0;
        }


        private ShapeType TranslateStrToShapeType ( string kind ) 
        {
            if ( string.IsNullOrWhiteSpace(kind) ) 
            {
                return ShapeType.nothing;
            }

            if ( (kind == "rectangle")   ||   ( kind == "Rectangle" ) ) 
            {
                return ShapeType.rectangle;
            }
            else if ( ( kind == "ellipse" ) || ( kind == "Ellipse" ) )
            {
                return ShapeType.ellipse;
            }

            return ShapeType.nothing;
        }
    }



    public class TextualAtom : LayoutMember
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
                if ( !string.IsNullOrWhiteSpace (value) )
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
            TextualAtom clone = new TextualAtom (Name, Width, Height, TopOffset, LeftOffset, Alignment, FontSize
                                                 , FontName, ForegroundHexStr, FontWeight, IncludedAtoms, IsSplitable
                                                 , NumberToLocate);
            return clone;
        }


        internal void TrimUnneededEdgeChar ( List<char> unNeeded )
        {
            bool charsAndContentExist = ( unNeeded != null ) && ( unNeeded.Count > 0 ) && ( unNeeded.Count > 0 );

            foreach ( char symbol in unNeeded )
            {
                _content = _content.TrimStart (symbol);
                _content = _content.TrimEnd (symbol);
            }
        }
    }



    public abstract class LayoutMember 
    {
        public double Width { get; protected set; }
        public double Height { get; protected set; }
        public double TopOffset { get; set; }
        public double LeftOffset { get; set; }
    }



    public abstract class BindableToAnother : LayoutMember
    {
        public string ? BindingName { get; set; }
        public bool IsAboveOfBinding { get; set; }
    }



    public class TextualAtomComparer <T> : IComparer <T> where T : TextualAtom
    {
        public int Compare ( T first, T second )
        {
            int result = -1;

            bool comparingShouldBe = (first != null)   &&   (second != null);

            if ( comparingShouldBe )
            {
                result = ( first.NumberToLocate - second.NumberToLocate );
            }

            return result;
        }
    }



    public enum ShapeType
    {
        rectangle = 0,
        ellipse = 1,
        nothing = 2
    }
}
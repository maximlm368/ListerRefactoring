using System;
using System.IO;
using System.Drawing;
using SkiaSharp;


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
        public Size OutlineSize { get; private set; }
        public List <TextualAtom> TextualFields { get; private set; }
        public List <InsideImage> InsideImages { get; private set; }


        public BadgeLayout ( Size outlineSize, List <TextualAtom> ? textualFields, List <InsideImage> ? insideImages )
        {
            OutlineSize = outlineSize;
            TextualFields = textualFields ?? new List <TextualAtom> ();
            InsideImages = insideImages ?? new List <InsideImage> ();
        }


        internal BadgeLayout Clone ( ) 
        {
            List<TextualAtom> atoms = new List<TextualAtom> ();
            
            foreach ( var atom   in   TextualFields ) 
            {
                atoms.Add (atom.Clone ());
            }

            BadgeLayout clone = new BadgeLayout (OutlineSize, atoms, InsideImages);
            return clone;
        }


        internal void SetTextualValues ( Dictionary <string, string> personProperties )
        {
            List<TextualAtom> includibles = new ( );
            List<TextualAtom> includings = new ( );

            AllocateValues ( personProperties, includibles );
            SetComplexValuesToIncludingAtoms (includings, includibles);

            List <TextualAtom> removable = new ( );

            foreach ( TextualAtom includedAtom   in   includibles )
            {
                if ( ! includedAtom.isNeeded )
                {
                    removable.Add ( includedAtom );
                }
            }

            List<TextualAtom> neededAtoms = new List<TextualAtom> ();
            neededAtoms.AddRange ( includibles );
            neededAtoms.AddRange (includings);

            //TextualFields.RemoveAt (3);

            TextualFields [1] = TextualFields [5];

            TextualFields.RemoveAt (2);
            TextualFields.RemoveAt (4);
            //TextualFields.RemoveAt (4);

           



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


        private void SetComplexValuesToIncludingAtoms ( List<TextualAtom> includings, List<TextualAtom> includibles ) 
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



    public class InsideImage
    {
        private readonly List<string> _geometryNames;
        public string Path { get; private set; }
        public Size Size { get; private set; }
        public string Color { get; private set; }
        public string GeometryElementName { get; private set; }
        public double TopShiftOnBackground { get; private set; }
        public double LeftShiftOnBackground { get; private set; }
        public ImageType ImageKind { get; private set; }

        public InsideImage ( string path, Size size, string color,
                           string geometryElementName, double topShiftOnBackground, double leftShiftOnBackground )
        {
            _geometryNames = SetGeometryNames ();
            Path = path;
            Size = size;
            Color = color;
            GeometryElementName = geometryElementName;
            TopShiftOnBackground = topShiftOnBackground;
            LeftShiftOnBackground = leftShiftOnBackground;

            bool isImageAbsent = string.IsNullOrWhiteSpace (Path);

            if ( isImageAbsent )
            {
                ImageKind = ImageType.geometricElement;
                bool areColorOrGeometryNameAbsent = string.IsNullOrWhiteSpace (Color)
                                                 || string.IsNullOrWhiteSpace (GeometryElementName);

                if ( areColorOrGeometryNameAbsent )
                {
                    ImageKind = ImageType.nothing;
                }
                else if ( ! _geometryNames.Contains (GeometryElementName) )
                {
                    ImageKind = ImageType.nothing;
                }
            }
        }


        private List<string> SetGeometryNames ()
        {
            List<string> result = new List<string> () { "Rectangle" };
            return result;
        }
    }



    public class TextualAtom
    {
        public string Name { get; private set; }
        public double Width { get; private set; }
        public double Height { get; private set; }
        public double TopOffset { get; set; }
        public double LeftOffset { get; private set; }
        public string Alignment { get; private set; }
        public double FontSize { get; private set; }
        public string FontFamily { get; private set; }
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
                           , double fontSize, string fontFamily, List<string>? includedAtoms, bool isSplitable )
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
            FontFamily = fontFamily;
            IncludedAtoms = includedAtoms ?? new List<string> ();
            IsSplitable = isSplitable;
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
            FontFamily = source.FontFamily;
            IncludedAtoms = source.IncludedAtoms ?? new List<string> ();
            IsSplitable = source.IsSplitable;
            isNeeded = true;
        }


        internal TextualAtom Clone () 
        {
            TextualAtom clone = new TextualAtom (Name, Width, Height, TopOffset, LeftOffset, Alignment, FontSize, FontFamily,
                                                                                             IncludedAtoms, IsSplitable);
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



    public enum ImageType
    {
        image = 0,
        geometricElement = 1,
        nothing = 2
    }
}
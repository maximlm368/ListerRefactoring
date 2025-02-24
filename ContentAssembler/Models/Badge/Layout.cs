namespace Core.Models.Badge
{
    public class Layout
    {
        private List<double> _paddinig;
        public string TemplateName { get; private set; }
        public double Width { get; private set; }
        public double Height { get; private set; }
        public double PaddingLeft { get; private set; }
        public double PaddingTop { get; private set; }
        public double PaddignRight { get; private set; }
        public double PaddingBottom { get; private set; }
        public List<TextualAtom> TextualFields { get; private set; }
        public List<InsideImage> InsideImages { get; private set; }
        public List<InsideShape> InsideShapes { get; private set; }


        public Layout ( double width, double height, string templateName, List<double>? padding
                           , List<TextualAtom>? textualFields, List<InsideImage>? insideImages
                           , List<InsideShape>? insideShapes )
        {
            Width = width;
            Height = height;
            TemplateName = templateName;
            _paddinig = padding;

            if ( padding != null && padding.Count == 4 )
            {
                PaddingLeft = padding [0];
                PaddingTop = padding [1];
                PaddignRight = padding [2];
                PaddingBottom = padding [3];
            }
            else
            {
                PaddingLeft = 0;
                PaddingTop = 0;
                PaddignRight = 0;
                PaddingBottom = 0;
            }

            TextualFields = textualFields ?? new List<TextualAtom> ();
            InsideImages = insideImages ?? new List<InsideImage> ();
            InsideShapes = insideShapes ?? new List<InsideShape> ();
        }


        internal Layout Clone ()
        {
            List<TextualAtom> atoms = new List<TextualAtom> ();

            foreach ( var atom in TextualFields )
            {
                atoms.Add (atom.Clone ());
            }

            Layout clone =
                        new Layout (Width, Height, TemplateName, _paddinig, atoms, InsideImages, InsideShapes);
            return clone;
        }


        internal void SetTextualValues ( Dictionary<string, string> personProperties )
        {
            List<TextualAtom> includibles = new ();
            List<TextualAtom> includings = new ();

            AllocateValues (personProperties, includibles);
            SetComplexValuesToIncludingAtoms (includings, includibles);

            List<TextualAtom> removables = new ();

            foreach ( TextualAtom includedAtom in includibles )
            {
                if ( !includedAtom.isNeeded )
                {
                    removables.Add (includedAtom);
                }
            }

            foreach ( TextualAtom removable in removables )
            {
                TextualFields.Remove (removable);
            }

            TextualFields.Sort (new TextualAtomComparer<TextualAtom> ());
        }


        private void AllocateValues ( Dictionary<string, string> personProperties, List<TextualAtom> includibles )
        {
            foreach ( KeyValuePair<string, string> property in personProperties )
            {
                foreach ( TextualAtom atom in TextualFields )
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
            foreach ( TextualAtom atom in TextualFields )
            {
                bool atomIsIncluding = !atom.ContentIsSet;

                if ( atomIsIncluding )
                {
                    string complexContent = "";

                    foreach ( string includedAtomName in atom.IncludedAtoms )
                    {
                        foreach ( TextualAtom includedAtom in includibles )
                        {
                            bool coincide = includedAtom.Name == includedAtomName;

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


        internal void TrimEdgeCharsOfAtomContent ()
        {
            List<char> unNeeded = new List<char> () { ' ', '"' };

            foreach ( TextualAtom atom in TextualFields )
            {
                atom.TrimUnneededEdgeChar (unNeeded);
            }
        }

    }
}
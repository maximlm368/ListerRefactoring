using System;
using System.IO;
using System.Drawing;
using SkiaSharp;


namespace ContentAssembler
{
    public class BadgeExeption : Exception { }



    public class Badgee
    {
        public Person Person { get; private set; }
        public string BackgroundImagePath { get; private set; }
        public BadgeLayout BadgeLayout { get; private set; }


        public Badgee ( Person person, string backgroundImagePath, BadgeLayout layout )
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
        public List<TextualAtom> TextualFields { get; private set; }
        public List<InsideImage> InsideImages { get; private set; }


        public BadgeLayout ( Size outlineSize, List<TextualAtom> ? textualFields, List<InsideImage> ? insideImages )
        {
            OutlineSize = outlineSize;
            TextualFields = textualFields ?? new List<TextualAtom> ();
            InsideImages = insideImages ?? new List<InsideImage> ();
        }


        internal void SetTextualValues ( Dictionary<string , string> personProperties )
        {
            List<TextualAtom> includibles = new ( );
            List<TextualAtom> includings = new ( );

            AllocateValues ( personProperties, includibles );
            SetComplexValuesToIncludingAtoms (includings, includibles);

            foreach ( TextualAtom includedAtom   in   includibles )
            {
                if ( ! includedAtom.isNeeded )
                {
                    includibles.Remove ( includedAtom );
                }
            }

            List<TextualAtom> neededAtoms = new List<TextualAtom> ();
            neededAtoms.AddRange ( includibles );
            neededAtoms.AddRange (includings);
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
}
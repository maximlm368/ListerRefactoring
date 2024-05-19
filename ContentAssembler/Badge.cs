using System;
using System.IO;
using System.Drawing;
using SkiaSharp;


namespace ContentAssembler
{
    public class Badgee
    {
        public Person Person { get; private set; }
        public string BackgroundImagePath { get; private set; }
        public BadgeLayout BadgeLayout { get; private set; }


        public Badgee ( Person person , string backgroundImagePath, BadgeLayout layout )
        {
            Person = person;
            BackgroundImagePath = backgroundImagePath;
            BadgeLayout = layout;

            Dictionary<string , string> personProperties = Person.GetProperties ( );
            BadgeLayout.SetTextualValues ( personProperties );


            
        }
    }



    public class BadgeLayout
    {
        public Size OutlineSize { get; private set; }
        public List<TextualAtom> TextualFields { get; private set; }
        public List<InsideImage> ? InsideImages { get; private set; }


        public BadgeLayout ( List<TextualAtom> textualFields, List<InsideImage> ? insideImages, Size outlineSize )
        {
            TextualFields = textualFields;
            OutlineSize = outlineSize;
            InsideImages = insideImages;
        }


        public void SetTextualValues ( Dictionary<string , string> personProperties )
        {
            List<TextualAtom> havingContent = new ( );
            List<TextualAtom> notHavingContent = new ( );

            foreach ( KeyValuePair<string , string> property   in   personProperties )
            {
                foreach ( TextualAtom atom in TextualFields )
                {
                    if ( atom.Name == property.Key )
                    {
                        atom.Content = property.Value;
                        havingContent.Add ( atom );
                        break;
                    }
                }
            }

            foreach ( TextualAtom atom   in   TextualFields )
            {
                if ( ! atom.contentIsSet )
                {
                    notHavingContent.Add ( atom );
                }
            }

        }


            
        
    }
}
namespace Core.Models.Badge
{
    public class Badge
    {
        public Person Person { get; private set; }
        public string BackgroundImagePath { get; private set; }
        public Layout BadgeLayout { get; private set; }


        private Badge ( Person person, string backgroundImagePath, Layout layout )
        {
            Person = person;
            BackgroundImagePath = backgroundImagePath;
            BadgeLayout = layout;

            Dictionary<string, string> personProperties = Person.GetProperties ();

            BadgeLayout.SetTextualValues (personProperties);
            BadgeLayout.TrimEdgeCharsOfAtomContent ();
        }


        public static Badge? GetBadge ( Person person, string backgroundImagePath, Layout layout )
        {
            bool isArgumentNull = ( person == null ) || ( backgroundImagePath == null ) || ( layout == null );

            if ( isArgumentNull )
            {
                return null;
            }

            return new Badge (person, backgroundImagePath, layout);
        }
    }

}
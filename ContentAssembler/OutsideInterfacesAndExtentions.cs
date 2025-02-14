namespace ContentAssembler 
{
    public interface IBadgeAppearenceProvider
    {
        public BadgeLayout GetBadgeLayout ( string badgeTemplateName );

        public Dictionary <BadgeLayout, KeyValuePair <string, List<string>>> GetBadgeLayouts ( );

        public string GetBadgeImageUri ( string templateName );
    }


    public interface IMemberColorProvider
    {
        public string GetIncorrectLineBackgroundStr ( string templateName );

        public string GetIncorrectMemberBorderStr ( string templateName );

        public string GetCorrectMemberBorderStr ( string templateName );

        public List<byte> GetIncorrectMemberBorderThickness ( string templateName );

        public List<byte> GetCorrectMemberBorderThickness ( string templateName );
    }


    public interface IPeopleSource 
    {
        public List <Person> ? GetPersons ( string ? personsFilePath, int gettingLimit );
    }


    public interface IPeopleSourceFactory
    {
        public IPeopleSource GetPeopleSource ( string ? personsFilePath );
    }
}


namespace ContentAssembler 
{
    public interface IBadgeAppearenceProvider
    {
        public BadgeLayout GetBadgeLayout ( string badgeTemplateName );

        public Dictionary <BadgeLayout, KeyValuePair <string, List<string>>> GetBadgeLayouts ( );

        public string GetBadgeImageUri ( string templateName );
    }


    public interface IBadLineColorProvider
    {
        public List<byte> GetBadLineColor ( string templateName );
    }


    public interface IPeopleSource 
    {
        public List <Person> GetPersons ( string ? personsFilePath, int gettingLimit );
    }


    public interface IPeopleSourceFactory
    {
        public IPeopleSource GetPeopleSource ( string ? personsFilePath );
    }
}


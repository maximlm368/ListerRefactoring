namespace ContentAssembler 
{
    public interface IBadgeAppearenceProvider
    {
        //public OrganizationalDataOfBadge GetBadgeData(string BadgeTemplateName);

        public BadgeLayout GetBadgeLayout ( string badgeTemplateName );

        public Dictionary <BadgeLayout, KeyValuePair <string, List<string>>> GetBadgeLayouts ( );

        public string GetBadgeBackgroundPath ( string templateName );
    }


    public interface IBadLineColorProvider
    {
        public List<byte> GetBadLineColor ( string templateName );
    }


    //public interface IFontFileProvider
    //{
    //    public Dictionary<string, string> GetTemplateFonts ( );
    //}


    public interface IPeopleSource 
    {
        public List <Person> GetPersons ( string ? personsFilePath );
    }


    public interface IPeopleSourceFactory
    {
        public IPeopleSource GetPeopleSource ( string ? personsFilePath );
    }
}


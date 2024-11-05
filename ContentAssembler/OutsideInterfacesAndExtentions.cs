namespace ContentAssembler 
{
    public interface IBadgeAppearenceProvider
    {
        //public OrganizationalDataOfBadge GetBadgeData(string BadgeTemplateName);

        public BadgeLayout GetBadgeLayout ( string badgeTemplateName );

        public List <TemplateName> GetBadgeTemplateNames ( );

        public string GetBadgeBackgroundPath ( string templateName );
    }


    public interface IBadLineColorProvider
    {
        public string GetBadLineColor ( string templateName );
    }


    public interface IFontFileProvider
    {
        public Dictionary<string, string> GetTemplateFonts ( );
    }


    public interface IPeopleSource 
    {
        public List <Person> GetPersons ( string ? personsFilePath );
    }


    public interface IPeopleSourceFactory
    {
        public IPeopleSource GetPeopleSource ( string ? personsFilePath );
    }


    public class TemplateName
    {
        public string name;
        public bool isFound;


        public TemplateName (string name, bool isFound ) 
        {
            this.name = name;
            this.isFound = isFound;
        }
    }
}


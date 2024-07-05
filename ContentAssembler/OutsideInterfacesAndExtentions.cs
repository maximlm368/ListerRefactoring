namespace ContentAssembler 
{
    public interface IBadgeAppearenceProvider
    {
        //public OrganizationalDataOfBadge GetBadgeData(string BadgeTemplateName);

        public BadgeLayout GetBadgeLayout ( string badgeTemplateName );

        public List <TemplateName> GetBadgeTemplateNames ( );

        public string GetBadgeBackgroundPath ( string templateName );
    }


    public interface IPeopleDataSource 
    {
        public List<Person> GetPersons(string? personsFilePath);

        public string sourcePath {  get; set; }
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


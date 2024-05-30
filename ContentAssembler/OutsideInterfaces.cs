namespace ContentAssembler 
{
    public interface IBadgeAppearenceProvider
    {
        public OrganizationalDataOfBadge GetBadgeData(string BadgeTemplateName);

        public BadgeLayout GetBadgeLayout ( string badgeTemplateName );

        public List<FileInfo> GetBadgeModelsNames();
    }


    public interface IPeopleDataSource 
    {
        public List<Person> GetPersons(string? personsFilePath);

        public string sourcePath {  get; set; }
    }
}


namespace ContentAssembler 
{
    public interface IBadgeAppearenceProvider
    {
        //public BadgeDimensions GetMainBadgeDimesions(string BadgeTemplateName);

        //public List<InsideImage> GetBadgeInsideImages(string BadgeTemplateName);

        public OrganizationalDataOfBadge GetBadgeData(string BadgeTemplateName);
        
        public List<FileInfo> GetBadgeModelsNames();
    }


    public interface IPeopleDataSource 
    {
        public List<Person> GetPersons(string? personsFilePath);

        public string sourcePath {  get; set; }
    }
}


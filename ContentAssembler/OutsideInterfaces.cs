namespace ContentAssembler 
{
    public interface IBadgeAppearenceDataSource
    {
        public BadgeDimensions GetMainBadgeDimesions(string BadgeModelName);

        public List<InsideImage> GetBadgeInsideImages(string BadgeModelName);

        public OrganizationalDataOfBadge GetBadgeData(string BadgeModelName);
        
        public List<FileInfo> GetBadgeModelsNames();
    }


    public interface IPeopleDataSource 
    {
        public List<Person> GetPersons(string? personsFilePath);

        public string sourcePath {  get; set; }
    }
}


using ContentAssembler;

namespace DataGateway
{
    public class ConfigFileBasedDataSource : IBadgeAppearenceDataSource
    {
        private string configFilePath;


        public ConfigFileBasedDataSource(string configFilePath) 
        {
            this.configFilePath = configFilePath;
        }


        public BadgeDimensions GetMainBadgeDimesions( string BadgeModelName ) 
        {


            return null;
        }


        public List<InsideImage> GetBadgeInsideImages( string BadgeModelName ) 
        {

            return null;
        }


        public OrganizationalDataOfBadge GetBadgeData (string badgeModelName)
        {
            double badgeWidth = 350;
            double badgeHeight = 212;
            Size badgeSize = new Size (badgeWidth, badgeHeight);

            double personTextAreaWidth = 220;
            double personTextAreaHeight = 147;
            Size personTextAreaSize = new Size (personTextAreaWidth, personTextAreaHeight);

            double personTextBlockTopShiftOnBackground = 65;
            double personTextBlockLeftShiftOnBackground = 130;

            double firstLevelFontSize = 30;
            double secondLevelFontSize = 16;
            double thirdLevelFontSize = 11;

            double firstLevelTBHeight = 37;
            double secondLevelTBHeight = 20;
            double thirdLevelTBHeight = 14;

            BadgeDimensions badgeDimensions = new BadgeDimensions( badgeSize, personTextAreaSize
                                                  , personTextBlockTopShiftOnBackground, personTextBlockLeftShiftOnBackground 
                                                  , firstLevelFontSize, secondLevelFontSize, thirdLevelFontSize
                                                  , firstLevelTBHeight, secondLevelTBHeight, thirdLevelTBHeight);

            OrganizationalDataOfBadge badgeDescriprion = new OrganizationalDataOfBadge (badgeDimensions, null);

            return badgeDescriprion;
        }


        public List<FileInfo> GetBadgeModelsNames ()
        {
            string badgeModelsFolderPath = "avares://Lister/Assets";
            badgeModelsFolderPath = "D:\\MML\\Lister\\Lister\\Lister\\Assets";
            DirectoryInfo modelFileDirectory = new DirectoryInfo(badgeModelsFolderPath);
            FileInfo[] Files = modelFileDirectory.GetFiles("*.jpg");
            List<FileInfo> modelNames = new List<FileInfo>();

            foreach (FileInfo file in Files)
            {
                modelNames.Add(file);
            }

            return modelNames;
        }
    }
}

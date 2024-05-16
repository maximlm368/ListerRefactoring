using ContentAssembler;
using ExtentionsAndAuxiliary;
using Microsoft.Extensions.Configuration;

namespace DataGateway
{
    public class ConfigFileBasedDataSource : IBadgeAppearenceDataSource
    {
        private string templatesFolderPath;
        private Dictionary<string, string> templateJsons;


        public ConfigFileBasedDataSource ( string templatesPath )
        {
            this.templatesFolderPath = templatesPath;
            DirectoryInfo containingDirectory = new DirectoryInfo ( templatesFolderPath );
            FileInfo [ ] fileInfos = containingDirectory.GetFiles ( "*.json" );
            templateJsons = new Dictionary<string , string> ( );

            foreach ( FileInfo fileInfo in fileInfos )
            {
                string jsonPath = fileInfo.FullName;
                string templateName = GetterFromJson.GetSectionValue ( new List<string> { "TemplateName" } , jsonPath );
                bool nameExists = !string.IsNullOrEmpty ( templateName );

                if ( nameExists )
                {
                    templateJsons.Add ( templateName , jsonPath );
                }
            }
        }


        public OrganizationalDataOfBadge GetBadgeData (string badgeTemplateName)
        {
            //string jsonPath = templateJsons [badgeTemplateName];
            //List<string> unitedNames = new ();

            //double badgeWidth = GetterFromJson.GetSectionValue (new List<string> { "Width" }, jsonPath).TranslateIntoDouble ();
            //double badgeHeight = 
            //GetterFromJson.GetSectionValue (new List<string> { "Height" }, jsonPath).TranslateIntoDouble ();
            //Size badgeSize = new Size (badgeWidth, badgeHeight);

            //IEnumerable<IConfigurationSection> unitings =
            //                      GetterFromJson.GetIncludedItemsOfSection (new List<string> { "UnitedTextBlocks : Items" }, jsonPath);
            //List<string> namesOfUnitedInOneUniting = new ();

            //foreach (IConfigurationSection unit in unitings ) 
            //{
            //    IConfigurationSection united = unit.GetSection ("United");
            //    List<string> unitedNames = GetterFromJson.GetChildrenValues (united);
            //}

            //TextualAtom familyName = BuildTextualAtom ("FamilyNameBlock", jsonPath);
            //TextualAtom firstName = BuildTextualAtom ("FirstNameBlock", jsonPath);
            //TextualAtom patronymicName = BuildTextualAtom ("PatronymicNameBlock", jsonPath);
            //TextualAtom post = BuildTextualAtom ("PostBlock", jsonPath);
            //TextualAtom department = BuildTextualAtom ("Department", jsonPath);



            double badgeWidth = 350;
            double badgeHeight = 214;
            Size badgeSize = new Size (badgeWidth, badgeHeight);

            double personTextAreaWidth = 220;
            double personTextAreaHeight = 147;
            Size personTextAreaSize = new Size (personTextAreaWidth, personTextAreaHeight);

            double personTextBlockTopShiftOnBackground = 60;
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


        public List<string> GetBadgeTemplateNames ()
        {
            List<string> templateNames = new ();

            foreach ( KeyValuePair <string, string> template   in   templateJsons )
            {
                templateNames.Add (template.Key);
            }

            return templateNames;
        }


        private TextualAtom BuildTextualAtom ( string atomName, string jsonPath ) 
        {
            



            double width = GetterFromJson.GetSectionValue (new List<string> { atomName, "Width" }, jsonPath)
                .TranslateIntoDouble ();
            double height = GetterFromJson.GetSectionValue (new List<string> { atomName, "Height" }, jsonPath)
            .TranslateIntoDouble ();
            double topOffset = GetterFromJson.GetSectionValue (new List<string> { atomName, "TopOffset" }, jsonPath)
            .TranslateIntoDouble ();
            double leftOffset = GetterFromJson.GetSectionValue (new List<string> { atomName, "LeftOffset" }, jsonPath)
            .TranslateIntoDouble ();
            string alignment = GetterFromJson.GetSectionValue (new List<string> { atomName, "Alignment" }, jsonPath);
            double fontSize = GetterFromJson.GetSectionValue (new List<string> { atomName, "FontSize" }, jsonPath)
            .TranslateIntoDouble ();
            string fontFamily = GetterFromJson.GetSectionValue (new List<string> { atomName, "FontFamily" }, jsonPath);

            TextualAtom atom = new TextualAtom (atomName, width, height, topOffset, leftOffset, alignment, fontSize, fontFamily);
            return atom;
        }


        public List<FileInfo> GetBadgeModelsNames ()
        {
            string badgeModelsFolderPath = @"./";
            DirectoryInfo modelFileDirectory = new DirectoryInfo (badgeModelsFolderPath);
            FileInfo [] Files = modelFileDirectory.GetFiles ("*.jpg");
            List<FileInfo> modelNames = new List<FileInfo> ();

            foreach ( FileInfo file in Files )
            {
                modelNames.Add (file);
            }

            return modelNames;
        }


        

    }
}

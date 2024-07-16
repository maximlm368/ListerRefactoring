using ContentAssembler;
using ExtentionsAndAuxiliary;
using Microsoft.Extensions.Configuration;
//using QuestPDF.Infrastructure;

namespace DataGateway
{
    public interface IBadLineColorProvider
    {
        public string GetBadLineColor ( string templateName );
    }



    public class BadgeAppearenceProvider : IBadgeAppearenceProvider, IBadLineColorProvider
    {
        private readonly string _defaultColor = "150,150,150";
        private string _templatesFolderPath;
        private List<string> _textualAtomNames;
        private Dictionary<string, string> _nameAndJson;
        private Dictionary<string, string> _nameAndColor;


        public BadgeAppearenceProvider (  )
        {
            _textualAtomNames = new List<string> ( ) {"FamilyName", "FirstName", "PatronymicName", "Post", "Department"};
            _templatesFolderPath = @"./";
            DirectoryInfo containingDirectory = new DirectoryInfo ( _templatesFolderPath );
            FileInfo [ ] fileInfos = containingDirectory.GetFiles ( "*.json" );
            _nameAndJson = new Dictionary<string , string> ( );
            _nameAndColor = new Dictionary<string , string> ( );

            foreach ( FileInfo fileInfo   in   fileInfos )
            {
                string jsonPath = fileInfo.FullName;
                string templateName = GetterFromJson.GetSectionValue ( new List<string> { "TemplateName" }, jsonPath );
                bool hasNameExist = ! string.IsNullOrEmpty ( templateName );

                if ( hasNameExist )
                {
                    _nameAndJson.Add ( templateName, jsonPath );
                }

                string color = GetterFromJson.GetSectionValue (new List<string> { "IncorrectLineBackground" }, jsonPath);
                hasNameExist = ! string.IsNullOrEmpty (templateName);

                if ( hasNameExist )
                {
                    _nameAndColor.Add (templateName, color);
                }
            }
        }


        public string GetBadgeBackgroundPath ( string templateName )
        {
            string jsonPath = _nameAndJson [ templateName ];
            string backgroundPath = GetterFromJson.GetSectionValue ( new List<string> { "BackgroundImagePath" } , jsonPath );
            string directoryPath = System.IO.Directory.GetCurrentDirectory ();
            backgroundPath = "file:///" + directoryPath + "//" + backgroundPath;
            return backgroundPath;
        }


        public string GetBadLineColor ( string templateName )
        {
            string color = _nameAndColor [templateName];
            return color;
        }


        public BadgeLayout GetBadgeLayout ( string templateName )
        {
            string jsonPath = _nameAndJson [ templateName ];

            double badgeWidth = 
                      GetterFromJson.GetSectionValue ( new List<string> { "Width" }, jsonPath ).TranslateIntoDouble ( );
            double badgeHeight =
                      GetterFromJson.GetSectionValue ( new List<string> { "Height" }, jsonPath ).TranslateIntoDouble ( );
            Size badgeSize = new Size ( badgeWidth, badgeHeight );

            List<int> intSpans = GetterFromJson.GetSectionValue (new List<string> { "InsideSpan" }, jsonPath).TranslateIntoIntList ( );
            List<double> spans = new List<double> ();

            foreach ( int item   in   intSpans )
            {
                spans.Add (( double ) item);
            }

            List <TextualAtom> atoms = GetAtoms ( jsonPath );
            SetUnitingAtoms (atoms, jsonPath);

            List <InsideImage> pictures = GetImages ( jsonPath );
            BadgeLayout result = new BadgeLayout (badgeSize, templateName, spans, atoms, pictures);

            return result;
        }


        private List <TextualAtom> GetAtoms ( string jsonPath ) 
        {
            List<TextualAtom> atoms = new ();

            atoms.Add (BuildTextualAtom ("FamilyName", jsonPath));
            atoms.Add (BuildTextualAtom ("FirstName", jsonPath));
            atoms.Add (BuildTextualAtom ("PatronymicName", jsonPath));
            atoms.Add (BuildTextualAtom ("Post", jsonPath));
            atoms.Add (BuildTextualAtom ("Department", jsonPath));

            return atoms;
        }


        private void SetUnitingAtoms ( List<TextualAtom> atoms, string jsonPath ) 
        {
            IEnumerable <IConfigurationSection> unitings =
                          GetterFromJson.GetIncludedItemsOfSection (new List<string> { "UnitedTextBlocks" }, jsonPath);

            foreach ( IConfigurationSection unit   in   unitings )
            {
                IConfigurationSection unitedSection = unit.GetSection ("United");
                IEnumerable <IConfigurationSection> unitedSections = GetterFromJson.GetChildrenOfSection (unitedSection);
                List<string> unitedAtomsNames = new List<string> ();

                foreach ( IConfigurationSection name   in   unitedSections )
                {
                    unitedAtomsNames.Add (name.Value);
                }

                TextualAtom unitingAtom = BuildTextualAtom (unit, unitedAtomsNames);
                atoms.Add (unitingAtom);
            }
        }


        private List<InsideImage> GetImages ( string jsonPath ) 
        {
            List <InsideImage> pictures = new ();

            IEnumerable <IConfigurationSection> images =
                              GetterFromJson.GetIncludedItemsOfSection (new List<string> { "InsideImages" }, jsonPath);

            foreach ( IConfigurationSection image in images )
            {
                InsideImage picture = BuildInsideImage (image);
                pictures.Add (picture);
            }

            return pictures;
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

            List<int> intForeground = GetterFromJson.GetSectionValue (new List<string> { atomName, "Foreground" }, jsonPath)
            .TranslateIntoIntList();

            List<byte> foreground = new List<byte> ();

            foreach ( int item   in   intForeground ) 
            {
                foreground.Add ( (byte) item);
            }

            string fontWeight = GetterFromJson.GetSectionValue (new List<string> { atomName, "FontWeight" }, jsonPath);
            string shiftableString = GetterFromJson.GetSectionValue (new List<string> { atomName, "IsSplitable" }, jsonPath);
            bool isShiftable = false;

            try 
            {
                int shiftableInt = Int32.Parse (shiftableString);
                isShiftable = Convert.ToBoolean (shiftableInt);
            }
            catch (Exception ex) {}

            TextualAtom atom = new TextualAtom (atomName, width, height, topOffset, leftOffset
                           , alignment, fontSize, fontFamily, foreground, fontWeight, null, isShiftable);

            return atom;
        }


        private TextualAtom BuildTextualAtom ( IConfigurationSection section, List<string> united )
        {
            IConfigurationSection childSection = section.GetSection ("Name");
            string atomName = GetterFromJson.GetSectionValue (childSection);
            childSection = section.GetSection ("Width");
            double width = GetterFromJson.GetSectionValue (childSection).TranslateIntoDouble ();
            childSection = section.GetSection ("Height");
            double height = GetterFromJson.GetSectionValue (childSection).TranslateIntoDouble ();
            childSection = section.GetSection ("TopOffset");
            double topOffset = GetterFromJson.GetSectionValue (childSection).TranslateIntoDouble ();
            childSection = section.GetSection ("LeftOffset");
            double leftOffset = GetterFromJson.GetSectionValue (childSection).TranslateIntoDouble ();
            childSection = section.GetSection ("Alignment");
            string alignment = GetterFromJson.GetSectionValue (childSection);
            childSection = section.GetSection ("FontSize");
            double fontSize = GetterFromJson.GetSectionValue (childSection).TranslateIntoDouble ();
            childSection = section.GetSection ("FontFamily");
            string fontFamily = GetterFromJson.GetSectionValue (childSection);

            childSection = section.GetSection ("Foreground");
            List<int> intForeground = GetterFromJson.GetSectionValue (childSection).TranslateIntoIntList ();

            List<byte> foreground = new List<byte> ();

            foreach ( int item   in   intForeground )
            {
                foreground.Add (( byte ) item);
            }

            childSection = section.GetSection ("FontWeight");
            string fontWeight = GetterFromJson.GetSectionValue (childSection);

            childSection = section.GetSection ("IsSplitable");
            string shiftableString = GetterFromJson.GetSectionValue (childSection);
            bool isShiftable = false;

            try
            {
                int shiftableInt = Int32.Parse (shiftableString);
                isShiftable = Convert.ToBoolean (shiftableInt);
            }
            catch ( Exception ex ) { }


            TextualAtom atom = new TextualAtom ( atomName, width, height, topOffset, leftOffset 
                                               , alignment, fontSize, fontFamily, foreground, fontWeight, united, isShiftable );
            return atom;
        }


        private InsideImage BuildInsideImage ( IConfigurationSection section )
        {
            IConfigurationSection childSection = section.GetSection ( "Name" );
            string imageName = GetterFromJson.GetSectionValue ( childSection );
            
            childSection = section.GetSection ( "Width" );
            double width = GetterFromJson.GetSectionValue ( childSection ).TranslateIntoDouble ( );
            
            childSection = section.GetSection ( "Height" );
            double height = GetterFromJson.GetSectionValue ( childSection ).TranslateIntoDouble ( );
            
            childSection = section.GetSection ( "TopOffset" );
            double topOffset = GetterFromJson.GetSectionValue ( childSection ).TranslateIntoDouble ( );

            childSection = section.GetSection ( "LeftOffset" );
            double leftOffset = GetterFromJson.GetSectionValue ( childSection ).TranslateIntoDouble ( );

            childSection = section.GetSection ( "ImagePath" );
            string path = GetterFromJson.GetSectionValue ( childSection );

            childSection = section.GetSection ( "Color" );
            string color = GetterFromJson.GetSectionValue ( childSection );
            
            childSection = section.GetSection ( "GeometricElementName" );
            string geometricElement = GetterFromJson.GetSectionValue ( childSection );

            InsideImage image = 
                       new InsideImage ( path, new Size ( width , height ), color, geometricElement, topOffset, leftOffset );
            return image;
        }


        public List <TemplateName> GetBadgeTemplateNames ( )
        {
            List <TemplateName> templateNames = new ();

            foreach ( KeyValuePair<string, string> template   in   _nameAndJson )
            {
                string jsonPath = template.Value;
                string backgroundPath = GetterFromJson.GetSectionValue ( new List<string> { "BackgroundImagePath" } , jsonPath );
                string directoryPath = System.IO.Directory.GetCurrentDirectory ( );
                string imagePath = directoryPath + "/" + backgroundPath;
                bool isFound = true;

                try
                {
                    using Stream stream = new FileStream ( imagePath , FileMode.Open );
                }
                catch ( FileNotFoundException ex )
                {
                    isFound = false;
                }

                templateNames.Add (new TemplateName (template.Key, isFound));
            }

            return templateNames;
        }


        //public List<FileInfo> GetBadgeModelsNames ()
        //{
        //    string badgeModelsFolderPath = @"./";
        //    DirectoryInfo modelFileDirectory = new DirectoryInfo (badgeModelsFolderPath);
        //    FileInfo [] Files = modelFileDirectory.GetFiles ("*.jpg");
        //    List<FileInfo> modelNames = new List<FileInfo> ();

        //    foreach ( FileInfo file in Files )
        //    {
        //        modelNames.Add (file);
        //    }

        //    return modelNames;
        //}


        //public List<FileInfo> GetBadgeModelsNamess ()
        //{
        //    string badgeModelsFolderPath = @"./";
        //    DirectoryInfo modelFileDirectory = new DirectoryInfo (badgeModelsFolderPath);
        //    FileInfo [] Files = modelFileDirectory.GetFiles ("*.jpg");
        //    List<FileInfo> modelNames = new List<FileInfo> ();

        //    foreach ( FileInfo file in Files )
        //    {
        //        modelNames.Add (file);
        //    }

        //    return modelNames;
        //}


        //public OrganizationalDataOfBadge GetBadgeData ( string badgeTemplateName )
        //{
        //    double badgeWidth = 350;
        //    double badgeHeight = 214;
        //    Size badgeSize = new Size ( badgeWidth , badgeHeight );

        //    double personTextAreaWidth = 220;
        //    double personTextAreaHeight = 147;
        //    Size personTextAreaSize = new Size ( personTextAreaWidth , personTextAreaHeight );

        //    double personTextBlockTopShiftOnBackground = 60;
        //    double personTextBlockLeftShiftOnBackground = 130;

        //    double firstLevelFontSize = 30;
        //    double secondLevelFontSize = 16;
        //    double thirdLevelFontSize = 11;

        //    double firstLevelTBHeight = 37;
        //    double secondLevelTBHeight = 20;
        //    double thirdLevelTBHeight = 14;

        //    BadgeDimensions badgeDimensions = new BadgeDimensions ( badgeSize , personTextAreaSize
        //                                          , personTextBlockTopShiftOnBackground , personTextBlockLeftShiftOnBackground
        //                                          , firstLevelFontSize , secondLevelFontSize , thirdLevelFontSize
        //                                          , firstLevelTBHeight , secondLevelTBHeight , thirdLevelTBHeight );

        //    OrganizationalDataOfBadge badgeDescriprion = new OrganizationalDataOfBadge ( badgeDimensions , null );

        //    return badgeDescriprion;
        //}

    }
}

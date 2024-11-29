using ContentAssembler;
using ExtentionsAndAuxiliary;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Text.Json;

using NJsonSchema;
using Newtonsoft.Json.Linq;
using NJsonSchema.Validation;

//using Newtonsoft.Json.Linq;
//using Newtonsoft.Json.Schema;


//using Json.Schema;

using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml.Office2013.Word;
using System.Linq;
using System.Text.Json.Nodes;
using System.ComponentModel;
//using Newtonsoft.Json.Schema;


namespace DataGateway
{
    public class BadgeAppearenceProvider : IBadgeAppearenceProvider, IBadLineColorProvider
    {
        private string _resourceUri;
        private string _resourceFolder;
        private FileInfo _schemeFile;

        private readonly string _defaultColor = "150,150,150";
        private readonly string _defaultSplitability = "0";
        
        private Dictionary<string, string> _nameAndJson;
        private Dictionary<string, List<byte>> _nameAndColor;
        private Dictionary<string, ICollection <ValidationError>> _jsonAndErrors;


        public BadgeAppearenceProvider ( string resourceUri, string resourceFolder, string jsonSchemeFolder )
        {
            _resourceUri = resourceUri;
            _resourceFolder = resourceFolder;

            DirectoryInfo containingDirectory = new DirectoryInfo (jsonSchemeFolder);
            FileInfo [] containingFiles = containingDirectory.GetFiles ("*.json");
            _schemeFile = containingFiles [0];

            containingDirectory = new DirectoryInfo (_resourceFolder);
            FileInfo [] fileInfos = containingDirectory.GetFiles ("*.json");
            _nameAndJson = new ();
            _nameAndColor = new ();
            _jsonAndErrors = new ();

            foreach ( FileInfo fileInfo   in   fileInfos )
            {
                string jsonPath = fileInfo.FullName;
                string templateName = GetSectionStrValue (new List<string> { "TemplateName" }, jsonPath) ?? "";

                bool nameShouldBeAdded = ! string.IsNullOrEmpty (templateName)
                                      &&  !_nameAndJson.ContainsKey (templateName);

                if ( nameShouldBeAdded )
                {
                    _nameAndJson.Add (templateName, jsonPath);
                }

                bool colorShouldBeAdded = (nameShouldBeAdded   &&   ! string.IsNullOrEmpty (templateName));

                if ( colorShouldBeAdded )
                {
                    List<byte> color = CreateColor (jsonPath);
                    _nameAndColor.Add (templateName, color);
                }
            }
        }


        private List<byte> CreateColor ( string jsonPath )
        {
            List<byte> color = new ();

            byte red = (byte) GetSectionIntValueOrDefault (new List<string> { "IncorrectLineBackground", "Red" }, jsonPath);
            color.Add (red);

            byte green = (byte) GetSectionIntValueOrDefault (new List<string> { "IncorrectLineBackground", "Green" }, jsonPath);
            color.Add (green);

            byte blue = (byte) GetSectionIntValueOrDefault (new List<string> { "IncorrectLineBackground", "Blue" }, jsonPath);
            color.Add (blue);

            return color;
        }


        public string GetBadgeBackgroundPath ( string templateName )
        {
            string jsonPath = _nameAndJson [ templateName ];
            string backgroundPath = GetSectionStrValueOrDefault ( new List<string> { "BackgroundImagePath" } , jsonPath) ?? "";
            backgroundPath = _resourceUri + backgroundPath;
            return backgroundPath;
        }


        public List<byte> GetBadLineColor ( string templateName )
        {
            return _nameAndColor [templateName];
        }


        public BadgeLayout GetBadgeLayout ( string templateName )
        {
            string jsonPath = _nameAndJson [ templateName ];

            double badgeWidth = GetSectionIntValueOrDefault ( new List<string> { "Width" }, jsonPath );
            double badgeHeight = GetSectionIntValueOrDefault (new List<string> { "Height" }, jsonPath);

            List<double> spans = new List<double> ();

            double leftSpan = GetSectionIntValueOrDefault (new List<string> { "InsideSpan", "Left" }, jsonPath);
            spans.Add (leftSpan);

            double topSpan = GetSectionIntValueOrDefault (new List<string> { "InsideSpan", "Top" }, jsonPath);
            spans.Add(topSpan);

            double rightSpan = GetSectionIntValueOrDefault (new List<string> { "InsideSpan", "Right" }, jsonPath);
            spans.Add (rightSpan);

            double bottomSpan = GetSectionIntValueOrDefault (new List<string> { "InsideSpan", "Bottom" }, jsonPath);
            spans.Add (bottomSpan);

            List <TextualAtom> atoms = GetAtoms ( jsonPath );
            SetUnitingAtoms (atoms, jsonPath);

            List <InsideImage> pictures = GetImages ( jsonPath );

            BadgeLayout result = new BadgeLayout (badgeWidth, badgeHeight, templateName, spans, atoms, pictures);

            return result;
        }


        private List <TextualAtom> GetAtoms ( string jsonPath ) 
        {
            List <TextualAtom> atoms = new ();

            atoms.Add (BuildTextualAtom ("FamilyName", jsonPath, 0));
            atoms.Add (BuildTextualAtom ("FirstName", jsonPath, 1));
            atoms.Add (BuildTextualAtom ("PatronymicName", jsonPath, 2));
            atoms.Add (BuildTextualAtom ("Post", jsonPath, 3));
            atoms.Add (BuildTextualAtom ("Department", jsonPath, 4));

            return atoms;
        }


        private void SetUnitingAtoms ( List <TextualAtom> atoms, string jsonPath ) 
        {
            IEnumerable <IConfigurationSection> items =
                          GetterFromJson.GetIncludedItemsOfSection (new List<string> { "UnitedTextBlocks" }, jsonPath);

            if ( items.Count() < 1 ) 
            {
                return;
            }

            foreach ( IConfigurationSection item   in   items )
            {
                string path = item.Path;

                string numberStr = GetterFromJson.GetSectionValue (item.GetSection ("Number"));

                int number = 0;

                try
                {
                    number = int.Parse (numberStr);
                }
                catch ( Exception ex ) 
                {
                    number = 1;
                }

                IConfigurationSection unitedSection = item.GetSection ("United");
                IEnumerable <IConfigurationSection> unitedSections = unitedSection.GetChildren();
                List<string> unitedAtomsNames = new List<string> ();

                foreach ( IConfigurationSection name   in   unitedSections )
                {
                    unitedAtomsNames.Add (name.Value);
                }

                TextualAtom unitingAtom = BuildTextualAtom (item, jsonPath, unitedAtomsNames, number);
                atoms.Add (unitingAtom);
            }
        }


        private List <InsideImage> GetImages ( string jsonPath ) 
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


        private TextualAtom BuildTextualAtom ( string atomName, string jsonPath, int numberToLocate ) 
        {
            double width = GetSectionIntValueOrDefault (new List<string> { atomName, "Width" }, jsonPath);
            double height = GetSectionIntValueOrDefault (new List<string> { atomName, "Height" }, jsonPath);
            double topOffset = GetSectionIntValueOrDefault (new List<string> { atomName, "TopOffset" }, jsonPath);
            double leftOffset = GetSectionIntValueOrDefault (new List<string> { atomName, "LeftOffset" }, jsonPath);
            string alignment = GetSectionStrValueOrDefault (new List<string> { atomName, "Alignment" }, jsonPath) ?? "";
            double fontSize = GetSectionIntValueOrDefault (new List<string> { atomName, "FontSize" }, jsonPath);
            string fontFile = GetSectionStrValueOrDefault (new List<string> { atomName, "FontFile" }, jsonPath) ?? "";
            string fontName = GetSectionStrValueOrDefault (new List<string> { atomName, "FontName" }, jsonPath) ?? "";

            List<byte> foreground = new List<byte> ();

            byte foregroundRed = 
                (byte) GetSectionIntValueOrDefault (new List<string> { atomName, "Foreground", "Red" }, jsonPath);
            foreground.Add (foregroundRed);

            byte foregroundGreen = 
                (byte) GetSectionIntValueOrDefault (new List<string> { atomName, "Foreground", "Green" }, jsonPath);
            foreground.Add (foregroundGreen);

            byte foregroundBlue = 
                (byte) GetSectionIntValueOrDefault (new List<string> { atomName, "Foreground", "Blue" }, jsonPath);
            foreground.Add (foregroundBlue);

            string fontWeight = GetSectionStrValueOrDefault (new List<string> { atomName, "FontWeight" }, jsonPath) ?? "";
            bool isShiftable = GetSectionBoolValueOrDefault (new List<string> { atomName, "IsSplitable" }, jsonPath);

            TextualAtom atom = new TextualAtom (atomName, width, height, topOffset, leftOffset
                                               , alignment, fontSize, fontFile, fontName, foreground
                                             , fontWeight, null, isShiftable, numberToLocate);

            return atom;
        }


        private TextualAtom BuildTextualAtom 
                        ( IConfigurationSection section, string jsonPath, List<string> united, int numberToLocate )
        {
            IConfigurationSection childSection = section.GetSection ("Name");
            string atomName = GetterFromJson.GetSectionValue (childSection) ?? "";

            childSection = section.GetSection ("Width");
            double width = GetSectionIntValueOrDefault (childSection, jsonPath, "Width");

            childSection = section.GetSection ("Height");
            double height = GetSectionIntValueOrDefault (childSection, jsonPath, "Height");

            childSection = section.GetSection ("TopOffset");
            double topOffset = GetSectionIntValueOrDefault (childSection, jsonPath, "TopOffset");

            childSection = section.GetSection ("LeftOffset");
            double leftOffset = GetSectionIntValueOrDefault (childSection, jsonPath, "LeftOffset");

            childSection = section.GetSection ("Alignment");
            string alignment = GetSectionStrValueOrDefault (childSection, jsonPath, "Alignment");

            childSection = section.GetSection ("FontSize");
            double fontSize = GetSectionIntValueOrDefault (childSection, jsonPath, "FontSize");

            childSection = section.GetSection ("FontFile");
            string fontFile = GetSectionStrValueOrDefault (childSection, jsonPath, "FontFile");

            childSection = section.GetSection ("FontName");
            string fontName = GetSectionStrValueOrDefault (childSection, jsonPath, "FontName");

            childSection = section.GetSection ("Foreground");
            childSection = section.GetSection ("Red");
            byte red = (byte) GetSectionIntValueOrDefault (childSection, jsonPath, "Red");

            childSection = section.GetSection ("Foreground");
            childSection = section.GetSection ("Green");
            byte green = ( byte ) GetSectionIntValueOrDefault (childSection, jsonPath, "Green");

            childSection = section.GetSection ("Foreground");
            childSection = section.GetSection ("Blue");
            byte blue = ( byte ) GetSectionIntValueOrDefault (childSection, jsonPath, "Blue");

            List<byte> foreground = new List<byte> () { red, green, blue };

            childSection = section.GetSection ("FontWeight");
            string fontWeight = GetSectionStrValueOrDefault (childSection, jsonPath, "FontWeight");

            childSection = section.GetSection ("IsSplitable");
            string shiftableString = GetterFromJson.GetSectionValue (childSection) ?? _defaultSplitability;
            bool isShiftable = false;

            try
            {
                int shiftableInt = Int32.Parse (shiftableString);
                isShiftable = Convert.ToBoolean (shiftableInt);
            }
            catch ( Exception ex ) { }


            TextualAtom atom = new TextualAtom ( atomName, width, height, topOffset, leftOffset , alignment, fontSize
                                          , fontFile, fontName, foreground, fontWeight, united, isShiftable, numberToLocate );
            return atom;
        }


        private InsideImage BuildInsideImage ( IConfigurationSection section )
        {
            //IConfigurationSection childSection = section.GetSection ("Name");
            //string imageName = GetterFromJson.GetSectionValue (childSection) ?? "";

            IConfigurationSection childSection = section.GetSection ( "Width" );
            double width = GetterFromJson.GetSectionValue ( childSection ).TranslateToDoubleOrZeroIfNot ( );
            
            childSection = section.GetSection ( "Height" );
            double height = GetterFromJson.GetSectionValue ( childSection ).TranslateToDoubleOrZeroIfNot ( );
            
            childSection = section.GetSection ( "TopOffset" );
            double topOffset = GetterFromJson.GetSectionValue ( childSection ).TranslateToDoubleOrZeroIfNot ( );

            childSection = section.GetSection ( "LeftOffset" );
            double leftOffset = GetterFromJson.GetSectionValue ( childSection ).TranslateToDoubleOrZeroIfNot ( );

            childSection = section.GetSection ( "ImagePath" );
            string path = GetterFromJson.GetSectionValue ( childSection) ?? "";

            childSection = section.GetSection ( "Color" );
            string color = GetterFromJson.GetSectionValue ( childSection) ?? "";
            
            childSection = section.GetSection ( "GeometricElementName" );
            string geometricElement = GetterFromJson.GetSectionValue ( childSection) ?? "";

            InsideImage image = 
                       new InsideImage ( path, width , height, color, geometricElement, topOffset, leftOffset );
            return image;
        }


        public Dictionary <BadgeLayout, KeyValuePair <string, List<string>>> GetBadgeLayouts ()
        {
            Dictionary <BadgeLayout, KeyValuePair<string, List<string>>> layouts = new ();

            foreach ( KeyValuePair<string, string> template   in   _nameAndJson )
            {
                string jsonPath = template.Value;

                try
                {
                    string json = File.ReadAllText (jsonPath);
                    var schemeTask = JsonSchema.FromFileAsync (_schemeFile.FullName);
                    schemeTask.Wait ();

                    var result = schemeTask.Result;

                    if ( result != null )
                    {
                        JsonSchemaValidator validator = new ();
                        JsonSchema schema = schemeTask.Result;
                        ICollection<ValidationError> errors = validator.Validate (json, schema);
                        bool templateIsCorrect = ( errors.Count == 0 );
                        List<string> message = new ();

                        if ( !templateIsCorrect )
                        {
                            List<ValidationError> children = new ();

                            foreach ( ValidationError err in errors )
                            {
                                ChildSchemaValidationError childErr = err as ChildSchemaValidationError;

                                if ( childErr != null )
                                {
                                    var childErrors = childErr.Errors;

                                    foreach ( var chErr in childErrors )
                                    {
                                        if ( chErr.Value.Count > 0 )
                                        {
                                            foreach ( ValidationError er in chErr.Value )
                                            {
                                                children.Add (er);
                                            }
                                        }
                                    }
                                }
                            }

                            foreach ( ValidationError err in children )
                            {
                                errors.Add (err);
                            }

                            foreach ( ValidationError err in errors )
                            {
                                string messageLine = string.Empty;

                                string errKind = TranslateErrorKindToRuss (err.Kind);
                                messageLine += errKind + " Ошибка в свойстве ";
                                string propertyPath = err.Path;

                                propertyPath = TrimWaste (propertyPath);

                                messageLine += propertyPath + " на строке номер ";
                                string lineNumber = err.LineNumber.ToString ();
                                messageLine += ( lineNumber );

                                message.Add (messageLine);
                            }

                            if ( !_jsonAndErrors.ContainsKey (jsonPath) )
                            {
                                _jsonAndErrors.Add (jsonPath, errors);
                            }
                        }

                        KeyValuePair<string, List<string>> jsonAndErrors = KeyValuePair.Create (jsonPath, message);
                        layouts.Add (GetBadgeLayout (template.Key), jsonAndErrors);
                    }
                }
                catch ( AggregateException ex )
                {
                    var mess = ex.Message;
                }
            }

            return layouts;
        }


        private string TrimWaste ( string propertyPath )
        {
            string result = propertyPath;

            if ( result.First () == '#' )
            {
                result = result.Substring (1);
            }

            if ( result.First () == '/' )
            {
                result = result.Substring (1);
            }

            return result;
        }


        private string TranslateErrorKindToRuss ( ValidationErrorKind kind )
        {
            string errKind = string.Empty;

            switch ( kind )
            {
                case ValidationErrorKind.IntegerExpected:
                    errKind = "Ожидалось целое число.";
                    break;
                case ValidationErrorKind.NumberExpected:
                    errKind = "Ожидалось целое число.";
                    break;
                case ValidationErrorKind.BooleanExpected:
                    errKind = "Ожидалось логическое значение.";
                    break;
                case ValidationErrorKind.StringExpected:
                    errKind = "Ожидалась строка.";
                    break;
                case ValidationErrorKind.ObjectExpected:
                    errKind = "Ожидался объект.";
                    break;
                case ValidationErrorKind.ArrayExpected:
                    errKind = "Ожидался массив.";
                    break;
                case ValidationErrorKind.NumberTooBig:
                    errKind = "Превышение максимума.";
                    break;
                case ValidationErrorKind.NumberTooSmall:
                    errKind = "Ниже минимума.";
                    break;
                case ValidationErrorKind.ArrayItemNotValid:
                    errKind = "Ошибка в элементе массива.";
                    break;
                case ValidationErrorKind.PropertyRequired:
                    errKind = "Не найдено обязательное поле.";
                    break;

                default:
                    errKind = kind.ToString () + ".";
                    break;
            }

            return errKind;
        }


        private string GetSectionStrValueOrDefault ( List<string> keyPathInJson, string jsonPath )
        {
            if ( _jsonAndErrors.ContainsKey (jsonPath) ) 
            {
                ICollection <ValidationError> errors = _jsonAndErrors [jsonPath];

                foreach ( ValidationError err   in   errors )
                {
                    string propertyPath = TrimWaste (err.Path);
                    string [] steps = propertyPath.Split ('.', 10);

                    if ( keyPathInJson.SequenceEqual<string> (steps) )
                    {
                        List<string> keyPathToDefault = BuildPathToDefaultValue (steps);
                        string strValue = GetSectionStrValue (keyPathToDefault, _schemeFile.FullName);

                        return strValue;
                    }
                }
            }

            string result = GetterFromJson.GetSectionStrValue (keyPathInJson, jsonPath);

            if ( result == null ) 
            {
                List<string> keyPathToDefault = GetKeyPathToDefault (keyPathInJson);
                result = GetterFromJson.GetSectionStrValue (keyPathToDefault, _schemeFile.FullName);
            }

            return result;
        }


        private int GetSectionIntValueOrDefault ( List<string> keyPathInJson, string jsonPath )
        {
            if ( _jsonAndErrors.ContainsKey(jsonPath) ) 
            {
                ICollection <ValidationError> errors = _jsonAndErrors [jsonPath];

                foreach ( ValidationError err   in   errors )
                {
                    string propertyPath = TrimWaste (err.Path);
                    string [] steps = propertyPath.Split ('.', 10);

                    if ( keyPathInJson.SequenceEqual<string> (steps) )
                    {
                        List<string> keyPathToDefault = BuildPathToDefaultValue (steps);
                        string strValue = GetSectionStrValue (keyPathToDefault, _schemeFile.FullName);

                        return Int32.Parse (strValue);
                    }
                }
            }

            int result = GetterFromJson.GetSectionIntValue (keyPathInJson, jsonPath);

            if ( result == -1 ) 
            {
                List<string> keyPathToDefault = GetKeyPathToDefault (keyPathInJson);
                result = GetterFromJson.GetSectionIntValue (keyPathToDefault, _schemeFile.FullName);
            }

            return result;
        }


        public List<string> GetKeyPathToDefault ( List<string> keyPathInJson )
        {
            string propertyPathRoot = keyPathInJson [0];
            List<string> keyPathToDefault = new () { "properties", propertyPathRoot, "default" };

            for ( int index = 1; index < keyPathInJson.Count; index++ )
            {
                keyPathToDefault.Add (keyPathInJson [index]);
            }

            return keyPathToDefault;
        }


        private bool GetSectionBoolValueOrDefault ( List<string> keyPathInJson, string jsonPath )
        {
            if ( _jsonAndErrors.ContainsKey (jsonPath) ) 
            {

                ICollection <ValidationError> errors = _jsonAndErrors [jsonPath];

                foreach ( ValidationError err   in   errors )
                {
                    string propertyPath = TrimWaste (err.Path);

                    string [] steps = propertyPath.Split ('.', 10);

                    if ( keyPathInJson.SequenceEqual<string> (steps) )
                    {
                        List<string> keyPathToDefault = BuildPathToDefaultValue (steps);

                        string strValue = GetSectionStrValue (keyPathToDefault, _schemeFile.FullName);

                        bool result = false;

                        if ( strValue == "True" )
                        {
                            result = true;
                        }

                        return result;
                    }
                }
            }

            return GetterFromJson.GetSectionBoolValue (keyPathInJson, jsonPath);
        }


        private string GetSectionStrValueOrDefault ( IConfigurationSection section, string jsonPath, string propertyName )
        {
            if ( _jsonAndErrors.ContainsKey (jsonPath) )
            {
                ICollection<ValidationError> errors = _jsonAndErrors [jsonPath];

                foreach ( ValidationError err in errors )
                {
                    string propertyPath = TrimWaste (err.Path);
                    string [] steps = propertyPath.Split ('.', 10);

                    if ( steps.Last () == propertyName )
                    {
                        List<string> keyPathToDefault = BuildPathToDefaultValue (steps);
                        string strValue = GetSectionStrValue (keyPathToDefault, _schemeFile.FullName);

                        return strValue;
                    }
                }
            }

            return GetterFromJson.GetSectionValue (section);
        }


        private int GetSectionIntValueOrDefault ( IConfigurationSection section, string jsonPath, string propertyName )
        {
            if ( _jsonAndErrors.ContainsKey (jsonPath) )
            {
                ICollection <ValidationError> errors = _jsonAndErrors [jsonPath];

                foreach ( ValidationError err   in   errors )
                {
                    string propertyPath = TrimWaste (err.Path);
                    string [] steps = propertyPath.Split ('.', 10);

                    if ( steps.Last () == propertyName )
                    {
                        List<string> keyPathToDefault = BuildPathToDefaultValue (steps);
                        string strValue = GetSectionStrValue (keyPathToDefault, _schemeFile.FullName);

                        return Int32.Parse (strValue);
                    }
                }
            }

            int result = 0;

            try
            {
                result = int.Parse (GetterFromJson.GetSectionValue (section));
            }
            catch ( Exception ex )
            {

            }

            return result;
        }


        private bool GetSectionBoolValueOrDefault ( IConfigurationSection section, string jsonPath, string propertyName )
        {
            bool result = false;

            if ( _jsonAndErrors.ContainsKey (jsonPath) )
            {

                ICollection <ValidationError> errors = _jsonAndErrors [jsonPath];

                foreach ( ValidationError err in errors )
                {
                    string propertyPath = TrimWaste (err.Path);
                    string [] steps = propertyPath.Split ('.', 10);

                    if ( steps.Last () == propertyName )
                    {
                        List<string> keyPathToDefault = BuildPathToDefaultValue (steps);
                        string strValue = GetSectionStrValue (keyPathToDefault, _schemeFile.FullName);

                        if ( strValue == "True" )
                        {
                            result = true;
                        }

                        return result;
                    }
                }
            }

            if ( ( GetterFromJson.GetSectionValue (section) == "true" ) ||
                 ( GetterFromJson.GetSectionValue (section) == "True" ) )
            {
                result = true;
            }

            return result;
        }


        private string GetSectionStrValue ( List<string> keyPathInJson, string jsonPath )
        {
            string result = GetterFromJson.GetSectionStrValue (keyPathInJson, jsonPath);

            return result;
        }


        private List<string> BuildPathToDefaultValue ( string[] keyPathInJson )
        {
            List<string> keyPathInJsonScheme = new ();

            foreach (string step   in   keyPathInJson) 
            {
                bool stepInArray = ( step.Last () == ']' );

                if ( stepInArray )
                {
                    int index = 0;

                    for ( ;  index < step.Length;   index++ )
                    {
                        if ( step [index] == '[' ) 
                        {
                            break;
                        }
                    }

                    string correctStep = step.Substring (0, index);

                    keyPathInJsonScheme.Add ("properties");
                    keyPathInJsonScheme.Add (correctStep);
                    keyPathInJsonScheme.Add ("items");
                }

                if ( ! stepInArray ) 
                {
                    keyPathInJsonScheme.Add ("properties");
                    keyPathInJsonScheme.Add (step);
                }
            }

            keyPathInJsonScheme.Add ("default");

            return keyPathInJsonScheme;
        }
    }
}

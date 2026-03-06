using Avalonia.Platform;
using Lister.Core.Entities.Badge;
using Lister.Desktop.Extentions;
using Microsoft.Extensions.Configuration;
using NJsonSchema;
using NJsonSchema.Validation;
using System.Drawing.Text;
using System.Text;

namespace Lister.Desktop.Services;

/// <summary>
/// Produces and keeps layout for each badge template got from json files.
/// </summary>
public sealed class BadgeLayoutProvider
{
    private static readonly string _defaultSplitability = "0";
    private static BadgeLayoutProvider? _instance;

    private string _schemeFile = string.Empty;
    private readonly Dictionary<string, string> _templateNameToDirectory = [];
    private readonly Dictionary<string, string> _templateNameToJson = [];
    private readonly Dictionary<string, string> _templateToIncorrectBackground = [];
    private readonly Dictionary<string, string> _templateToIncorrectBorderColor = [];
    private readonly Dictionary<string, string> _templateToCorrectBorderColor = [];
    private readonly Dictionary<string, List<byte>> _templateToIncorrectBorderThickness = [];
    private readonly Dictionary<string, List<byte>> _templateToCorrectBorderThickness = [];
    private readonly Dictionary<string, Layout> _badgeLayouts = [];
    private readonly Dictionary<string, ICollection<ValidationError>> _jsonAndErrors = [];
    private readonly Dictionary<string, string> _incorrectJsonAndError = [];
    private string _osName = string.Empty;

    private enum States
    {
        BeforeColon = 0,
        AfterColon = 1,
        BeforeName = 2,
        InName = 3
    }

    private BadgeLayoutProvider () 
    {
    
    }

    public static BadgeLayoutProvider GetInstance ()
    {
        _instance ??= new BadgeLayoutProvider ();

        return _instance;
    }

    public Task SetUp ( string resourceFolder, string jsonSchemeFolder, string osName )
    {
        Task task = new (
            () =>
            {
                SetLayouts ( resourceFolder, jsonSchemeFolder );
                Encoding.RegisterProvider ( CodePagesEncodingProvider.Instance );
                _osName = osName;
            }
        );

        task.Start ();

        return task;
    }

    public string GetBadgeImageUri ( string templateName )
    {
        string fileName = GetSectionStrValueOrDefault ( ["BackgroundImagePath"], _templateNameToJson [templateName] ) ?? "";
        string folderName = _templateNameToDirectory [templateName];
        string imagesFolder = folderName + "\\Images\\";
        string fileUri = imagesFolder + fileName;

        return fileUri;
    }

    public string GetIncorrectLineBackgroundHex ( string templateName )
    {
        return _templateToIncorrectBackground [templateName];
    }

    public string GetIncorrectMemberBorderColor ( string templateName )
    {
        return _templateToIncorrectBorderColor [templateName];
    }

    public string GetCorrectMemberBorderColor ( string templateName )
    {
        return _templateToCorrectBorderColor [templateName];
    }

    public List<byte> GetIncorrectMemberBorderThickness ( string templateName )
    {
        return _templateToIncorrectBorderThickness [templateName];
    }

    public List<byte> GetCorrectMemberBorderThickness ( string templateName )
    {
        return _templateToCorrectBorderThickness [templateName];
    }

    public Layout? GetBadgeLayout ( string templateName )
    {
        if ( _badgeLayouts.TryGetValue ( templateName, out Layout? value ) )
        {
            return value;
        }

        string jsonPath = _templateNameToJson [templateName];

        double badgeWidth = GetSectionIntValueOrDefault ( ["Width"], jsonPath );
        double badgeHeight = GetSectionIntValueOrDefault ( ["Height"], jsonPath );

        List<double> paddings = [];

        double leftSpan = GetSectionIntValueOrDefault ( ["Padding", "Left"], jsonPath );
        paddings.Add ( leftSpan );

        double topSpan = GetSectionIntValueOrDefault ( ["Padding", "Top"], jsonPath );
        paddings.Add ( topSpan );

        double rightSpan = GetSectionIntValueOrDefault ( ["Padding", "Right"], jsonPath );
        paddings.Add ( rightSpan );

        double bottomSpan = GetSectionIntValueOrDefault ( ["Padding", "Bottom"], jsonPath );
        paddings.Add ( bottomSpan );

        List<TextLine> lines = GetLines ( jsonPath );
        SetUnitingLines ( lines, jsonPath );

        List<ComponentImage> images = GetImages ( jsonPath, templateName );
        List<ComponentShape> shapes = GetShapes ( jsonPath );

        Layout? result = Layout.GetInstance ( badgeWidth, badgeHeight, templateName, paddings, lines, images, shapes );

        return result;
    }

    public Dictionary<Layout, KeyValuePair<string, List<string>>> GetBadgeLayouts ()
    {
        Dictionary<Layout, KeyValuePair<string, List<string>>> layouts = [];

        if ( string.IsNullOrWhiteSpace ( _schemeFile ) )
        {
            return layouts;
        }

        using Stream stream = AssetLoader.Open ( new Uri ( _schemeFile ) );
        Task<JsonSchema> schemeTask = JsonSchema.FromJsonAsync ( stream );
        schemeTask.Wait ();

        foreach ( KeyValuePair<string, string> template in _templateNameToJson )
        {
            string jsonPath = template.Value;

            try
            {
                string json = File.ReadAllText ( jsonPath );
                JsonSchema result = schemeTask.Result;

                if ( result != null )
                {
                    JsonSchemaValidator validator = new ();
                    List<string> message = [];


                    if ( _incorrectJsonAndError.ContainsKey ( jsonPath ) )
                    {
                        if ( !_jsonAndErrors.ContainsKey ( jsonPath ) )
                        {
                            message.Add ( _incorrectJsonAndError [jsonPath] );
                        }
                    }
                    else
                    {
                        ICollection<ValidationError> errors = validator.Validate ( json, result );
                        bool templateIsIncorrect = errors.Count != 0;

                        if ( templateIsIncorrect )
                        {
                            List<ValidationError> children = [];

                            foreach ( ValidationError err in errors )
                            {
                                if ( err is ChildSchemaValidationError childErr )
                                {
                                    var childErrors = childErr.Errors;

                                    foreach ( var chErr in childErrors )
                                    {
                                        if ( chErr.Value.Count > 0 )
                                        {
                                            foreach ( ValidationError er in chErr.Value )
                                            {
                                                children.Add ( er );
                                            }
                                        }
                                    }
                                }
                            }

                            foreach ( ValidationError err in children )
                            {
                                errors.Add ( err );
                            }

                            foreach ( ValidationError error in errors )
                            {
                                string messageLine = string.Empty;
                                string errKind = TranslateErrorKindToRuss ( error.Kind );
                                messageLine += errKind;

                                if ( error.Kind != ValidationErrorKind.PropertyRequired )
                                {
                                    messageLine += " Ошибка в свойстве ";
                                    string? propertyPath = TrimWaste ( error.Path );
                                    messageLine += propertyPath + " на строке номер ";
                                    string lineNumber = error.LineNumber.ToString ();
                                    messageLine += lineNumber;
                                }
                                else
                                {
                                    if (  error.Path != null  &&  error.Path.Length > 2  )
                                    {
                                        messageLine += error.Path.Substring ( 2, error.Path.Length - 2 );
                                    }
                                }

                                messageLine += ";";
                                message.Add ( messageLine );
                            }

                            _jsonAndErrors.TryAdd ( jsonPath, errors );
                        }
                    }

                    List<string> uninstalledFonts = GetUninstalledFontsFrom ( jsonPath );

                    foreach ( string uninstalledFont in uninstalledFonts )
                    {
                        string uninstalledFontMessage = "Не установлен шрифт с именем " + uninstalledFont;
                        message.Add ( uninstalledFontMessage );
                    }

                    KeyValuePair<string, List<string>> jsonAndErrors = KeyValuePair.Create ( jsonPath, message );

                    if ( template.Key != null )
                    {
                        Layout? layout = GetBadgeLayout ( template.Key );

                        if ( layout != null )
                        {
                            layouts.Add ( layout, jsonAndErrors );
                        }
                    }
                }
            }
            catch ( AggregateException )
            {

            }
        }

        return layouts;
    }

    private void SetLayouts ( string resourceFolder, string schemeFile )
    {
        _schemeFile = schemeFile;
        string templatesFolder = Path.Combine ( resourceFolder, "Templates" );
        DirectoryInfo containingDirectory = new ( templatesFolder );
        DirectoryInfo [] templateDirectories = containingDirectory.GetDirectories ();

        foreach ( DirectoryInfo templateDirectory in templateDirectories )
        {
            string directoryName = templateDirectory.FullName;
            FileInfo [] templateFiles = templateDirectory.GetFiles ( "*.json" );

            foreach ( FileInfo fileInfo in templateFiles )
            {
                string jsonPath = fileInfo.FullName;

                if ( JsonProcessor.TryValidJson ( jsonPath, out string validationMessage) )
                {
                    string templateName = JsonProcessor.GetSectionStrValue ( ["TemplateName"], jsonPath, false );
                    _templateNameToDirectory.Add ( templateName, directoryName );

                    bool nameShouldBeAdded = !string.IsNullOrEmpty ( templateName ) && !_templateNameToJson.ContainsKey ( templateName );

                    if ( nameShouldBeAdded )
                    {
                        _templateNameToJson.Add ( templateName, jsonPath );
                    }

                    bool colorShouldBeAdded = nameShouldBeAdded && !string.IsNullOrEmpty ( templateName );

                    if ( colorShouldBeAdded )
                    {
                        SetBadgeComponetMarkers ( jsonPath, templateName );
                    }
                }
                else
                {
                    if ( IsJsonTemplate ( jsonPath, out string templateName ) )
                    {
                        _templateNameToDirectory.Add ( templateName, directoryName );
                        _incorrectJsonAndError.Add ( jsonPath, TranslateIncorrectJsonMessage ( validationMessage ) );
                        _templateNameToJson.Add ( templateName, jsonPath );
                        SetBadgeComponetMarkers ( jsonPath, templateName );
                    }
                }
            }
        }
    }

    private void SetBadgeComponetMarkers ( string jsonPath, string templateName )
    {
        string background = GetIncorrectLineBackground ( jsonPath );
        _templateToIncorrectBackground?.Add ( templateName, background );

        string incorrectBorderColor = GetBorderColor ( jsonPath, "IncorrectMemberSettings" );
        _templateToIncorrectBorderColor?.Add ( templateName, incorrectBorderColor );

        string correctBorderColor = GetBorderColor ( jsonPath, "CorrectMemberSettings" );
        _templateToCorrectBorderColor?.Add ( templateName, correctBorderColor );

        List<byte> incorrectBorderThickness = GetThickness ( jsonPath, "IncorrectMemberSettings" );
        _templateToIncorrectBorderThickness?.Add ( templateName, incorrectBorderThickness );

        List<byte> correctBorderThickness = GetThickness ( jsonPath, "CorrectMemberSettings" );
        _templateToCorrectBorderThickness?.Add ( templateName, correctBorderThickness );
    }

    private string GetIncorrectLineBackground ( string jsonPath )
    {
        string background = GetSectionStrValueOrDefault ( ["IncorrectMemberSettings", "Background"], jsonPath );

        if ( string.IsNullOrWhiteSpace ( background ) )
        {
            background = "#c8c8c8";
        }

        return background;
    }

    private List<byte> GetThickness ( string jsonPath, string tag )
    {
        List<byte> thickness = [];

        byte left = ( byte ) GetSectionIntValueOrDefault ( [tag, "BorderThickness", "Left"], jsonPath );
        thickness.Add ( left );

        byte top = ( byte ) GetSectionIntValueOrDefault ( [tag, "BorderThickness", "Top"], jsonPath );
        thickness.Add ( top );

        byte right = ( byte ) GetSectionIntValueOrDefault ( [tag, "BorderThickness", "Right"], jsonPath );
        thickness.Add ( right );

        byte bottom = ( byte ) GetSectionIntValueOrDefault ( [tag, "BorderThickness", "Bottom"], jsonPath );
        thickness.Add ( bottom );

        return thickness;
    }

    private string GetBorderColor ( string jsonPath, string tag )
    {
        string borderColor = GetSectionStrValueOrDefault ( [tag, "BorderColor"], jsonPath );

        if ( string.IsNullOrWhiteSpace ( borderColor ) )
        {
            borderColor = "#d23650";
        }

        return borderColor;
    }

    private static string TranslateIncorrectJsonMessage ( string message )
    {
        string seekable = "LineNumber: ";
        int lenght = seekable.Length;
        int incomingIndex = message.IndexOf ( seekable );
        int endIndex = message.IndexOf ( '|' );
        string lineNumStr = message.Substring ( incomingIndex + lenght, endIndex - incomingIndex - lenght );
        string? result = "Ошибка на строке " + lineNumStr + " (" + message + ")";

        return result;
    }

    private static bool IsJsonTemplate ( string jsonPath, out string templateName )
    {
        List<char> tempNameChars = [];
        List<char> forbidenForName = ['\'', '(', ')', '{', '}', '[', ']'];
        string jsonText = File.ReadAllText ( jsonPath );
        int lenght = "\"TemplateName\"".Length;
        int incomingIndex = jsonText.IndexOf ( "\"TemplateName\"" );

        if ( incomingIndex > -1 )
        {
            States states = States.BeforeColon;
            int scratch = incomingIndex + lenght;

            for ( int index = scratch; index < jsonText.Length; index++ )
            {
                char current = jsonText [index];

                if ( states == States.BeforeColon )
                {
                    if ( current == ' ' )
                    {
                        continue;
                    }
                    else if ( current == ':' )
                    {
                        states = States.AfterColon;
                    }
                    else
                    {
                        templateName = string.Empty;
                        return false;
                    }
                }
                else if ( states == States.AfterColon )
                {
                    if ( current == ' ' )
                    {
                        continue;
                    }
                    else if ( current == '"' )
                    {
                        states = States.BeforeName;
                    }
                    else
                    {
                        templateName = string.Empty;

                        return false;
                    }
                }
                else if ( states == States.BeforeName )
                {
                    if ( current == ' ' || current == '"' || forbidenForName.Contains ( current ) )
                    {
                        templateName = string.Empty;

                        return false;
                    }
                    else
                    {
                        states = States.InName;
                        tempNameChars.Add ( current );
                    }
                }
                else if ( states == States.InName )
                {
                    if ( current == '"' )
                    {
                        templateName = new string ( [.. tempNameChars] );
                        return true;
                    }
                    else if ( forbidenForName.Contains ( current ) )
                    {
                        templateName = string.Empty;

                        return false;
                    }
                    else
                    {
                        tempNameChars.Add ( current );

                        continue;
                    }
                }
            }
        }

        templateName = string.Empty;

        return false;
    }

    private List<TextLine> GetLines ( string jsonPath )
    {
        List<TextLine> textLines =
        [
            BuildTextLine ( "FamilyName", jsonPath, 0 ),
            BuildTextLine ( "FirstName", jsonPath, 1 ),
            BuildTextLine ( "PatronymicName", jsonPath, 2 ),
            BuildTextLine ( "Post", jsonPath, 3 ),
            BuildTextLine ( "Department", jsonPath, 4 )
        ];

        return textLines;
    }

    private void SetUnitingLines ( List<TextLine> atoms, string jsonPath )
    {
        IEnumerable<IConfigurationSection> items = JsonProcessor.GetIncludedItemsOfSection ( ["UnitedTextBlocks"], jsonPath );

        foreach ( IConfigurationSection item in items )
        {
            int number = DigitalStringParser.ParseToInt ( item.GetSection ( "Number" )?.Value );

            if ( number == 0 )
            {
                number = 1;
            }

            IConfigurationSection unitedSection = item.GetSection ( "United" );
            IEnumerable<IConfigurationSection> unitedSections = unitedSection.GetChildren ();
            List<string> unitedTextLinesNames = [];

            foreach ( IConfigurationSection name in unitedSections )
            {
                if ( name.Value == null )
                {
                    continue;
                }

                unitedTextLinesNames.Add ( name.Value );
            }

            TextLine complexTextLine = BuildTextLine ( item, jsonPath, unitedTextLinesNames, number );
            atoms.Add ( complexTextLine );
        }
    }

    private List<ComponentImage> GetImages ( string jsonPath, string templateName )
    {
        List<ComponentImage> images = [];
        IEnumerable<IConfigurationSection> imageSections = JsonProcessor.GetIncludedItemsOfSection ( ["InsideImages"], jsonPath );

        foreach ( IConfigurationSection imageSection in imageSections )
        {
            ComponentImage image = BuildInsideImage ( imageSection, templateName );
            images.Add ( image );
        }

        return images;
    }

    private static List<ComponentShape> GetShapes ( string jsonPath )
    {
        List<ComponentShape> shapes = [];
        IEnumerable<IConfigurationSection> shapeSections = JsonProcessor.GetIncludedItemsOfSection ( ["InsideShapes"], jsonPath );

        foreach ( IConfigurationSection shapeSection in shapeSections )
        {
            ComponentShape shape = BuildInsideShape ( shapeSection );
            shapes.Add ( shape );
        }

        return shapes;
    }

    private TextLine BuildTextLine ( string lineName, string jsonPath, int numberToLocate )
    {
        TextLine line = new (
            lineName,
            GetSectionIntValueOrDefault ( [lineName, "Width"], jsonPath ),
            GetSectionIntValueOrDefault ( [lineName, "Height"], jsonPath ),
            GetSectionIntValueOrDefault ( [lineName, "TopOffset"], jsonPath ),
            GetSectionIntValueOrDefault ( [lineName, "LeftOffset"], jsonPath ),
            GetSectionStrValueOrDefault ( [lineName, "Alignment"], jsonPath ),
            GetSectionIntValueOrDefault ( [lineName, "FontSize"], jsonPath ),
            GetSectionStrValueOrDefault ( [lineName, "FontName"], jsonPath ),
            GetSectionStrValueOrDefault ( [lineName, "Foreground"], jsonPath ),
            GetSectionStrValueOrDefault ( [lineName, "FontWeight"], jsonPath ),
            null,
            GetSectionBoolValueOrDefault ( [lineName, "IsSplitable"], jsonPath ),
            numberToLocate
        );

        return line;
    }

    private TextLine BuildTextLine ( IConfigurationSection section, string jsonPath, List<string> united, int numberToLocate )
    {
        string lineName = section.GetSection ( "Name" )?.Value ?? "";
        double width = GetSectionIntValueOrDefault ( section.GetSection ( "Width" ), jsonPath );
        double height = GetSectionIntValueOrDefault ( section.GetSection ( "Height" ), jsonPath );
        double topOffset = GetSectionIntValueOrDefault ( section.GetSection ( "TopOffset" ), jsonPath );
        double leftOffset = GetSectionIntValueOrDefault ( section.GetSection ( "LeftOffset" ), jsonPath );
        double fontSize = GetSectionIntValueOrDefault ( section.GetSection ( "FontSize" ), jsonPath );

        string? alignment = GetSectionStrValueOrDefault ( section.GetSection ( "Alignment" ), jsonPath );
        string? fontName = GetSectionStrValueOrDefault ( section.GetSection ( "FontName" ), jsonPath );
        string? foreground = GetSectionStrValueOrDefault ( section.GetSection ( "Foreground" ), jsonPath );
        string? fontWeight = GetSectionStrValueOrDefault ( section.GetSection ( "FontWeight" ), jsonPath );

        string shiftableString = section.GetSection ( "IsSplitable" )?.Value ?? _defaultSplitability;
        int shiftableInt = DigitalStringParser.ParseToInt ( shiftableString );
        bool isShiftable = Convert.ToBoolean ( shiftableInt );

        TextLine textLine = new ( lineName, width, height, topOffset, leftOffset, alignment, fontSize, fontName, foreground,
            fontWeight, united, isShiftable, numberToLocate 
        );

        return textLine;
    }

    private ComponentImage BuildInsideImage ( IConfigurationSection section, string templateName )
    {
        double [] commonData = GetSharedData ( section );
        string fileName = section.GetSection ( "Path" )?.Value ?? "";
        string imagesFolder = _templateNameToDirectory [templateName] + "\\Images\\";
        string fileUri = imagesFolder + fileName;
        string bindingObjectName = section.GetSection ( "BindingObject" )?.Value ?? "";
        string isAbove = section.GetSection ( "IsAboveOfBinding" )?.Value ?? "";
        bool isAboveOfBinding = false;

        if ( isAbove == "yes" )
        {
            isAboveOfBinding = true;
        }

        ComponentImage image = new ( fileUri, commonData [0], commonData [1], commonData [2], commonData [3],
            bindingObjectName, isAboveOfBinding );

        return image;
    }

    private static ComponentShape BuildInsideShape ( IConfigurationSection section )
    {
        double [] commonData = GetSharedData ( section );
        string kind = section.GetSection ( "Type" )?.Value ?? "";
        string fillColor = section.GetSection ( "FillColor" )?.Value ?? "#000000";
        string bindingObjectName = section.GetSection ( "BindingObject" )?.Value ?? "";
        string isAbove = section.GetSection ( "IsAboveOfBinding" )?.Value ?? "";

        ComponentShape shape = new ( commonData [0], commonData [1], commonData [2], commonData [3], fillColor, kind,
            bindingObjectName, isAbove == "yes"
        );

        return shape;
    }

    private static double [] GetSharedData ( IConfigurationSection section )
    {
        double [] result = new double [4];
        _ = double.TryParse ( section.GetSection ( "Width" )?.Value, out result [0] );
        _ = double.TryParse ( section.GetSection ( "Height" )?.Value, out result [1] );
        _ = double.TryParse ( section.GetSection ( "TopOffset" )?.Value, out result [2] );
        _ = double.TryParse ( section.GetSection ( "LeftOffset" )?.Value, out result [3] );

        return result;
    }

    private List<string> GetUninstalledFontsFrom ( string jsonPath )
    {
        List<string> fontNames =
        [
            JsonProcessor.GetSectionStrValue ( ["CommonDefaultFontFamily"], jsonPath, false ),
            JsonProcessor.GetSectionStrValue ( ["FamilyName", "FontName"], jsonPath, false ),
            JsonProcessor.GetSectionStrValue ( ["FirstName", "FontName"], jsonPath, false ),
            JsonProcessor.GetSectionStrValue ( ["PatronymicName", "FontName"], jsonPath, false ),
            JsonProcessor.GetSectionStrValue ( ["Post", "FontName"], jsonPath, false ),
            JsonProcessor.GetSectionStrValue ( ["Department", "FontName"], jsonPath, false ),
        ];

        IEnumerable<IConfigurationSection> unitings = JsonProcessor.GetIncludedItemsOfSection ( ["UnitedTextBlocks"], jsonPath );

        foreach ( IConfigurationSection unit in unitings )
        {
            IConfigurationSection fontNameSection = unit.GetSection ( "FontName" );
            string? fontName = fontNameSection.Value;

            if ( fontName != null )
            {
                fontNames.Add ( fontName );
            }
        }

        return GetUninstalledFontsAmong ( fontNames );
    }

    private List<string> GetUninstalledFontsAmong ( List<string> fontNames )
    {
        if ( _osName == "Windows" && OperatingSystem.IsWindowsVersionAtLeast ( 6, 1 ) )
        {
            InstalledFontCollection ifc = new ();
            System.Drawing.FontFamily [] families = ifc.Families;
            List<string> installed = [];

            foreach ( System.Drawing.FontFamily family in families )
            {
                string? familyName = family.Name;

                if ( familyName != null )
                {
                    installed.Add ( familyName );
                }
            }

            return GetUninstalled ( installed, fontNames );
        }
        else if ( _osName == "Linux" )
        {
            string fontInstallingCommand = "fc-list : family | sort | uniq";
            string result = TerminalCommandExecuter.ExecuteCommand ( fontInstallingCommand );
            List<string> installed = [.. result.Split ( '\n' )];

            return GetUninstalled ( installed, fontNames );
        }

        return [];
    }

    private static List<string> GetUninstalled ( List<string> installed, List<string> checkebles )
    {
        List<string> uninstalled = [];

        foreach ( var name in checkebles )
        {
            bool isExisting = !string.IsNullOrWhiteSpace ( name );

            if ( isExisting && !installed.Contains ( name ) )
            {
                uninstalled.Add ( name );
            }
        }

        uninstalled = [.. uninstalled.Distinct ( StringComparer.OrdinalIgnoreCase )];

        return uninstalled;
    }

    private static string TrimWaste ( string? propertyPath )
    {
        if ( string.IsNullOrWhiteSpace ( propertyPath ) )
        {
            return string.Empty;
        }

        string? result = propertyPath;

        if ( result.First () == '#' )
        {
            result = result [1..];
        }

        if ( result.First () == '/' )
        {
            result = result [1..];
        }

        return result;
    }

    private static string TranslateErrorKindToRuss ( ValidationErrorKind kind )
    {
        string errKind;
#pragma warning disable IDE0066 // Преобразовать оператор switch в выражение
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
                errKind = "Не найдено обязательное поле ";
                break;
            case ValidationErrorKind.PatternMismatch:
                errKind = "Некорректное значение цвета. Присутствуют недопустимые символы.";
                break;
            case ValidationErrorKind.StringTooLong:
                errKind = "Некорректное значение цвета. Слишком длинное.";
                break;
            case ValidationErrorKind.StringTooShort:
                errKind = "Некорректное значение цвета. Слишком короткое.";
                break;

            default:
                errKind = kind.ToString () + ".";
                break;
        }
#pragma warning restore IDE0066 // Преобразовать оператор switch в выражение

        return errKind;
    }

    private string GetSectionStrValueOrDefault ( List<string> keyPathInJson, string jsonPath )
    {
        if ( _jsonAndErrors.TryGetValue ( jsonPath, out ICollection<ValidationError>? errors ) )
        {
            foreach ( ValidationError err in errors )
            {
                string propertyPath = TrimWaste ( err.Path );
                string [] steps = propertyPath.Split ( '.' );

                if ( keyPathInJson.SequenceEqual ( steps ) )
                {
                    return GetDefaultStrValue ( steps );
                }
            }
        }
        else if ( _incorrectJsonAndError.ContainsKey ( jsonPath ) )
        {
            return GetDefaultStrValue ( keyPathInJson );
        }

        string result = JsonProcessor.GetSectionStrValue ( keyPathInJson, jsonPath, false ) ?? string.Empty;

        if ( string.IsNullOrWhiteSpace ( result ) )
        {
            result = GetDefaultStrValue ( keyPathInJson );
        }

        if ( result == null ) 
        {
            throw new Exception ( "Default value is absent in schema file" );
        }

        return result;
    }

    private int GetSectionIntValueOrDefault ( List<string> keyPathInJson, string jsonPath )
    {
        if ( _jsonAndErrors.TryGetValue ( jsonPath, out ICollection<ValidationError>? errors ) )
        {
            foreach ( ValidationError err in errors )
            {
                string propertyPath = TrimWaste ( err.Path );
                string [] steps = propertyPath.Split ( '.' );

                if ( keyPathInJson.SequenceEqual ( steps ) )
                {
                    return DigitalStringParser.ParseToInt ( GetDefaultStrValue ( steps ) );
                }
            }
        }
        else if ( _incorrectJsonAndError.ContainsKey ( jsonPath ) )
        {
            return DigitalStringParser.ParseToInt ( GetDefaultStrValue ( keyPathInJson ) );
        }

        int result = JsonProcessor.GetSectionIntValue ( keyPathInJson, jsonPath, false );
        bool errorsAbsentButSectionNotFound = result == -1;

        if ( errorsAbsentButSectionNotFound )
        {
            List<string> keyPathToDefault = BuildPathToDefaultValue ( keyPathInJson );
            result = JsonProcessor.GetSectionIntValue ( keyPathToDefault, _schemeFile, true );
        }

        return result;
    }

    private bool GetSectionBoolValueOrDefault ( List<string> keyPathInJson, string jsonPath )
    {
        if ( _jsonAndErrors.TryGetValue ( jsonPath, out ICollection<ValidationError>? errors ) )
        {
            foreach ( ValidationError err in errors )
            {
                string propertyPath = TrimWaste ( err.Path );

                string [] steps = propertyPath.Split ( '.' );

                if ( keyPathInJson.SequenceEqual ( steps ) )
                {
                    return GetSectionDefaultBoolValue ( steps );
                }
            }
        }
        else if ( _incorrectJsonAndError.ContainsKey ( jsonPath ) )
        {
            return GetSectionDefaultBoolValue ( keyPathInJson );
        }

        return JsonProcessor.GetSectionBoolValue ( keyPathInJson, jsonPath );
    }

    private bool GetSectionDefaultBoolValue ( ICollection<string> keyPathInJson )
    {
        List<string> keyPathToDefault = BuildPathToDefaultValue ( keyPathInJson );
        string strValue = JsonProcessor.GetSectionStrValue ( keyPathToDefault, _schemeFile, true );

        if ( strValue == "True" )
        {
            return true;
        }

        return false;
    }

    private string? GetSectionStrValueOrDefault ( IConfigurationSection section, string jsonPath )
    {
        if ( _jsonAndErrors.TryGetValue ( jsonPath, out ICollection<ValidationError>? errors ) )
        {
            foreach ( ValidationError err in errors )
            {
                if ( PathesEqual ( section, err ) )
                {
                    return GetDefaultStrValue ( section );
                }
            }
        }
        else if ( _incorrectJsonAndError.ContainsKey ( jsonPath ) )
        {
            return GetDefaultStrValue ( section );
        }

        return section?.Value;
    }

    private string GetDefaultStrValue ( IConfigurationSection section )
    {
        List<string> keyPathToDefault = BuildPathToDefaultValue ( section );
        string defaultStr = JsonProcessor.GetSectionStrValue ( keyPathToDefault, _schemeFile, true );

        return defaultStr;
    }

    private string GetDefaultStrValue ( ICollection<string> steps )
    {
        List<string> keyPathToDefault = BuildPathToDefaultValue ( steps );

        return JsonProcessor.GetSectionStrValue ( keyPathToDefault, _schemeFile, true );
    }

    private int GetSectionIntValueOrDefault ( IConfigurationSection section, string jsonPath )
    {
        if ( _jsonAndErrors.TryGetValue ( jsonPath, out ICollection<ValidationError>? errors ) )
        {
            foreach ( ValidationError err in errors )
            {
                if ( PathesEqual ( section, err ) )
                {
                    List<string> keyPathToDefault = BuildPathToDefaultValue ( section );
                    string strValue = JsonProcessor.GetSectionStrValue ( keyPathToDefault, _schemeFile, true );

                    return DigitalStringParser.ParseToInt ( strValue );
                }
            }
        }
        else if ( _incorrectJsonAndError.ContainsKey ( jsonPath ) )
        {
            List<string> keyPathToDefault = BuildPathToDefaultValue ( section );
            string strValue = JsonProcessor.GetSectionStrValue ( keyPathToDefault, _schemeFile, true );

            return DigitalStringParser.ParseToInt ( strValue );
        }

        return DigitalStringParser.ParseToInt ( section?.Value );
    }

    private static bool PathesEqual ( IConfigurationSection section, ValidationError err )
    {
        string [] sectionSteps = section.Path.Split ( ':' );

        string propertyPath = TrimWaste ( err.Path );
        string [] impureSteps = propertyPath.Split ( '.', 10 );

        bool metArrayIndexer = false;
        string pureStep = string.Empty;
        string indexator = string.Empty;

        int indexatorNum = 0;
        int counter = 0;

        foreach ( string step in impureSteps )
        {
            bool stepInArray = step.Last () == ']';

            if ( stepInArray )
            {
                int index = 0;

                for ( ; index < step.Length; index++ )
                {
                    if ( step [index] == '[' )
                    {
                        break;
                    }
                }

                pureStep = step [..index];
                indexator = step.Substring ( index + 1, step.Length - index - 2 );
                metArrayIndexer = true;
                indexatorNum = counter;
            }

            counter++;
        }

        string [] steps = impureSteps;

        if ( metArrayIndexer )
        {
            steps = new string [impureSteps.Length + 1];

            for ( int index = 0; index < steps.Length; index++ )
            {
                if ( index < indexatorNum )
                {
                    steps [index] = impureSteps [index];
                }
                else if ( index == indexatorNum )
                {
                    steps [index] = pureStep;
                }
                else if ( index == indexatorNum + 1 )
                {
                    steps [index] = indexator.ToString ();
                }
                else
                {
                    steps [index] = impureSteps [index - 1];
                }
            }
        }

        bool equals = true;

        if ( sectionSteps.Length != steps.Length )
        {
            return false;
        }

        for ( int index = 0; index < sectionSteps.Length; index++ )
        {
            if ( steps [index] != sectionSteps [index] )
            {
                equals = false;
                break;
            }
        }

        return equals;
    }

    private static List<string> BuildPathToDefaultValue ( IConfigurationSection section )
    {
        List<string> keyPathInJsonScheme = ["default"];

        string [] keyPathInJson = section.Path.Split ( ':' );

        foreach ( string step in keyPathInJson )
        {
            keyPathInJsonScheme.Add ( step );
        }

        return keyPathInJsonScheme;
    }

    private static List<string> BuildPathToDefaultValue ( ICollection<string> keyPathInJson )
    {
        List<string> keyPathInJsonScheme = ["default", .. keyPathInJson];

        return keyPathInJsonScheme;
    }
}

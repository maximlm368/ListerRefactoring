using Avalonia.Platform;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace Lister.Desktop.CoreAbstractionsImplimentations.BadgeCreator;

public static class GetterFromJson
{
    private static string _configFilePath;
    private static string _attributeSection;
    private static List<string> _incorrectJsons = [];


    public static IConfigurationSection? GetSection(string jsonPath, List<string> keyPathOfSection)
    {
        IConfigurationSection section = null;

        if (keyPathOfSection.Count > 1)
        {
            section = GetConfigRoot( jsonPath, false ).GetSection( keyPathOfSection[0] );

            for (int step = 1; step < keyPathOfSection.Count; step++)
            {
                string sectionName = keyPathOfSection[step];
                section = section.GetSection( sectionName );
            }
        }

        return section;
    }


    public static bool CheckJsonCorrectness(string jsonPath, out string error)
    {
        if (_incorrectJsons.Contains( jsonPath ))
        {
            error = string.Empty;
        }

        bool sectionIsNotAvailable = jsonPath == null
                                     ||
                                     jsonPath == string.Empty;

        if (sectionIsNotAvailable)
        {
            error = string.Empty;
            return false;
        }

        try
        {
            string[] lines = File.ReadAllLines( jsonPath );

            JsonDocument doc = JsonDocument.Parse( File.ReadAllText( jsonPath ) );
            error = string.Empty;

            return true;
        }
        catch (JsonException ex)
        {
            error = ex.Message;
            _incorrectJsons.Add( jsonPath );

            return false;
        }
    }


    public static string GetSectionStrValue (List<string> keyPathOfSection, string jsonPath, bool isJsonFromDll )
    {
        if (_incorrectJsons.Contains( jsonPath ))
        {
            return string.Empty;
        }

        bool isSectionNotAvailable = keyPathOfSection == null
                                     || 
                                     keyPathOfSection.Count < 1
                                     || 
                                     jsonPath == null
                                     || 
                                     jsonPath == string.Empty;

        if (isSectionNotAvailable)
        {
            return string.Empty;
        }

        try
        {
            JsonDocument doc;

            if ( isJsonFromDll )
            {
                doc = JsonDocument.Parse ( AssetLoader.Open ( new Uri ( jsonPath ) ) );
            }
            else 
            {
                doc = JsonDocument.Parse ( File.ReadAllText ( jsonPath ) );
            }
        }
        catch (JsonException ex)
        {
            _incorrectJsons.Add( jsonPath );
        }

        try
        {
            IConfigurationRoot configRoot = GetConfigRoot( jsonPath, isJsonFromDll );

            string sectionName = keyPathOfSection[0];
            IConfigurationSection section = configRoot.GetSection( sectionName );

            if (keyPathOfSection.Count > 1)
            {
                for (int step = 1; step < keyPathOfSection.Count; step++)
                {
                    sectionName = keyPathOfSection[step];
                    section = section.GetSection( sectionName );
                }
            }

            string result = section.Value;

            return result;
        }
        catch (Exception ex)
        {
            return string.Empty;
        }
    }


    public static int GetSectionIntValue(List<string> keyPathOfSection, string jsonPath, bool isJsonFromDll)
    {
        string strResult = GetSectionStrValue ( keyPathOfSection, jsonPath, isJsonFromDll );

        bool sectionIsNotAvailable = string.IsNullOrWhiteSpace ( strResult );

        if (sectionIsNotAvailable)
        {
            return -1;
        }

        int result = Parse ( strResult );

        return result;
    }


    public static bool GetSectionBoolValue ( List<string> keyPathOfSection, string jsonPath )
    {
        string strResult = GetSectionStrValue ( keyPathOfSection, jsonPath, false );

        bool sectionIsNotAvailable = string.IsNullOrWhiteSpace ( strResult );

        bool result = false;

        if ( sectionIsNotAvailable )
        {
            return result;
        }

        bool resultIsTrue = strResult == "true" || strResult == "True";

        if ( resultIsTrue )
        {
            result = true;
        }

        return result;
    }


    public static IEnumerable<IConfigurationSection> GetIncludedItemsOfSection
                                                     (List<string> keyPathOfSection, string jsonPath)
    {
        if (keyPathOfSection == null || keyPathOfSection.Count < 1 || _incorrectJsons.Contains( jsonPath ))
        {
            return Enumerable.Empty<IConfigurationSection>();
        }

        try
        {
            IConfigurationRoot configRoot = GetConfigRoot( jsonPath, false );
            string sectionName = keyPathOfSection[0];
            IConfigurationSection section = configRoot.GetSection( sectionName );

            if (keyPathOfSection.Count > 1)
            {
                for (int step = 1; step < keyPathOfSection.Count; step++)
                {
                    sectionName = keyPathOfSection[step];
                    section = section.GetSection( sectionName );
                }
            }

            IConfigurationSection items = section.GetSection( "Items" );
            IEnumerable<IConfigurationSection> targetChildren = items.GetChildren();

            return targetChildren;
        }
        catch (Exception ex)
        {
            string str = ex.ToString();
            string name = ex.GetType().Name;

            return Enumerable.Empty<IConfigurationSection>();
        }
    }

    private static int Parse(string parsable)
    {
        int result = -1;
        bool isInt = int.TryParse( parsable, out result );

        if (!isInt)
        {
            return -1;
        }

        return result;
    }


    private static IConfigurationRoot GetConfigRoot(string jsonPath, bool isJsonFromDll )
    {
        var builder = new ConfigurationBuilder();

        if ( isJsonFromDll )
        {
            builder.AddJsonStream ( AssetLoader.Open ( new Uri ( jsonPath ) ) );
        }
        else 
        {
            builder.AddJsonFile ( jsonPath );
        }
        
        return builder.Build();
    }
}

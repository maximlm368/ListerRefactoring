using Avalonia.Platform;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Lister.Desktop.ExecutersForCoreAbstractions.BadgeCreator;

/// <summary>
/// Gets values of sections of json files. Valids json file.
/// </summary>
public static class JsonProcessor
{
    private static readonly List<string> _incorrectJsons = [];

    public static IConfigurationSection? GetSection ( string jsonPath, List<string> keyPathOfSection )
    {
        IConfigurationSection? section = null;

        if ( keyPathOfSection.Count > 1 )
        {
            section = GetConfigRoot ( jsonPath, false ).GetSection ( keyPathOfSection [0] );

            for ( int index = 1; index < keyPathOfSection.Count; index++ )
            {
                string sectionName = keyPathOfSection [index];
                section = section.GetSection ( sectionName );
            }
        }

        return section;
    }

    public static bool TryValidJson ( string jsonPath, out string error )
    {
        if ( _incorrectJsons.Contains ( jsonPath ) || string.IsNullOrWhiteSpace ( jsonPath ) )
        {
            error = string.Empty;

            return false;
        }

        try
        {
            JsonDocument doc = JsonDocument.Parse ( File.ReadAllText ( jsonPath ) );
            error = string.Empty;

            return true;
        }
        catch ( JsonException ex )
        {
            error = ex.Message;
            _incorrectJsons.Add ( jsonPath );

            return false;
        }
    }

    public static string GetSectionStrValue ( List<string> SectionPath, string jsonPath, bool isJsonFromDll )
    {
        if ( _incorrectJsons.Contains ( jsonPath ) )
        {
            return string.Empty;
        }

        bool isSectionNotAvailable = SectionPath == null
                                     || SectionPath.Count < 1
                                     || string.IsNullOrWhiteSpace ( jsonPath );

        if ( isSectionNotAvailable )
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
        catch ( JsonException )
        {
            _incorrectJsons.Add ( jsonPath );
        }

        try
        {
            IConfigurationRoot configRoot = GetConfigRoot ( jsonPath, isJsonFromDll );
#pragma warning disable CS8602 //Dereference of a possibly null reference.
            IConfigurationSection section = configRoot.GetSection ( SectionPath [0] );

            if ( SectionPath.Count > 1 )
            {
                for ( int index = 1; index < SectionPath.Count; index++ )
                {
                    SectionPath [0] = SectionPath [index];
                    section = section.GetSection ( SectionPath [0] );
                }
            }
#pragma warning disable CS8602 //Dereference of a possibly null reference.

            return section.Value ?? string.Empty;
        }
        catch ( Exception )
        {
            return string.Empty;
        }
    }

    public static int GetSectionIntValue ( List<string> keyPathOfSection, string jsonPath, bool isJsonFromDll )
    {
        string strResult = GetSectionStrValue ( keyPathOfSection, jsonPath, isJsonFromDll );
        bool sectionIsNotAvailable = string.IsNullOrWhiteSpace ( strResult );

        if ( sectionIsNotAvailable )
        {
            return -1;
        }

        return int.TryParse ( strResult, out int result ) ? result : -1;
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

    public static IEnumerable<IConfigurationSection> GetIncludedItemsOfSection ( List<string> keyPathOfSection, string jsonPath )
    {
        if ( keyPathOfSection == null || keyPathOfSection.Count < 1 || _incorrectJsons.Contains ( jsonPath ) )
        {
            return [];
        }

        try
        {
            IConfigurationRoot configRoot = GetConfigRoot ( jsonPath, false );
            string sectionName = keyPathOfSection [0];
            IConfigurationSection section = configRoot.GetSection ( sectionName );

            if ( keyPathOfSection.Count > 1 )
            {
                for ( int step = 1; step < keyPathOfSection.Count; step++ )
                {
                    sectionName = keyPathOfSection [step];
                    section = section.GetSection ( sectionName );
                }
            }

            IConfigurationSection items = section.GetSection ( "Items" );
            IEnumerable<IConfigurationSection> targetChildren = items.GetChildren ();

            return targetChildren;
        }
        catch ( Exception )
        {
            return [];
        }
    }

    private static IConfigurationRoot GetConfigRoot ( string jsonPath, bool isJsonFromDll )
    {
        var builder = new ConfigurationBuilder ();

        if ( isJsonFromDll )
        {
            builder.AddJsonStream ( AssetLoader.Open ( new Uri ( jsonPath ) ) );
        }
        else
        {
            builder.AddJsonFile ( jsonPath );
        }

        return builder.Build ();
    }

    public static void WritePersonSource ( string fileName, string keepablePath )
    {
        try
        {
            int limit = GetSectionIntValue ( ["maketLimit"], fileName, false );
            Config config = new () { maketLimit = limit, personSource = keepablePath ?? string.Empty };
            string jsonStr = JsonSerializer.Serialize ( config );
            File.WriteAllText ( fileName, jsonStr );
        }
        catch ( Exception ) 
        {
        
        }
    }

    private class Config
    {
        [JsonInclude]
        public int maketLimit;

        [JsonInclude]
        public string? personSource;
    }
}

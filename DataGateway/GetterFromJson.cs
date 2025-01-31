using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using static System.Collections.Specialized.BitVector32;
using ContentAssembler;
//using DocumentFormat.OpenXml.Office2010.PowerPoint;

namespace DataGateway
{
    public static class GetterFromJson
    {
        private static string configFilePath;
        private static string attributeSection;


        public static IConfigurationSection ? GetSection ( string jsonPath, List<string> keyPathOfSection )
        {
            IConfigurationSection section = null;

            if ( keyPathOfSection.Count > 1 )
            {
                section = GetConfigRoot (jsonPath).GetSection (keyPathOfSection [0]);

                for ( int step = 1; step < keyPathOfSection.Count; step++ )
                {
                    string sectionName = keyPathOfSection [step];
                    section = section.GetSection (sectionName);
                }
            }

            return section;
        }


        public static bool CheckJsonCorrectness ( string jsonPath, out string error )
        {
            bool sectionIsNotAvailable = (( jsonPath == null )   ||   ( jsonPath == string.Empty ));

            if ( sectionIsNotAvailable )
            {
                error = string.Empty;

                return false;
            }

            try
            {
                JsonDocument doc = System.Text.Json.JsonDocument.Parse (File.ReadAllText (jsonPath));
                error = string.Empty;

                return true;
            }
            catch ( JsonException ex )
            {
                error = ex.Message;
                
                return false;
            }
        }


        public static string GetSectionStrValue ( List<string> keyPathOfSection, string jsonPath )
        {
            bool sectionIsNotAvailable = ( keyPathOfSection == null )   ||   ( keyPathOfSection.Count < 1 ) 
                                        ||   ( jsonPath == null )   ||   ( jsonPath == string.Empty );

            if (sectionIsNotAvailable)
            {
                return string.Empty;
            }

            try
            {
                JsonDocument doc = System.Text.Json.JsonDocument.Parse (File.ReadAllText (jsonPath));
            }
            catch ( JsonException ex )
            {
            }

            try 
            {
                IConfigurationRoot configRoot = GetConfigRoot (jsonPath);

                string sectionName = keyPathOfSection [0];
                IConfigurationSection section = configRoot.GetSection (sectionName);

                if ( keyPathOfSection.Count > 1 )
                {
                    for ( int step = 1;   step < keyPathOfSection.Count;   step++ )
                    {
                        sectionName = keyPathOfSection [step];
                        section = section.GetSection (sectionName);
                    }
                }

                string result = section.Value;

                return result;
            } 
            catch ( Exception ex ) 
            {
                return string.Empty;
            }
        }


        public static int GetSectionIntValue ( List<string> keyPathOfSection, string jsonPath )
        {
            string strResult = GetSectionStrValue (keyPathOfSection, jsonPath);

            bool sectionIsNotAvailable = ( string.IsNullOrWhiteSpace(strResult) );

            if ( sectionIsNotAvailable )
            {
                return -1;
            }

            try
            {
                int result = Int32.Parse(strResult);

                return result;
            }
            catch ( System.IO.InvalidDataException ex )
            {
                return -1;
            }
        }


        public static bool GetSectionBoolValue ( List<string> keyPathOfSection, string jsonPath )
        {
            string strResult = GetSectionStrValue (keyPathOfSection, jsonPath);

            bool sectionIsNotAvailable = ( string.IsNullOrWhiteSpace (strResult) );

            bool result = false;

            if ( sectionIsNotAvailable )
            {
                return result;
            }

            bool resultIsTrue = (( strResult == "true" )   ||   ( strResult == "True" ));

            if ( resultIsTrue )
            {
                result = true;
            }

            return result;
        }


        public static string ? GetSectionValue ( IConfigurationSection section )
        {
            if( section == null ) 
            {
                return null; 
            }

            string result = section.Value;
            return result;
        }


        public static IEnumerable <IConfigurationSection> GetIncludedItemsOfSection
                                                                          ( List<string> keyPathOfSection, string jsonPath )
        {
            if ( (keyPathOfSection == null)   ||   (keyPathOfSection.Count < 1) ) 
            {
                return Enumerable.Empty<IConfigurationSection> ();
            }

            try 
            {
                IConfigurationRoot configRoot = GetConfigRoot (jsonPath);
                string sectionName = keyPathOfSection [0];
                IConfigurationSection section = configRoot.GetSection (sectionName);

                if ( keyPathOfSection.Count > 1 )
                {
                    for ( int step = 1;   step < keyPathOfSection.Count;   step++ )
                    {
                        sectionName = keyPathOfSection [step];
                        section = section.GetSection (sectionName);
                    }
                }

                IConfigurationSection items = section.GetSection ("Items");
                IEnumerable <IConfigurationSection> targetChildren = items.GetChildren ();

                return targetChildren;
            }
            catch ( Exception ex ) 
            {
                return Enumerable.Empty<IConfigurationSection> ();
            }
        }


        public static IEnumerable <IConfigurationSection> GetChildren ( List<string> keyPathOfSection, string jsonPath )
        {
            if ( ( keyPathOfSection == null )   ||   ( keyPathOfSection.Count < 1 ) )
            {
                return Enumerable.Empty<IConfigurationSection> ();
            }

            try
            {
                IConfigurationRoot configRoot = GetConfigRoot (jsonPath);
                string sectionName = keyPathOfSection [0];
                IConfigurationSection section = configRoot.GetSection (sectionName);

                if ( keyPathOfSection.Count > 1 )
                {
                    for ( int step = 1; step < keyPathOfSection.Count; step++ )
                    {
                        sectionName = keyPathOfSection [step];
                        section = section.GetSection (sectionName);
                    }
                }

                IEnumerable <IConfigurationSection> targetChildren = section.GetChildren ();

                return targetChildren;
            }
            catch ( Exception ex )
            {
                return Enumerable.Empty<IConfigurationSection> ();
            }
        }


        private static IConfigurationRoot GetConfigRoot ( string jsonPath )
        {
            var builder = new ConfigurationBuilder ();
            builder.AddJsonFile (jsonPath);

            return builder.Build ();
        }
    }
}

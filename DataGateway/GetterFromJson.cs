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

namespace DataGateway
{
    public static class GetterFromJson
    {
        private static string configFilePath;
        private static string attributeSection;


        public static string GetSectionStrValue ( List<string> keyPathInJson, string jsonPath )
        {
            bool sectionIsNotAvailable = ( keyPathInJson == null )   ||   ( keyPathInJson.Count < 1 ) 
                                        ||   ( jsonPath == null )   ||   ( jsonPath == string.Empty );

            if (sectionIsNotAvailable)
            {
                return string.Empty;
            }

            try 
            {
                IConfigurationRoot configRoot = GetConfigRoot (jsonPath);

                string sectionName = keyPathInJson [0];
                IConfigurationSection section = configRoot.GetSection (sectionName);

                if ( keyPathInJson.Count > 1 )
                {
                    for ( int step = 1;   step < keyPathInJson.Count;   step++ )
                    {
                        sectionName = keyPathInJson [step];
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


        public static int GetSectionIntValue ( List<string> keyPathInJson, string jsonPath )
        {
            string strResult = GetSectionStrValue (keyPathInJson, jsonPath);

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


        public static bool GetSectionBoolValue ( List<string> keyPathInJson, string jsonPath )
        {
            string strResult = GetSectionStrValue (keyPathInJson, jsonPath);

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


        public static string GetSectionValue ( IConfigurationSection section )
        {
            string templateName = section.Value;
            return templateName;
        }


        public static IEnumerable <IConfigurationSection> GetIncludedItemsOfSection
                                                                          ( List<string> keyPathInJson, string jsonPath )
        {
            if ( (keyPathInJson == null)   ||   (keyPathInJson.Count < 1) ) 
            {
                return Enumerable.Empty<IConfigurationSection> ();
            }

            try 
            {
                IConfigurationRoot configRoot = GetConfigRoot (jsonPath);
                string sectionName = keyPathInJson [0];
                IConfigurationSection section = configRoot.GetSection (sectionName);

                if ( keyPathInJson.Count > 1 )
                {
                    for ( int step = 1;   step < keyPathInJson.Count;   step++ )
                    {
                        sectionName = keyPathInJson [step];
                        section = section.GetSection (sectionName);
                    }
                }

                IConfigurationSection items = section.GetSection ("Items");
                IEnumerable<IConfigurationSection> targetChildren = items.GetChildren ();

                //foreach ( IConfigurationSection unit   in   targetChildren )
                //{
                //    IConfigurationSection unitedSection = unit.GetSection ("United");
                //    IEnumerable <IConfigurationSection> unitedSections = unitedSection.GetChildren ();
                //}

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

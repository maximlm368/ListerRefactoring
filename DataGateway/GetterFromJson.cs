using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.Configuration;
using static System.Collections.Specialized.BitVector32;
using ContentAssembler;

namespace DataGateway
{
    public static class GetterFromJson
    {
        private static string configFilePath;
        private static string attributeSection;


        public static string GetSectionValue ( List<string> keyPathInJson, string jsonPath )
        {
            if ( ( keyPathInJson == null )   ||   ( keyPathInJson.Count < 1 ) )
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
                    for ( int step = 1; step < keyPathInJson.Count; step++ )
                    {
                        sectionName = keyPathInJson [step];
                        section = section.GetSection (sectionName);
                    }
                }

                string templateName = section.Value;
                return templateName;
            }
            catch ( System.IO.InvalidDataException ex )
            {
                return string.Empty;
            }
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
                    for ( int step = 1; step < keyPathInJson.Count; step++ )
                    {
                        sectionName = keyPathInJson [step];
                        section = section.GetSection (sectionName);
                    }
                }

                IConfigurationSection items = section.GetSection ("Items");
                IEnumerable<IConfigurationSection> targetChildren = items.GetChildren ();

                foreach ( IConfigurationSection unit in targetChildren )
                {
                    IConfigurationSection unitedSection = unit.GetSection ("United");
                    IEnumerable<IConfigurationSection> unitedSections = unitedSection.GetChildren ();
                }

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

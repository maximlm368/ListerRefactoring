using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.Configuration;
using static System.Collections.Specialized.BitVector32;
using QuestPDF.Infrastructure;
using ContentAssembler;

namespace DataGateway
{
    internal static class GetterFromJson
    {
        //private static IConfigurationRoot configRoot;
        private static string configFilePath;
        private static string attributeSection;


        internal static string GetSectionValue ( List<string> keyPathInJson, string jsonPath )
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

            string templateName = section.Value;
            return templateName;
        }


        internal static string GetSectionValue ( IConfigurationSection section )
        {
            string templateName = section.Value;
            return templateName;
        }


        internal static IEnumerable<IConfigurationSection> GetIncludedItemsOfSection
                                                                          ( List<string> keyPathInJson, string jsonPath )
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

            foreach ( IConfigurationSection unit   in   targetChildren )
            {
                IConfigurationSection unitedSection = unit.GetSection ("United");
                IEnumerable<IConfigurationSection> unitedSections = unitedSection.GetChildren ();

                foreach ( IConfigurationSection sect   in   unitedSections )
                {
                    string jjkj = sect.Value;

                    int fdd = 0;
                }
            }


            return targetChildren;
        }


        internal static IEnumerable<IConfigurationSection> GetChildrenOfSection ( IConfigurationSection parent )
        {
            IEnumerable<IConfigurationSection> targetChildren = parent.GetChildren ();

            return targetChildren;
        }


        private static IConfigurationRoot GetConfigRoot ( string jsonPath )
        {
            var builder = new ConfigurationBuilder ();
            builder.AddJsonFile (jsonPath);
            return builder.Build ();
        }
    }
}

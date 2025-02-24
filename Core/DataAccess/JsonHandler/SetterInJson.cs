using System.Text.Json.Serialization;
using System.Text.Json;

namespace Core.DataAccess.JsonHandlers
{
    public static class SetterInJson
    {
        public static void WritePersonSource ( string fileName, string keepablePath )
        {
            string path = keepablePath;

            if ( path == null )
            {
                path = string.Empty;
            }

            int limit = GetterFromJson.GetSectionIntValue ( new List<string> () { "maketLimit" }, fileName );

            Config config = new Config () { maketLimit = limit, personSource = path };

            string jsonStr = JsonSerializer.Serialize ( config );
            File.WriteAllText ( fileName, jsonStr );
        }


        private class Config
        {
            [JsonInclude]
            public int maketLimit;

            [JsonInclude]
            public string personSource;
        }
    }
}

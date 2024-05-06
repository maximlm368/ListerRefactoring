using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Lister.ViewModels;
using System.Text;

namespace Lister.Extentions 
{
    public static class StringExtention
    {
        public static string ExtractFileNameFromPath(this string path)
        {
            string result;
            var builder = new StringBuilder();

            for (var charCounter = path.Length - 1;    charCounter >= 0;    charCounter--)
            {
                if (path[charCounter] != '/'   &&   path[charCounter] != '\\')
                {
                    builder.Append(path[charCounter]);
                }
                else 
                {
                    break;
                }
            }

            result = builder.ToString().ReverseAndReturn();
            return result;
        }


        public static string ReverseAndReturn(this string str)
        {
            string result;
            var builder = new StringBuilder();

            for (var charCounter = str.Length - 1;   charCounter >= 0;    charCounter--)
            {
                builder.Append(str[charCounter]);
            }

            result = builder.ToString();
            return result;
        }


        public static List<string> SplitIntoRestAndLastWord ( this string str )
        {
            List<string> result = new List<string>();

            for ( var charCounter = str.Length - 1;   charCounter >= 0;   charCounter-- )
            {
                if ( str [charCounter] == ' '   ||   str [charCounter] == '-' ) 
                {
                    int gapLength = 1;
                    int endPartLength = str.Length - charCounter - gapLength;
                    string secondPart = str.Substring (charCounter + 1, endPartLength);
                    string firstPart = str.Substring (0, charCounter);
                    result.Add (firstPart);
                    result.Add (secondPart);

                    break;
                }
            }

            return result;
        }


        public static bool IsAllEmpty ( this string target, string [] parts )
        {
            bool result = true;

            for ( int index = 0;   index < parts.Length;   index++ )
            {
                if ( !string.IsNullOrWhiteSpace (parts [index]) )
                {
                    result = false;
                    break;
                }
            }

            return result;
        }

    }



    public static class ListExtensions 
    {
        public static List<T []> SeparateIntoPairs <T> ( this List<T> items ) where T : class
        {
            List<T []> result = new List<T []> ();
            int counterInPair = 0;
            T [] pair = [null, null];
            bool pairIsNotEmptyAlready = false;

            for ( int itemCounter = 0;   itemCounter < items.Count;   itemCounter++ )
            {
                pair [counterInPair] = items [itemCounter];
                pairIsNotEmptyAlready = true;

                if ( counterInPair == 1 )
                {
                    result.Add (pair);
                    pair = [null, null];
                    counterInPair = 0;
                    pairIsNotEmptyAlready = false;
                }
                else
                {
                    counterInPair++;
                }

                bool isLastPair = (itemCounter == items.Count - 1);

                if ( isLastPair   &&   pairIsNotEmptyAlready ) 
                {
                    result.Add (pair);
                }
            }

            return result;
        }
    }



    public static class ArrayExtensions
    {
        public static T [] SubArray<T> ( this T [] array, int offset, int length )
        {
            T [] result = new T [length];
            Array.Copy (array, offset, result, 0, length);

            //int scratch = offset;

            //for ( int index = 0;   index < length;   index++ ) 
            //{
            //    result [index] = array [scratch];
            //    scratch++;
            //}

            return result;
        }
    }



    public static class ImageHelper
    {
        public static Bitmap LoadFromResource(Uri resourceUri)
        {
            Bitmap bitmap = new Bitmap(AssetLoader.Open(resourceUri));
            return bitmap;
        }

        public static async Task<Bitmap?> LoadFromWeb(Uri url)
        {
            using var httpClient = new HttpClient();
            try
            {
                var response = await httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                var data = await response.Content.ReadAsByteArrayAsync();
                return new Bitmap(new MemoryStream(data));
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"An error occurred while downloading image '{url}' : {ex.Message}");
                return null;
            }
        }
    }



    public enum FileDialogMode 
    {
        pick = 0,
        save = 1
    }
}
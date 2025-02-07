using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Lister.ViewModels;
using System.Text;
using System.Collections;
using System.Diagnostics.Metrics;
using System.Collections.Immutable;

namespace Lister.Extentions
{
    public class BadgeComparer : IComparer <BadgeViewModel>
    {
        public int Compare ( BadgeViewModel ? x, BadgeViewModel ? y )
        {
            if ((x==null)  ||  (y==null)) 
            {
                return 0;
            }

            string xStringPresentation = x.BadgeModel.Person.StringPresentation;
            string yStringPresentation = y.BadgeModel.Person.StringPresentation;

            int res = string.Compare (xStringPresentation, yStringPresentation);

            return res;
        }
    }



    internal static class ListExtention
    {
        public static List <T> Clone <T> ( this List <T> source )
        {
            List <T> result = new List <T> ();

            if ( source == null )
            {
                return result;
            }

            for ( int index = 0;   index < source.Count;   index++ )
            {
                result.Add ( source[index] );
            }

            return result;
        }
    }



    public static class ImageHelper
    {
        public static Bitmap ? LoadFromResource (string source)
        {
            Bitmap bitmap = null;

            if ( File.Exists (source) ) 
            {
                bitmap = new Bitmap (source);
            }

            return bitmap;
        }


        public static async Task <Bitmap ?> LoadFromWeb (Uri url)
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
                return null;
            }
        }
    }
}
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



    public static class ImageHelper
    {
        public static Bitmap ? LoadFromResource (Uri resourceUri)
        {
            Bitmap bitmap = null;

            if ( File.Exists (resourceUri.AbsolutePath) ) 
            {
                using Stream stream = new FileStream (resourceUri.AbsolutePath, FileMode.Open);
                bitmap = new Bitmap (stream);
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
                Console.WriteLine($"An error occurred while downloading image '{url}' : {ex.Message}");
                return null;
            }
        }
    }
}
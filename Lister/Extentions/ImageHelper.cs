using Avalonia.Media.Imaging;

namespace View.Extentions;

internal static class ImageHelper
{
    public static Bitmap ? LoadFromResource ( string source )
    {
        Bitmap bitmap = null;

        if ( File.Exists ( source ) )
        {
            bitmap = new Bitmap ( source );
        }

        return bitmap;
    }
}
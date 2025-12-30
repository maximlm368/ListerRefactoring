using Avalonia.Media.Imaging;

namespace Lister.Desktop.Extentions;

internal static class ImageHelper
{
    public static Bitmap? LoadFromResource ( string? source )
    {
        return File.Exists ( source ) ? new Bitmap ( source ) : null;
    }
}

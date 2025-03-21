namespace Lister.Desktop.App.Configs;

internal static class MainViewConfigs
{
    internal static readonly string ProcentSymbol = "%";
    internal static readonly int MaxDepth = 5;
    internal static readonly int MinDepth = -5;

    internal static readonly string PdfFileName = "Badge";
    internal static readonly string SaveTitle = "Сохранение документа";
    internal static readonly string IncorrectXSLX = " - некорректный файл.";

    internal static readonly string FileIsTooBig = " - превышает лимит построения ";
    internal static readonly string LimitIsExhaustedMessage
                                         = "Макет не будет построен.  Количество бейджей не должно превышать ";

    internal static readonly string FileIsOpenMessage
                             = "Файл с таким именем открыт в другом приложении. Закройте его и повторите попытку.";

    internal static readonly string[] Patterns = ["*.pdf"];
}
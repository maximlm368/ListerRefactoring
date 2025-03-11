namespace View.App.Configs;

internal static class MainViewConfigs
{
    internal static readonly string procentSymbol = "%";
    internal static readonly int maxDepth = 5;
    internal static readonly int minDepth = -5;

    internal static readonly string pdfFileName = "Badge";
    internal static readonly string saveTitle = "Сохранение документа";
    internal static readonly string incorrectXSLX = " - некорректный файл.";

    internal static readonly string fileIsTooBig = " - превышает лимит построения ";
    internal static readonly string limitIsExhaustedMessage
                                         = "Макет не будет построен.  Количество бейджей не должно превышать ";

    internal static readonly string fileIsOpenMessage
                             = "Файл с таким именем открыт в другом приложении. Закройте его и повторите попытку.";

    internal static readonly string [] patterns = ["*.pdf"];
}
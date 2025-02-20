using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lister
{
    internal static class SceneConfigs
    {
        internal static readonly int badgeCountLimit = 3000;
        internal static readonly string extentionToolTip = "Развернуть панель";
        internal static readonly string shrinkingToolTip = "Свернуть панель";
        internal static readonly string fileIsOpenMessage =
                        "Файл с таким именем открыт в другом приложении, закройте его и повторите попытку.";
    }



    internal static class PersonChoosingConfigs
    {
        internal static readonly string placeHolder = "Весь список";
        internal static readonly int inputLimit = 50;
        
        internal static readonly string defaultBackgroundColor = "#ffffff";
        //internal static readonly string defaultBackgroundColor = "#00ff00";
        internal static readonly string defaultBorderColor = "#d5e8f6";
        internal static readonly string defaultForegroundColor = "#000000";

        internal static readonly string focusedBackgroundColor = "#e3f1fc";
        //internal static readonly string focusedBackgroundColor = "#000000";
        internal static readonly string focusedBorderColor = "#d5e8f6";

        internal static readonly string hoveredBackgroundColor = "#ffffff";
        internal static readonly string hoveredBorderColor = "#4aacfe";

        internal static readonly string selectedBackgroundColor = "#e3f1fc";
        //internal static readonly string selectedBackgroundColor = "#ff0000";
        internal static readonly string selectedBorderColor = "#d5e8f6";
        internal static readonly string selectedForegroundColor = "#3a5a81";

        internal static readonly string incorrectTemplateColor = "#8c8c8c";
    }



    internal static class PersonSourceConfigs
    {
        internal static readonly int badgeCountLimit = 3000;
        internal static readonly string pickerTitle = "Источники данных";
		internal static readonly string filePickerTitle = "Выбор файла";

        internal static readonly string [] xlsxHeaderNames = ["№", "Фамилия", "Имя", "Отчество", "Отделение", "Должность"];

        internal static readonly string [] sourceExtentions = ["*.csv", "*.xlsx"];
    }



    internal static class NavigationZoomerConfigs
    {
        internal static readonly string procentSymbol = "%";
        internal static readonly short maxDepth = 5;
        internal static readonly short minDepth = -5;
    }



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



    internal static class PrintDialogConfigs
    {
        internal static readonly string warnImagePath = "Icons/warning-alert.ico";
        internal static readonly string emptyCopies = "Количество копий не может быть пустым";
        internal static readonly string emptyPages = "Список страниц не может быть пустым";
        internal static readonly string emptyPrinters = "Список принтеров не может быть пустым";
    }



    internal static class EditorConfigs
    {
        internal static readonly string extentionToolTip = "Развернуть панель";
        internal static readonly string shrinkingToolTip = "Свернуть панель";
		internal static readonly string allFilter = "Все";
		internal static readonly string incorrectFilter = "С ошибками";
        internal static readonly string correctFilter = "Без ошибок";
        internal static readonly string allTip = "Все бейджи";
        internal static readonly string correctTip = "Бейджи без ошибок";
		internal static readonly string incorrectTip = "Бейджи с ошибками";

        internal static readonly string focusedFontColor = "#ffffff";
        internal static readonly string releasedFontColor = "#afafaf";
        internal static readonly string focusedFontBorderColor = "#323232";
        internal static readonly string releasedFontBorderColor = "#969696";
    }



    internal static class WaitingConfigs
    {
        internal static readonly string gifName = "Icons/Loading.gif";
    }
}



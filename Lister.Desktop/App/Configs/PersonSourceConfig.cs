namespace Lister.Desktop.App.Configs;

internal static class PersonSourceConfigs
{
    internal static readonly int BadgeCountLimit = 3000;
    internal static readonly string PickerTitle = "Источники данных";
    internal static readonly string FilePickerTitle = "Выбор файла";

    internal static readonly string[] XlsxHeaderNames = ["№", "Фамилия", "Имя", "Отчество", "Отделение", "Должность"];

    internal static readonly string[] SourceExtentions = ["*.csv", "*.xlsx"];
}
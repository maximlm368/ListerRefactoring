namespace View.App.Configs;


internal static class PersonSourceConfigs
{
    internal static readonly int badgeCountLimit = 3000;
    internal static readonly string pickerTitle = "Источники данных";
    internal static readonly string filePickerTitle = "Выбор файла";

    internal static readonly string [] xlsxHeaderNames = ["№", "Фамилия", "Имя", "Отчество", "Отделение", "Должность"];

    internal static readonly string [] sourceExtentions = ["*.csv", "*.xlsx"];
}
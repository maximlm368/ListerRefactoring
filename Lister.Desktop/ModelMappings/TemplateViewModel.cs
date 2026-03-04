using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Lister.Desktop.ModelMappings;

/// <summary>
/// Represents visible entity for badge template that corresponds particular badge layout.
/// </summary>
public sealed partial class TemplateViewModel : ObservableObject
{
    public string SourcePath { get; private set; }

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private SolidColorBrush? _color;

    public List<string> CorrectnessMessage { get; private set; }

    public TemplateViewModel ( string templateName, SolidColorBrush colorIfCorrect, SolidColorBrush colorIfIncorrect,
        List<string> errors, string sourcePath
    )
    {
        Name = templateName;
        CorrectnessMessage = errors;
        SourcePath = sourcePath;

        bool isCorrect = errors.Count == 0;

        if ( isCorrect )
        {
            Color = colorIfCorrect;
        }
        else
        {
            Color = colorIfIncorrect;
        }
    }
}

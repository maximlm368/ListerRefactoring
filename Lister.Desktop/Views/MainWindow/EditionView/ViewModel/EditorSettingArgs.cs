using Avalonia.Media;

namespace Lister.Desktop.Views.MainWindow.EditionView.ViewModel;

public class EditorViewModelArgs
{
    private static readonly SolidColorBrush defaultBrash = new ( new Color ( 0, 0, 0, 0 ) );

    public string ExtentionToolTip = string.Empty;
    public string ShrinkingToolTip = string.Empty;
    public string AllFilter = string.Empty;
    public string IncorrectFilter = string.Empty;
    public string CorrectFilter = string.Empty;
    public string AllTip = string.Empty;
    public string CorrectTip = string.Empty;
    public string IncorrectTip = string.Empty;
    public SolidColorBrush FocusedFontSizeColor = defaultBrash;
    public SolidColorBrush ReleasedFontSizeColor = defaultBrash;
    public SolidColorBrush FocusedFontSizeBorderColor = defaultBrash;
    public SolidColorBrush ReleasedFontSizeBorderColor = defaultBrash;
}
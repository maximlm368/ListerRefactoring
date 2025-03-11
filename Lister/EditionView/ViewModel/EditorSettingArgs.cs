using Avalonia.Media;

namespace View.EditionView.ViewModel;

public class EditorViewModelArgs
{
    public string extentionToolTip;
    public string shrinkingToolTip;
    public string allFilter;
    public string incorrectFilter;
    public string correctFilter;
    public string allTip;
    public string correctTip;
    public string incorrectTip;

    public SolidColorBrush focusedFontSizeColor;
    public SolidColorBrush releasedFontSizeColor;
    public SolidColorBrush focusedFontSizeBorderColor;
    public SolidColorBrush releasedFontSizeBorderColor;
}
using Avalonia.Controls;

namespace View.ViewBase;

public abstract class ShowingDialog : UserControl
{
    public abstract void HandleDialogClosing();
}
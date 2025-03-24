using Avalonia.Controls;

namespace Lister.Desktop.Views.ViewBase;

public abstract class ShowingDialog : UserControl
{
    public abstract void HandleDialogClosing();
}
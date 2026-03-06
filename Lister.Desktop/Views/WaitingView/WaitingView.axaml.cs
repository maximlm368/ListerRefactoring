using Avalonia.Controls;

namespace Lister.Desktop.Views.WaitingView;

/// <summary>
/// Is view that is visible only while some asynchronous long time action like badge building or pdf creation is occurring.
/// </summary>
public sealed partial class WaitingViewUserControl : UserControl
{
    public WaitingViewUserControl()
    {
        InitializeComponent ();
    }
}

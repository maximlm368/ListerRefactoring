using Avalonia;

namespace Lister.Desktop.Components.MeasurableLable;

internal class MeasurableLabel : Avalonia.Controls.Label
{
    public static readonly StyledProperty<double> AccessableWidthProperty =
        AvaloniaProperty.Register<MeasurableLabel, double> ( "AccessableWidth" );
    public double AccessableWidth
    {
        get => GetValue ( AccessableWidthProperty );
        set => SetValue ( AccessableWidthProperty, value );
    }

    public MeasurableLabel () : base () 
    {
        Loaded += ( sender, args ) =>
        {
            AccessableWidth = Bounds.Width - Padding.Left;
        };
    }
}

using Avalonia.Controls;
using Avalonia.Input;

namespace Lister.Desktop.Views.MainView.Parts.BuildButton;

public partial class BadgesBuildingUserControl : UserControl
{
    public event Action? SomePartPressed;

    public BadgesBuildingUserControl ()
    {
        InitializeComponent ();

        //DataContext = ListerApp.Services.GetRequiredService<BadgesBuildingViewModel> ();
        //BuildBadges.FocusAdorner = null;
    }

    private void RightPointerPressed ( object sender, PointerPressedEventArgs args )
    {
        PointerPoint point = args.GetCurrentPoint ( sender as Control );

        if ( point.Properties.IsRightButtonPressed )
        {
            SomePartPressed?.Invoke ();
            Focus ();
        }
    }

    internal void ChangeSize ( double widthDifference )
    {
        foreach ( Control? child in LeftShadow.Children )
        {
            if ( child.Width > 10 )
            {
                child.Width -= widthDifference / 2;
            }
            else
            {
                double left = child.GetValue ( Canvas.LeftProperty );
                child.SetValue ( Canvas.LeftProperty, left - widthDifference / 2 );
            }
        }

        foreach ( Control? child in RightShadow.Children )
        {
            if ( child.Width > 10 )
            {
                child.Width -= widthDifference / 2;
            }
        }
    }
}

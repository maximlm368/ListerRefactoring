using Avalonia.Controls;
using Avalonia.Input;
using Microsoft.Extensions.DependencyInjection;
using View.MainWindow.MainView.Parts.BuildButton.ViewModel;
using View.App;
using Lister.Desktop.CoreModelReflections;

namespace View.MainWindow.MainView.Parts.BuildButton;

public partial class BadgesBuildingUserControl : UserControl
{
    private static readonly string _jsonError = 
    "Невозможно загрузить этот шаблон.Обратитесь к разработчику по телефону 324-708";

    private static TemplateViewModel _chosen;

    private MainView _parent;
    private BadgesBuildingViewModel _viewModel;
    private string _theme;


    public BadgesBuildingUserControl ()
    {
        InitializeComponent ();

        DataContext = ListerApp.Services.GetRequiredService<BadgesBuildingViewModel> ();
        _viewModel = ( BadgesBuildingViewModel ) DataContext;
        buildBadges.FocusAdorner = null;
    }


    private void RightPointerPressed ( object sender, PointerPressedEventArgs args )
    {
        var point = args.GetCurrentPoint (sender as Control);

        if ( point.Properties.IsRightButtonPressed )
        {
            MainView.SomeControlPressed = true;
            this.Focus ();
        }
    }


    //internal void ThemeChanged ( object sender, EventArgs args )
    //{
    //    if ( ActualThemeVariant == null ) 
    //    {
    //        return;
    //    }

    //    _theme = ActualThemeVariant.Key.ToString ();
    //    //_viewModel.ChangeAccordingTheme ( _theme );
    //}


    internal void ChangeSize ( double widthDifference )
    {
        var leftChildren = leftShadow.Children;
        var rightChildren = rightShadow.Children;

        foreach ( var child   in   leftChildren )
        {
            if ( child.Width > 10 )
            {
                child.Width -= widthDifference / 2;
            }
            else 
            {
                double left = child.GetValue (Canvas.LeftProperty);
                child.SetValue (Canvas.LeftProperty, left - widthDifference / 2);
            }
        }

        foreach ( var child   in   rightChildren )
        {
            if ( child.Width > 10 )
            {
                child.Width -= widthDifference / 2;
            }
        }
    }
}

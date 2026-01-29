using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using Lister.Core.Entities;

namespace Lister.Desktop.ModelMappings.BadgeVM;

/// <summary>
/// Maps Person model into visible entity.
/// </summary>
internal sealed partial class VisiblePerson : ObservableObject
{
    private static SolidColorBrush? _defaultBorderColor;
    private static SolidColorBrush? _defaultBackgroundColor;
    private static SolidColorBrush? _defaultForegroundColor;

    private static SolidColorBrush? _selectedBorderColor;
    private static SolidColorBrush? _selectedBackgroundColor;
    private static SolidColorBrush? _selectedForegroundColor;

    private static SolidColorBrush? _focusedBorderColor;
    private static SolidColorBrush? _focusedBackgroundColor;

    private bool _isFocused;
    internal bool IsFocused
    {
        get 
        { 
            return _isFocused; 
        }

        set
        {
            if ( value )
            {
                BorderColor = _focusedBorderColor;
                BackgroundColor = _focusedBackgroundColor;

                if ( IsSelected )
                {
                    ForegroundColor = _selectedForegroundColor;
                }
            }
            else
            {
                CancelFocused ();
            }

            _isFocused = value;
        }
    }

    private bool _isSelected;
    internal bool IsSelected
    {
        get
        { 
            return _isSelected; 
        }

        set
        {
            if ( value )
            {
                BorderColor = _selectedBorderColor;
                BackgroundColor = _selectedBackgroundColor;
                ForegroundColor = _selectedForegroundColor;
                FontWeight = FontWeight.Bold;

                if ( !IsFocused )
                {
                    IsFocused = true;
                }
            }
            else
            {
                CancelSeleted ();
            }

            _isSelected = value;
        }
    }

    internal Person Model { get; private set; }

    [ObservableProperty]
    private SolidColorBrush? _borderColor;

    [ObservableProperty]
    private SolidColorBrush? _backgroundColor;

    [ObservableProperty]
    private SolidColorBrush? _foregroundColor;

    [ObservableProperty]
    private FontWeight _fontWeight;

    public static void SetColors ( List<SolidColorBrush> defaultColors, List<SolidColorBrush> focusedColors, List<SolidColorBrush> selectedColors )
    {
        _defaultBackgroundColor = defaultColors [0];
        _defaultBorderColor = defaultColors [1];
        _defaultForegroundColor = defaultColors [2];

        _selectedBackgroundColor = selectedColors [0];
        _selectedBorderColor = selectedColors [1];
        _selectedForegroundColor = selectedColors [2];

        _focusedBackgroundColor = focusedColors [0];
        _focusedBorderColor = focusedColors [1];
    }

    internal VisiblePerson ( Person person )
    {
        Model = person;
        IsFocused = false;
        IsSelected = false;
    }

    public override string ToString ()
    {
        return Model.FullName;
    }

    private void CancelFocused ()
    {
        BorderColor = _defaultBorderColor;

        if ( !IsSelected )
        {
            ForegroundColor = _defaultForegroundColor;
            BackgroundColor = _defaultBackgroundColor;
            FontWeight = FontWeight.Normal;
        }
    }

    private void CancelSeleted ()
    {
        if ( IsFocused )
        {
            BorderColor = _focusedBorderColor;
            BackgroundColor = _focusedBackgroundColor;
        }
        else
        {
            BorderColor = _defaultBorderColor;
            BackgroundColor = _defaultBackgroundColor;
        }

        ForegroundColor = _defaultForegroundColor;
        FontWeight = FontWeight.Normal;
    }
}

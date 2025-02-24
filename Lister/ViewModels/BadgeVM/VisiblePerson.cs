using Avalonia.Media;
using Core.Models;
using ReactiveUI;

namespace Lister.ViewModels
{
    internal class VisiblePerson : ReactiveObject
    {
        private static bool _colorsIsSet = false;

        private static SolidColorBrush _defaultBorderColor;
        private static SolidColorBrush _defaultBackgroundColor;
        private static SolidColorBrush _defaultForegroundColor;

        private static SolidColorBrush _selectedBorderColor;
        private static SolidColorBrush _selectedBackgroundColor;
        private static SolidColorBrush _selectedForegroundColor;

        private static SolidColorBrush _focusedBorderColor;
        private static SolidColorBrush _focusedBackgroundColor;

        private bool _isFocused;
        internal bool IsFocused 
        {
            get { return _isFocused; }
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
            get { return _isSelected; }
            set
            {
                if ( value )
                {
                    BorderColor = _selectedBorderColor;
                    BackgroundColor = _selectedBackgroundColor;
                    ForegroundColor = _selectedForegroundColor;
                    FontWeight = FontWeight.Bold;

                    if ( ! IsFocused ) 
                    {
                        IsFocused = true;
                    }
                }
                else 
                {
                    CancelSeleted();
                }

                _isSelected = value;
            }
        }

        internal Person Person { get; private set; }

        private SolidColorBrush _borderColor;
        internal SolidColorBrush BorderColor
        {
            get { return _borderColor; }
            private set
            {
                this.RaiseAndSetIfChanged (ref _borderColor, value, nameof (BorderColor));
            }
        }

        private SolidColorBrush _backgroundColor;
        internal SolidColorBrush BackgroundColor
        {
            get { return _backgroundColor; }
            private set
            {
                this.RaiseAndSetIfChanged (ref _backgroundColor, value, nameof (BackgroundColor));
            }
        }

        private SolidColorBrush _foregroundColor;
        internal SolidColorBrush ForegroundColor
        {
            get { return _foregroundColor; }
            private set
            {
                this.RaiseAndSetIfChanged (ref _foregroundColor, value, nameof (ForegroundColor));
            }
        }

        private FontWeight _fontWeight;
        internal FontWeight FontWeight
        {
            get { return _fontWeight; }
            private set
            {
                this.RaiseAndSetIfChanged (ref _fontWeight, value, nameof (FontWeight));
            }
        }


        public static void SetColors ( List <SolidColorBrush> defaultColors, List <SolidColorBrush> focusedColors
                                                                          , List <SolidColorBrush> selectedColors ) 
        {
            if ( !_colorsIsSet ) 
            {
                _defaultBackgroundColor = defaultColors [0];
                _defaultBorderColor = defaultColors [1];
                _defaultForegroundColor = defaultColors [2];

                _selectedBackgroundColor = selectedColors [0];
                _selectedBorderColor = selectedColors [1];
                _selectedForegroundColor = selectedColors [2];

                _focusedBackgroundColor = focusedColors [0];
                _focusedBorderColor = focusedColors [1];
                
                _colorsIsSet = true;
            }
        }


        internal VisiblePerson ( Person person ) 
        {
            Person = person;
            IsFocused = false;
            IsSelected = false;
        }


        private void CancelFocused () 
        {
            BorderColor = _defaultBorderColor;
            
            if ( ! IsSelected )
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
}

using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using Lister.Core.Document;
using Lister.Core.Entities;
using Lister.Core.Entities.Badge;
using Lister.Desktop.Entities;
using Lister.Desktop.Entities.BadgeVM;
using Lister.Desktop.Extentions;
using Lister.Desktop.Services;
using System.Collections.ObjectModel;

namespace Lister.Desktop.Views.MainView.Parts.PersonChoosing.ViewModel;

internal partial class PersonChoosingViewModel : ObservableObject
{
    private static readonly SolidColorBrush _black = new ( Colors.Black );
    private static SolidColorBrush _correctTemplateForeground = _black;
    private static SolidColorBrush _incorrectTemplateForeground = _black;

    private Dictionary<Layout, KeyValuePair<string, List<string>>>? _badgeLayouts;
    private readonly DocumentProcessor? _documentProcessor;

    private VisiblePerson? _selected;
    public VisiblePerson? Selected
    {
        get => _selected;

        set
        {
            _selected = value;
            ChosenPerson = value?.Model;
        }
    }

    private TemplateViewModel? _chosenTemplate;
    internal TemplateViewModel? ChosenTemplate
    {
        get => _chosenTemplate;

        set
        {
            if ( value == null || value.Color == null )
            {
                return;
            }

            _chosenTemplate = value;
            OnPropertyChanged ();

            SickTemplateIsSet = ( value.Color.Color.R != 0 )
                                || ( value.Color.Color.G != 0 )
                                || ( value.Color.Color.B != 0 );

            SetReadiness ();
        }
    }

    [ObservableProperty]
    private ObservableCollection<TemplateViewModel>? _templates;

    [ObservableProperty]
    private ObservableCollection<VisiblePerson>? _people;

    [ObservableProperty]
    private Person? _chosenPerson;

    [ObservableProperty]
    private bool _settingsIsComplated;

    private bool _sickTemplateIsSet;
    public bool SickTemplateIsSet
    {
        get => _sickTemplateIsSet;

        private set
        {
            _sickTemplateIsSet = true;
            OnPropertyChanged ();
        }
    }

    [ObservableProperty]
    private bool _choiceIsDisabled;

    [ObservableProperty]
    private bool _fileNotFound;

    public PersonChoosingViewModel ()
    {

    }

    public PersonChoosingViewModel ( SolidColorBrush incorrectTemplateForeground, SolidColorBrush correctTemplateForeground, 
        DocumentProcessor documentProcessor 
    )
    {
        _incorrectTemplateForeground = incorrectTemplateForeground;
        _correctTemplateForeground = correctTemplateForeground;
        ChoiceIsDisabled = true;
        _documentProcessor = documentProcessor;
    }

    internal void ResetPersons ( )
    {
        List<Person>? people = _documentProcessor?.People;

        if ( people == null )
        {
            ChoiceIsDisabled = true;
            FileNotFound = true;

            return;
        }

        List<VisiblePerson>? visiblePeople = people?.Clone ()
             ?.Where ( person => !person.IsEmpty () )
             .Select ( person => new VisiblePerson ( person ) )
             .OrderBy ( person => person.Model.FullName )
             .ToList ();

        People = visiblePeople != null ? new ObservableCollection<VisiblePerson> ( visiblePeople ) : null;
        ChosenPerson = null;
        ChoiceIsDisabled = people == null;
    }

    private void SetReadiness ()
    {
        SettingsIsComplated = People != null
            && People.Count > 0
            && ChosenTemplate != null;
    }

    internal void SetUp ()
    {
        ObservableCollection<TemplateViewModel> templates = [];
        _badgeLayouts ??= BadgeLayoutProvider.GetInstance ().GetBadgeLayouts ();

        foreach ( KeyValuePair<Layout, KeyValuePair<string, List<string>>> layout in _badgeLayouts )
        {
            List<string> errors = layout.Value.Value;
            string source = layout.Value.Key;

            templates.Add (
                new TemplateViewModel ( layout.Key.TemplateName,
                    _correctTemplateForeground,
                    _incorrectTemplateForeground,
                    errors,
                    source
                )
            );
        }

        Templates = templates;
    }
}

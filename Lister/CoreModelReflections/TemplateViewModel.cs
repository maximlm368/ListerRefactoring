using Avalonia.Media;
using ReactiveUI;

namespace View.CoreModelReflection;

public class TemplateViewModel : ReactiveObject
{
    private TemplateName TemplateName { get; set; }
    public string SourcePath { get; private set; }

    private string _name;
    public string Name
    {
        get
        {
            return _name;
        }
        set
        {
            this.RaiseAndSetIfChanged ( ref _name, value, nameof ( Name ) );
        }
    }

    private SolidColorBrush _color;
    public SolidColorBrush Color
    {
        get
        {
            return _color;
        }
        set
        {
            this.RaiseAndSetIfChanged ( ref _color, value, nameof ( Color ) );
        }
    }

    private List<string> _correctnessMessage;
    public List<string> CorrectnessMessage
    {
        get
        {
            return _correctnessMessage;
        }
        set
        {
            _correctnessMessage = value;
        }
    }


    public TemplateViewModel ( TemplateName templateName, SolidColorBrush colorIfCorrect, SolidColorBrush colorIfIncorrect
                              , List<string> correctnessMessage, string sourcePath )
    {
        TemplateName = templateName;
        Name = templateName.Name;
        CorrectnessMessage = correctnessMessage;
        SourcePath = sourcePath;

        bool isCorrect = ( correctnessMessage.Count == 0 );

        if ( isCorrect )
        {
            Color = colorIfCorrect;
        }
        else
        {
            Color = colorIfIncorrect;
        }
    }
}
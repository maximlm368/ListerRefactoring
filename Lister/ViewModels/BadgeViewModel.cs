using Avalonia.Media.Imaging;
using Avalonia.Platform;
using ContentAssembler;
using ReactiveUI;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using static QuestPDF.Helpers.Colors;
using Lister.Extentions;
using System.Collections.ObjectModel;
using Avalonia.Media;
using Avalonia.Layout;
using System.Globalization;
using Avalonia.Controls;
using Avalonia.Interactivity;
using ExtentionsAndAuxiliary;

namespace Lister.ViewModels;

class BadgeViewModel : ViewModelBase
{
    private const double coefficient = 1.1;
    internal Badge BadgeModel { get; private set; }
    internal Bitmap ImageBitmap { get; private set; }

    private string lN;
    internal string LastName
    {
        get { return lN; }
        set
        {
            this.RaiseAndSetIfChanged (ref lN, value, nameof (LastName));
        }
    }

    private string fsN;
    internal string FirstAndSecondName
    {
        get { return fsN; }
        set
        {
            this.RaiseAndSetIfChanged (ref fsN, value, nameof (FirstAndSecondName));
        }
    }

    private string dN;
    internal string DepartmentName
    {
        get { return dN; }
        set
        {
            this.RaiseAndSetIfChanged (ref dN, value, nameof (DepartmentName));
        }
    }

    private string pN;
    internal string PositionName
    {
        get { return pN; }
        set
        {
            this.RaiseAndSetIfChanged (ref pN, value, nameof (PositionName));
        }
    }

    private ObservableCollection<string> rL;
    internal ObservableCollection<string> ReserveLastNameTextBlocks
    {
        get { return rL; }
        set
        {
            this.RaiseAndSetIfChanged (ref rL, value, nameof (ReserveLastNameTextBlocks));
        }
    }

    private ObservableCollection<string> rFM;
    internal ObservableCollection<string> ReserveFirstAndMiddleNamesTextBlocks
    {
        get { return rFM; }
        set
        {
            this.RaiseAndSetIfChanged (ref rFM, value, nameof (ReserveFirstAndMiddleNamesTextBlocks));
        }
    }

    private ObservableCollection<string> rd;
    internal ObservableCollection<string> ReserveDepartmentTextBlocks
    {
        get { return rd; }
        set
        {
            this.RaiseAndSetIfChanged (ref rd, value, nameof (ReserveDepartmentTextBlocks));
        }
    }

    private ObservableCollection<string> rp;
    internal ObservableCollection<string> ReservePositionTextBlocks
    {
        get { return rp; }
        set
        {
            this.RaiseAndSetIfChanged (ref rp, value, nameof (ReservePositionTextBlocks));
        }
    }

    private double pH;
    internal double PersonalTextContainerHeight
    {
        get { return pH; }
        set
        {
            this.RaiseAndSetIfChanged (ref pH, value, nameof (PersonalTextContainerHeight));
        }
    }

    private double bW;
    internal double BadgeWidth
    {
        get { return bW; }
        set
        {
            this.RaiseAndSetIfChanged (ref bW, value, nameof (BadgeWidth));
        }
    }

    private double bH;
    internal double BadgeHeight
    {
        get { return bH; }
        set
        {
            this.RaiseAndSetIfChanged (ref bH, value, nameof (BadgeHeight));
        }
    }

    private double tS;
    internal double TextAreaTopShift
    {
        get { return tS; }
        set
        {
            this.RaiseAndSetIfChanged (ref tS, value, nameof (TextAreaTopShift));
        }
    }

    private double lS;
    internal double TextAreaLeftShift
    {
        get { return lS; }
        set
        {
            this.RaiseAndSetIfChanged (ref lS, value, nameof (TextAreaLeftShift));
        }
    }

    private double tW;
    internal double TextAreaWidth
    {
        get { return tW; }
        set
        {
            this.RaiseAndSetIfChanged (ref tW, value, nameof (TextAreaWidth));
        }
    }

    private double tH;
    internal double TextAreaHeight
    {
        get { return tH; }
        set
        {
            this.RaiseAndSetIfChanged (ref tH, value, nameof (TextAreaHeight));
        }
    }

    private double fFS;
    internal double FirstLevelFontSize
    {
        get { return fFS; }
        set
        {
            this.RaiseAndSetIfChanged (ref fFS, value, nameof (FirstLevelFontSize));
        }
    }

    private double sFS;
    internal double SecondLevelFontSize
    {
        get { return sFS; }
        set
        {
            this.RaiseAndSetIfChanged (ref sFS, value, nameof (SecondLevelFontSize));
        }
    }

    private double tFS;
    internal double ThirdLevelFontSize
    {
        get { return tFS; }
        set
        {
            this.RaiseAndSetIfChanged (ref tFS, value, nameof (ThirdLevelFontSize));
        }
    }

    private double fH;
    internal double FirstLevelTBHeight
    {
        get { return fH; }
        set
        {
            this.RaiseAndSetIfChanged (ref fH, value, nameof (FirstLevelTBHeight));
        }
    }

    private double sH;
    internal double SecondLevelTBHeight
    {
        get { return sH; }
        set
        {
            this.RaiseAndSetIfChanged (ref sH, value, nameof (SecondLevelTBHeight));
        }
    }

    private double thH;
    internal double ThirdLevelTBHeight
    {
        get { return thH; }
        set
        {
            this.RaiseAndSetIfChanged (ref thH, value, nameof (ThirdLevelTBHeight));
        }
    }

    private double dTP;
    internal double DepartmentTopPadding
    {
        get { return dTP; }
        set
        {
            this.RaiseAndSetIfChanged (ref dTP, value, nameof (DepartmentTopPadding));
        }
    }

    private double pTP;
    internal double PostTopPadding
    {
        get { return pTP; }
        set
        {
            this.RaiseAndSetIfChanged (ref pTP, value, nameof (PostTopPadding));
        }
    }

    private double borderThickness;
    private Avalonia.Thickness bT;
    internal Avalonia.Thickness BorderThickness
    {
        get { return bT; }
        set
        {
            this.RaiseAndSetIfChanged (ref bT, value, nameof (BorderThickness));
        }
    }

    internal bool IsCorrect { get; private set; }


    internal BadgeViewModel ( Badge badgeModel )
    {
        this.BadgeModel = badgeModel;
        BadgeWidth = badgeModel.badgeDescription.badgeDimensions.outlineSize.width;
        BadgeHeight = badgeModel.badgeDescription.badgeDimensions.outlineSize.height;
        TextAreaTopShift = badgeModel.badgeDescription.badgeDimensions.personTextAreaTopShiftOnBackground;
        TextAreaLeftShift = badgeModel.badgeDescription.badgeDimensions.personTextAreaLeftShiftOnBackground;
        TextAreaWidth = badgeModel.badgeDescription.badgeDimensions.personTextAreaSize.width;
        TextAreaHeight = badgeModel.badgeDescription.badgeDimensions.personTextAreaSize.height;
        FirstLevelFontSize = badgeModel.badgeDescription.badgeDimensions.firstLevelFontSize;
        SecondLevelFontSize = badgeModel.badgeDescription.badgeDimensions.secondLevelFontSize;
        ThirdLevelFontSize = badgeModel.badgeDescription.badgeDimensions.thirdLevelFontSize;
        FirstLevelTBHeight = badgeModel.badgeDescription.badgeDimensions.firstLevelTBHeight;
        SecondLevelTBHeight = badgeModel.badgeDescription.badgeDimensions.secondLevelTBHeight;
        ThirdLevelTBHeight = badgeModel.badgeDescription.badgeDimensions.thirdLevelTBHeight;

        DepartmentTopPadding = 10;
        PostTopPadding = 5;

        LastName = badgeModel.person.FamilyName;
        FirstAndSecondName = badgeModel.person.FirstName + " " + badgeModel.person.PatronymicName;
        DepartmentName = badgeModel.person.Department;
        PositionName = badgeModel.person.Post;
        ReserveLastNameTextBlocks = new ObservableCollection<string> ();
        ReserveFirstAndMiddleNamesTextBlocks = new ObservableCollection<string> ();
        ReserveDepartmentTextBlocks = new ObservableCollection<string> ();
        ReservePositionTextBlocks = new ObservableCollection<string> ();

        PersonalTextContainerHeight = 0;
        IsCorrect = true;
        borderThickness = 1;
        BorderThickness = new Avalonia.Thickness (borderThickness);

        FitTextDataInContainer ();
    }

    public event PropertyChangedEventHandler? PropertyChanged;


    internal void ShowBackgroundImage ()
    {
        string path = BadgeModel.backgroundImagePath;
        Uri uri = new Uri (path);
        this.ImageBitmap = ImageHelper.LoadFromResource (uri);
    }


    internal void HideBackgroundImage ()
    {
        this.ImageBitmap = null;
    }


    internal void ZoomOn ( double coefficient )
    {
        BadgeWidth *= coefficient;
        BadgeHeight *= coefficient;
        TextAreaLeftShift *= coefficient;
        TextAreaTopShift *= coefficient;
        TextAreaWidth *= coefficient;
        TextAreaHeight *= coefficient;
        FirstLevelFontSize *= coefficient;
        SecondLevelFontSize *= coefficient;
        ThirdLevelFontSize *= coefficient;
        FirstLevelTBHeight *= coefficient;
        SecondLevelTBHeight *= coefficient;
        ThirdLevelTBHeight *= coefficient;
        DepartmentTopPadding *= coefficient;
        PostTopPadding *= coefficient;

        borderThickness *= coefficient;
        BorderThickness = new Avalonia.Thickness (borderThickness);
    }


    internal void ZoomOut ( double coefficient )
    {
        BadgeWidth /= coefficient;
        BadgeHeight /= coefficient;
        TextAreaLeftShift /= coefficient;
        TextAreaTopShift /= coefficient;
        TextAreaWidth /= coefficient;
        TextAreaHeight /= coefficient;
        FirstLevelFontSize /= coefficient;
        SecondLevelFontSize /= coefficient;
        ThirdLevelFontSize /= coefficient;
        FirstLevelTBHeight /= coefficient;
        SecondLevelTBHeight /= coefficient;
        ThirdLevelTBHeight /= coefficient;
        DepartmentTopPadding /= coefficient;
        PostTopPadding /= coefficient;

        borderThickness /= coefficient;
        BorderThickness = new Avalonia.Thickness (borderThickness);
    }


    internal VMBadge Clone ()
    {
        VMBadge clone = new VMBadge (this.BadgeModel);
        return clone;
    }


    internal void Zoom ( double coefficient )
    {
        if ( coefficient != 1 )
        {
            ZoomOn (coefficient);
        }
    }


    private void FitTextDataInContainer ()
    {
        FitLastNameInContainer ();
        FitFirstAndMiddleNamesInContainer ();
        FitDepartmentInContainer ();
        FitPositionInContainer ();

        PersonalTextContainerHeight = PersonalTextContainerHeight + FirstLevelTBHeight + SecondLevelTBHeight
                                                                  + ThirdLevelTBHeight * 2;

        for ( int lineCounter = 0; lineCounter < ReserveLastNameTextBlocks.Count; lineCounter++ )
        {
            PersonalTextContainerHeight += FirstLevelTBHeight;
        }

        for ( int lineCounter = 0; lineCounter < ReserveFirstAndMiddleNamesTextBlocks.Count; lineCounter++ )
        {
            PersonalTextContainerHeight += SecondLevelTBHeight;
        }

        for ( int lineCounter = 0; lineCounter < ReserveDepartmentTextBlocks.Count; lineCounter++ )
        {
            PersonalTextContainerHeight += ThirdLevelTBHeight;
        }

        for ( int lineCounter = 0; lineCounter < ReservePositionTextBlocks.Count; lineCounter++ )
        {
            PersonalTextContainerHeight += ThirdLevelTBHeight;
        }

        if ( PersonalTextContainerHeight >= TextAreaHeight )
        {
            IsCorrect = false;
        }
    }


    private void FitLastNameInContainer ()
    {
        string beingProcessedLine = LastName;
        List<string> stack = new List<string> ();
        stack.Add (beingProcessedLine);

        while ( true )
        {
            string additionalLine = AddLineIfCurrentOverFlow (FirstLevelFontSize, stack);

            if ( additionalLine == string.Empty )
            {
                break;
            }
        }

        if ( stack.Count > 1 )
        {
            LastName = stack [0];

            for ( int stepCounter = 1; stepCounter < stack.Count; stepCounter++ )
            {
                ReserveLastNameTextBlocks.Add (stack [stepCounter]);
            }
        }
    }


    private void FitFirstAndMiddleNamesInContainer ()
    {
        string beingProcessedLine = FirstAndSecondName;
        List<string> stack = new List<string> ();
        stack.Add (beingProcessedLine);

        while ( true )
        {
            string additionalLine = AddLineIfCurrentOverFlow (SecondLevelFontSize, stack);

            if ( additionalLine == string.Empty )
            {
                break;
            }
        }

        if ( stack.Count > 1 )
        {
            FirstAndSecondName = stack [0];

            for ( int stepCounter = 1; stepCounter < stack.Count; stepCounter++ )
            {
                ReserveFirstAndMiddleNamesTextBlocks.Add (stack [stepCounter]);
            }
        }
    }


    private void FitDepartmentInContainer ()
    {
        string beingProcessedLine = DepartmentName;
        List<string> stack = new List<string> ();
        stack.Add (beingProcessedLine);

        while ( true )
        {
            string additionalLine = AddLineIfCurrentOverFlow (ThirdLevelFontSize, stack);

            if ( additionalLine == string.Empty )
            {
                break;
            }
        }

        if ( stack.Count > 1 )
        {
            DepartmentName = stack [0];

            for ( int stepCounter = 1; stepCounter < stack.Count; stepCounter++ )
            {
                ReserveDepartmentTextBlocks.Add (stack [stepCounter]);
            }
        }
    }


    private void FitPositionInContainer ()
    {
        string beingProcessedLine = PositionName;
        List<string> stack = new List<string> ();
        stack.Add (beingProcessedLine);

        while ( true )
        {
            string additionalLine = AddLineIfCurrentOverFlow (ThirdLevelFontSize, stack);

            if ( additionalLine == string.Empty )
            {
                break;
            }
        }

        if ( stack.Count > 1 )
        {
            PositionName = stack [0];

            for ( int stepCounter = 1; stepCounter < stack.Count; stepCounter++ )
            {
                ReservePositionTextBlocks.Add (stack [stepCounter]);
            }
        }
    }


    private string AddLineIfCurrentOverFlow ( double fontSize, List<string> lineStack )
    {
        string beingProcessedLine = lineStack.Last ();
        int positionInStack = lineStack.Count - 1;
        string additionalLine = string.Empty;
        FormattedText formatted = new FormattedText
                    (beingProcessedLine, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, Typeface.Default, fontSize, null);
        double usefulTextBlockWidth = formatted.Width * coefficient;
        bool lineIsOverflow = ( usefulTextBlockWidth >= TextAreaWidth );

        while ( lineIsOverflow )
        {
            List<string> splited = beingProcessedLine.SeparateIntoMainAndTailViaLastSeparator (new List<char> { '/', '\\' });

            if ( splited.Count > 0 )
            {
                beingProcessedLine = splited [0];
                additionalLine = splited [1] + " " + additionalLine;

                formatted = new FormattedText
                    (beingProcessedLine, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, Typeface.Default, fontSize, null);
                usefulTextBlockWidth = formatted.Width * coefficient;
                lineIsOverflow = ( usefulTextBlockWidth >= TextAreaWidth );
            }
            else
            {
                break;
            }

        }

        if ( additionalLine != string.Empty )
        {
            lineStack.RemoveAt (positionInStack);
            lineStack.Add (beingProcessedLine);
            lineStack.Add (additionalLine);
        }

        return additionalLine;
    }
}
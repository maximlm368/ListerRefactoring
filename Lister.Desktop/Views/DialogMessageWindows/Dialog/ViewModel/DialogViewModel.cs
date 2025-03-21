﻿using Avalonia.Media.Imaging;
using ReactiveUI;
using View.App;
using Lister.Desktop.Extentions;

namespace View.DialogMessageWindows.Dialog.ViewModel;

public class DialogViewModel : ReactiveObject
{
    private static string _warnImageName = "Icons/warning-alert.ico";

    private DialogWindow _view;

    private Bitmap _warnImage;
    internal Bitmap WarnImage
    {
        get { return _warnImage; }
        private set
        {
            this.RaiseAndSetIfChanged( ref _warnImage, value, nameof( WarnImage ) );
        }
    }


    public DialogViewModel(DialogWindow view)
    {
        _view = view;

        string correctnessIcon = ListerApp.ResourceFolderName + _warnImageName;
        WarnImage = ImageHelper.LoadFromResource( correctnessIcon );
    }


    internal void ChooseYes()
    {
        _view.ChooseYes();
    }


    internal void ChooseNo()
    {
        _view.ChooseNo();
    }
}

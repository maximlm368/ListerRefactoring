﻿using Avalonia.Media.Imaging;
using Lister.Desktop.App;
using ReactiveUI;

namespace Lister.Desktop.Views.DialogMessageWindows.Message.ViewModel;

public class MessageViewModel : ReactiveObject
{
    private static readonly string _warnImageName = "Icons/warning-alert.ico";

    private readonly int _lineHeight = 16;
    private int _topMargin = 54;
    private MessageDialog _view;

    private Bitmap wI;
    internal Bitmap WarnImage
    {
        get { return wI; }
        private set
        {
            this.RaiseAndSetIfChanged( ref wI, value, nameof( WarnImage ) );
        }
    }

    private string _message;
    internal string Message
    {
        get { return _message; }
        set
        {
            this.RaiseAndSetIfChanged( ref _message, value, nameof( Message ) );
        }
    }


    public MessageViewModel()
    {
        string warningIconPath = ListerApp.ResourceFolderName + _warnImageName;
    }


    internal void PassView(MessageDialog view)
    {
        _view = view;
    }


    internal void Close()
    {
        _view.Shut();
    }
}
﻿using ReactiveUI;
using Lister.ViewModels.BadgeVM;

namespace View.EditionWindow.ViewModel;

public partial class BadgeEditorViewModel : ReactiveObject
{
    public readonly double _scalabilityCoefficient = 1.25;
    private short _scalabilityDepth = 2;
    private readonly short _maxDepth = 5;
    private readonly short _minDepth = 0;

    private double _zoomDegree = 100;
    private string procentSymbol = "%";

    private bool _zoomerIsEnable;
    internal bool ZoommerIsEnable
    {
        get { return _zoomerIsEnable; }
        private set
        {
            if ( value )
            {
                FocusedFontSizeColor = _focusedFontSizeColor;
                FocusedFontSizeBorderColor = _focusedFontSizeBorderColor;
            }
            else 
            {
                FocusedFontSizeColor = _releasedFontSizeColor;
                FocusedFontSizeBorderColor = _releasedFontSizeBorderColor;
            }

            this.RaiseAndSetIfChanged (ref _zoomerIsEnable, value, nameof (ZoommerIsEnable));
        }
    }

    private bool _zoomOnIsEnable;
    internal bool ZoomOnIsEnable
    {
        set
        {
            this.RaiseAndSetIfChanged (ref _zoomOnIsEnable, value, nameof (ZoomOnIsEnable));
        }
        get
        {
            return _zoomOnIsEnable;
        }
    }

    private bool _zoomOutIsEnable;
    internal bool ZoomOutIsEnable
    {
        set
        {
            this.RaiseAndSetIfChanged (ref _zoomOutIsEnable, value, nameof (ZoomOutIsEnable));
        }
        get
        {
            return _zoomOutIsEnable;
        }
    }

    private string _zoomDegreeInView;
    internal string ZoomDegreeInView
    {
        get { return _zoomDegreeInView; }
        private set
        {
            this.RaiseAndSetIfChanged (ref _zoomDegreeInView, value, nameof (ZoomDegreeInView));
        }
    }


    private void SetUpZoommer ( )
    {
        ZoomOnIsEnable = true;
        ZoomOutIsEnable = true;
        _zoomDegree *= _scalabilityCoefficient;
        _zoomDegree *= _scalabilityCoefficient;
        ZoomDegreeInView = Math.Round(_zoomDegree).ToString () + " " + procentSymbol;
    }


    internal void ZoomOn ()
    {
        if ( BeingProcessedBadge == null )
        {
            return;
        }

        if ( _scalabilityDepth < _maxDepth )
        {

            BeingProcessedBadge.ZoomOn (_scalabilityCoefficient);
            _scalabilityDepth++;

            _zoomDegree *= _scalabilityCoefficient;
            _scale *= _scalabilityCoefficient;
            short zDegree = ( short ) _zoomDegree;
            ZoomDegreeInView = zDegree.ToString () + " " + procentSymbol;
        }

        if ( _scalabilityDepth == _maxDepth )
        {
            ZoomOnIsEnable = false;
        }

        if ( ! ZoomOutIsEnable )
        {
            ZoomOutIsEnable = true;
        }
    }


    internal void ZoomOut ()
    {
        if ( BeingProcessedBadge == null ) 
        {
            return;
        }

        if ( _scalabilityDepth > _minDepth )
        {
            BeingProcessedBadge.ZoomOut (_scalabilityCoefficient);
            _scalabilityDepth--;

            _zoomDegree /= _scalabilityCoefficient;
            _scale /= _scalabilityCoefficient;
            short zDegree = ( short ) _zoomDegree;
            ZoomDegreeInView = zDegree.ToString () + " " + procentSymbol;
        }

        if ( _scalabilityDepth == _minDepth )
        {
            ZoomOutIsEnable = false;
        }

        if ( ! ZoomOnIsEnable )
        {
            ZoomOnIsEnable = true;
        }
    }


    #region Scale

    //private void SetStandardScale ( BadgeViewModel beingPrecessed )
    //{
    //    if ( beingPrecessed.Scale != 1 )
    //    {
    //        beingPrecessed.ZoomOut (beingPrecessed.Scale);
    //    }
    //}


    private void SetOriginalScale ( BadgeViewModel beingPrecessed, double scale )
    {
        if ( scale != 1 )
        {
            beingPrecessed.ZoomOn (scale);
        }
    }


    private void SetToCorrectScale ( BadgeViewModel processable )
    {
        if ( processable.Scale != _scale )
        {
            if ( processable.Scale != 1 )
            {
                processable.ZoomOut (processable.Scale);
            }

            processable.ZoomOn (_scale);
        }
    }
    #endregion
}


public delegate void BackingToMainViewHandler ( );
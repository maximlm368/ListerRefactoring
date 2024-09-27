using Avalonia.Controls;
using Avalonia.Input;
using Avalonia;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ContentAssembler;
using static System.Net.Mime.MediaTypeNames;
using System.Collections.ObjectModel;
using Avalonia.Media;
using System.Globalization;
using System.Reflection.Metadata;
using ExtentionsAndAuxiliary;
using Microsoft.VisualBasic;
using Avalonia.Media.Imaging;
using Lister.Extentions;
using System.Linq.Expressions;
using Lister.Views;
using System.Windows.Input;
using Avalonia.Platform.Storage;
using System.Reactive.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Avalonia.Threading;
using System.Reflection;

namespace Lister.ViewModels
{
    public partial class BadgeEditorViewModel : ViewModelBase
    {
        public readonly double _scalabilityCoefficient = 1.25;
        private short _scalabilityDepth = 2;
        private readonly short _maxDepth = 5;
        private readonly short _minDepth = 0;

        private double _zoomDegree = 100;
        private string procentSymbol = "%";

        private bool zE;
        internal bool ZoommerIsEnable
        {
            get { return zE; }
            private set
            {
                this.RaiseAndSetIfChanged (ref zE, value, nameof (ZoommerIsEnable));
            }
        }

        private bool zonE;
        internal bool ZoomOnIsEnable
        {
            set
            {
                this.RaiseAndSetIfChanged (ref zonE, value, nameof (ZoomOnIsEnable));
            }
            get
            {
                return zonE;
            }
        }

        private bool zoutE;
        internal bool ZoomOutIsEnable
        {
            set
            {
                this.RaiseAndSetIfChanged (ref zoutE, value, nameof (ZoomOutIsEnable));
            }
            get
            {
                return zoutE;
            }
        }

        private string zoomDV;
        internal string ZoomDegreeInView
        {
            get { return zoomDV; }
            private set
            {
                this.RaiseAndSetIfChanged (ref zoomDV, value, nameof (ZoomDegreeInView));
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
            if ( _scalabilityDepth < _maxDepth )
            {

                BeingProcessedBadge.ZoomOn (_scalabilityCoefficient, false);
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
            if ( _scalabilityDepth > _minDepth )
            {
                BeingProcessedBadge.ZoomOut (_scalabilityCoefficient, false);
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

        private void SetStandardScale ( BadgeViewModel beingPrecessed )
        {
            if ( beingPrecessed.Scale != 1 )
            {
                beingPrecessed.ZoomOut (beingPrecessed.Scale, true);
            }
        }


        private void SetOriginalScale ( BadgeViewModel beingPrecessed, double scale )
        {
            if ( scale != 1 )
            {
                beingPrecessed.ZoomOn (scale, false);
            }
        }


        private void SetToCorrectScale ( BadgeViewModel processable )
        {
            if ( processable.Scale != _scale )
            {
                if ( processable.Scale != 1 )
                {
                    processable.ZoomOut (processable.Scale, true);
                }
                processable.ZoomOn (_scale, false);
            }
        }
        #endregion
    }


    public delegate void BackingToMainViewHandler ( );
}
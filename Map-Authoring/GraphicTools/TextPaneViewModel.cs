using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Extensions;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;


namespace GraphicTools
{
    internal class TextPaneViewModel : DockPane
    {

        private const string _dockPaneID = "GraphicTools_TextPane";

        protected TextPaneViewModel()
        
        {

            _pointTextCmd = new RelayCommand(() => Module1.Current.RunPointTextTool(), () => true);
            _polygonTextCmd = new RelayCommand(() => Module1.Current.RunPolygonTextTool(), () => true);
            _applyTextCmd = new RelayCommand(() => Module1.Current.RunApplyTextTool(), () => true);

        }

        #region Commands

        private RelayCommand _pointTextCmd;
        public ICommand PointTextCmd
        {
            get
            {
                return _pointTextCmd;
            }
        }

        private RelayCommand _polygonTextCmd;
        public ICommand PolygonTextCmd
        {
            get
            {
                return _polygonTextCmd;
            }
        }


        private RelayCommand _applyTextCmd;
        public ICommand ApplyTextCmd
        {
            get
            {
                return _applyTextCmd;
            }
        }

        #endregion

        #region Properties

        // Textbox value
        public string _txtBoxDoc = "Default Text"; 
        public string TxtBoxDoc
        {
            get
            {
                return _txtBoxDoc;
            }
            set
            {
                _txtBoxDoc = value;
                NotifyPropertyChanged();
            }
        }

        // Risk / Vulnerability textbox value
        public string _queryTxtBoxDoc;
        public string QueryTxtBoxDoc
        {
            get
            {
                return _queryTxtBoxDoc;
            }
            set
            {
                _queryTxtBoxDoc = value;
                NotifyPropertyChanged();
            }
        }

        #endregion



        /// <summary>
        /// Show the DockPane.
        /// </summary>
        internal static void Show()
        {
            DockPane pane = FrameworkApplication.DockPaneManager.Find(_dockPaneID);
            if (pane == null)
                return;
            pane.Activate();
        }

        protected override void OnShow(bool isVisible)
        {
            Module1.Current.blnDockpaneOpenStatus = true;
        }
        protected override void OnHidden()
        {
            Module1.Current.blnDockpaneOpenStatus = false;
        }

    }

    /// <summary>
    /// Button implementation to show the DockPane.
    /// </summary>
    internal class TextPane_ShowButton : Button
    {
        protected override void OnClick()
        {
            TextPaneViewModel.Show();
        }

    }

}


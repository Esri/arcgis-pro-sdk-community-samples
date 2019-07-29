using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
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


namespace MapToolIdentifyWithDockpane
{
    internal class MapToolIdentifyDockpaneViewModel : DockPane
    {
        private const string _dockPaneID = "MapToolIdentifyWithDockpane_Dockpane";
        private ObservableCollection<string> _listOfFeatures = new ObservableCollection<string>();

        /// <summary>
        /// used to lock collections for use by multiple threads
        /// </summary>
        public readonly object LockCollections = new object();

        #region CTor

        protected MapToolIdentifyDockpaneViewModel()
        {
            BindingOperations.EnableCollectionSynchronization(_listOfFeatures, LockCollections);
            // register self with Module class
            Module1.MapToolIdentifyDockpaneVM = this;
        }

        #endregion CTor

        #region Methods for external updates of data

        /// <summary>
        /// Clear the list of features from any thread
        /// </summary>
        internal void ClearListOfFeatures ()
        {
            lock (_listOfFeatures)
            {
                ProApp.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    ListOfFeatures.Clear();
                }));
            }
        }

        /// <summary>
        /// Update the list of features 
        /// </summary>
        internal void AddToListOfFeatures(string addItem)
        {
            lock (_listOfFeatures)
            {
                ProApp.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    ListOfFeatures.Add(addItem);
                }));
            }
        }
        #endregion 

        #region Public Properties

        /// <summary>
        /// List of the current active map's identified features
        /// </summary>
        public ObservableCollection<string> ListOfFeatures
        {
            get { return _listOfFeatures; }
        }

        /// <summary>
        /// Text shown near the top of the DockPane.
        /// </summary>
        private string _heading = "Identify Results";
        public string Heading
        {
            get { return _heading; }
            set
            {
                SetProperty(ref _heading, value, () => Heading);
            }
        }

        #endregion Public Properties

        #region Static methods

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

        #endregion Static methods
    }

    /// <summary>
    /// Button implementation to show the DockPane.
    /// </summary>
    internal class MapToolIdentifyDockpane_ShowButton : Button
    {
        protected override void OnClick()
        {
            MapToolIdentifyDockpaneViewModel.Show();
        }
    }
}

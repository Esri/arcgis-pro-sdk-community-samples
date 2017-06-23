/*

   Copyright 2017 Esri

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

   See the License for the specific language governing permissions and
   limitations under the License.

*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using System.Xml.Linq;
using ArcGIS.Core.CIM;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Mapping;

namespace CustomPaneWithContents
{
    internal class CustomPaneViewModel : ViewStatePane, IContentsProvider, IContentsControl
    {
        private const string _viewPaneID = "CustomPaneWithContents_CustomPane";
        DispatcherTimer timer;
        private string _currentTime = "";
        private string _paneName = "";
        private static OperationManager _operationsManager = null;


        /// <summary>
        /// Consume the passed in CIMView. Call the base constructor to wire up the CIMView.
        /// </summary>
        public CustomPaneViewModel(CIMView view)
          : base(view)
        {

            timer = new DispatcherTimer(new TimeSpan(0, 0, 0, 0, 50), DispatcherPriority.Background,
                timer_Tick, Dispatcher.CurrentDispatcher);
            timer.IsEnabled = true;
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            _currentTime = DateTime.Now.ToString("HH:mm:ss tt");
            NotifyPropertyChanged("CurrentTime");
        }

        /// <summary>
        /// Create a new instance of the pane.
        /// </summary>
        internal static CustomPaneViewModel Create()
        {
            var view = new CIMGenericView();
            view.ViewType = _viewPaneID;
            view.ViewXML = SaveViewXML("Custom Pane", DateTime.Now.ToString("HH:mm:ss tt"));
            return FrameworkApplication.Panes.Create(_viewPaneID, new object[] { view }) as CustomPaneViewModel;
        }

        #region View Model properties

        public string PaneName => _paneName;

        public string UserName => $"\"{System.Security.Principal.WindowsIdentity.GetCurrent().Name}\"";

        public string CurrentTime => _currentTime;

        /// <summary>
        /// Note: Provide your own operations manager to maintain your own associated
        /// undo/redo stack on the UI
        /// </summary>
        public override OperationManager OperationManager
        {
            get
            {
                if (_operationsManager == null)
                    _operationsManager = new OperationManager();
                return _operationsManager;
            }
        }

        #endregion

        #region Pane Overrides

        /// <summary>
        /// Must be overridden in child classes used to persist the state of the view to the CIM.
        /// </summary>
        public override CIMView ViewState
        {
            get
            {
                _cimView.InstanceID = (int)InstanceID;
                _cimView.ViewXML = SaveViewXML(this.PaneName, DateTime.Now.ToString("HH:mm:ss tt"));
                return _cimView;
            }
        }

        /// <summary>
        /// Called when the pane is initialized.
        /// </summary>
        protected async override Task InitializeAsync()
        {
            var content = ReadViewXml(_cimView.ViewXML);
            _paneName = content.Item1;
            await base.InitializeAsync();
        }

        /// <summary>
        /// Called when the pane is uninitialized.
        /// </summary>
        protected async override Task UninitializeAsync()
        {
            await base.UninitializeAsync();
        }

        #endregion Pane Overrides

        internal static string SaveViewXML(string paneName, string timeStamp)
        {
            //We can store anything we want in our CIM xml.
            //make a simple xml structure to store the pane name
            XDocument xml = new XDocument(
                new XElement("Root",
                    new XElement("CustomPaneName", paneName)
                    )
                );
            xml.Element("Root").Add(new XElement("LastChanged", timeStamp));
            return xml.ToString(SaveOptions.DisableFormatting);
        }

        internal static Tuple<string, string> ReadViewXml(string xml)
        {
            XDocument doc = XDocument.Parse(xml);
            XElement root = doc.Root;
            var paneName = root.Element("CustomPaneName");
            var timeStamp = root.Element("LastChanged");
            return new Tuple<string, string>(paneName.Value, timeStamp.Value);
        }

        /// <summary>
        /// Indicates whether the provider's contents are ready to show. Used internally. External providers should return true.
        /// </summary>
        public bool ContentsReady => true;

        #region Snippet Create a Contents Control for a Custom View
        /// <summary>
        /// Create a contents control to be shown as the TOC when the View is activated.
        /// </summary>
        /// <remarks>
        /// The contents will be shown in the Contents Dock Pane when the IContentsProvider (i.e. your Pane) is the active pane
        /// </remarks>
        private Contents _contents;

        public Contents Contents
        {
            get
            {
                if (_contents == null)
                {
                    //This is your user control to be used as contents
                    FrameworkElement contentsControl = new CustomPaneContentsControl()
                    {
                        //Vanilla MVVM here
                        DataContext = this//This can be any custom view model
                    };

                    //This is the Pro Framework contents control wrapper
                    _contents = new Contents()
                    {
                        //The view is your user control
                        ContentsView = contentsControl,
                        ContentsViewModel = this,//This is your pane view model
                        OperationManager = this.OperationManager//optional
                    };
                }
                return _contents;
            }
        }

        #endregion

        /// <summary>
        /// Gets whether the control is in a read-only state.
        /// </summary>
        public bool ReadOnly
        {
            get
            {
                return false;
            }
            set
            {

            }
        }

        /// <summary>
        /// Gets the default caption to be used for the Contents dock pane.
        /// </summary>
        /// <remarks>
        /// The caption override is only shown when your content is
        ///             active
        /// </remarks>
        public string CaptionOverride => "Custom Content";
    }

    /// <summary>
    /// Button implementation to create a new instance of the pane and activate it.
    /// </summary>
    internal class CustomPane_OpenButton : Button
    {
        protected override void OnClick()
        {
            CustomPaneViewModel.Create();
        }
    }
}

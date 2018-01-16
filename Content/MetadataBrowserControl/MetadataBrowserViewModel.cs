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
using System.Text;
using System.Threading.Tasks;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using System.Windows.Input;
using ArcGIS.Desktop.Core;
using System.Windows;
using Microsoft.Win32;
using System.IO;
using System.Collections.ObjectModel;

namespace MetadataBrowserControl
{
    internal class MetadataBrowserViewModel : DockPane
    {
        private const string _dockPaneID = "MetadataBrowserControl_Dockpane1";

        protected MetadataBrowserViewModel() { }

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

        protected override Task InitializeAsync()
        {
            DockpaneVisible = Visibility.Collapsed; //Visibility controlled from the module                
            XSLFiles.Clear();
            XSLFiles.Add(new XSLFile(AppDomain.CurrentDomain.BaseDirectory.Replace("bin\\", "Resources\\Metadata\\Stylesheets\\XML.xsl")));
            XSLFiles.Add(new XSLFile("Browse for stylesheet"));
            this.SelectedXSL = XSLFiles[0];
            return base.InitializeAsync();
        }
        #region public properties
        private string _heading = "Select stylesheet to render Item Metadata";
        public string Heading
        {
            get { return _heading; }
            set
            {
                SetProperty(ref _heading, value, () => Heading);
            }
        }        
        /// <summary>
        /// The string to display in the browser control - represents the transformed XML of the project item's metadata
        /// </summary>
        private string _webPage = @"<html><body>Select a project item to view its Metadata. You cannot view the metadata for some items such as system styles.</body></html>";
        public string WebPage
        {
            get { 

                return _webPage;
            }
            set
            {
                SetProperty(ref _webPage, value, () => WebPage);
            }
        }
        private ItemMetadata _itemInfo = null;
        /// <summary>
        /// The selected project item's ItemMetadata
        /// </summary>
        public ItemMetadata ItemInformation
        {
            get { return _itemInfo; }
            set
            {
                SetProperty(ref _itemInfo, value, () => ItemInformation);
            }
        }

        /// <summary>
        /// Visibility of dockpane
        /// </summary>
        private Visibility _dockpaneVisible;
        public Visibility DockpaneVisible
        {
            get { return _dockpaneVisible; }
            set
            {
                SetProperty(ref _dockpaneVisible, value, () => DockpaneVisible);
            }
        }
        private ObservableCollection<XSLFile> _xslFiles = new ObservableCollection<XSLFile>();
        /// <summary>
        /// Collection of XSLFiles displayed in combo box
        /// </summary>
        public ObservableCollection<XSLFile> XSLFiles
        {
            get { return _xslFiles; }
            set {
                SetProperty(ref _xslFiles, value, () => XSLFiles);
            }
        }
        private XSLFile _selectedXSLFile;
        /// <summary>
        /// The selected XSLFile to apply the transform with
        /// </summary>
        public XSLFile SelectedXSL
        {
            get { return _selectedXSLFile; }
            set {
                if (!File.Exists(value.FileFullName)) // User picked browse
                {
                    if (_selectedXSLFile == null) //flag to check so we don't get into the setter when XSLFile is added to collection
                        return;
                    XSLFile browsedFile = BrowseFileName();
                    if (browsedFile == null) //user cancelled
                        return;
                    _selectedXSLFile = null; //set to null so setter is not called when user Browses.
                    XSLFiles.Insert(0, browsedFile); //insert new file to start of collection.
                    SetProperty(ref _selectedXSLFile, browsedFile, () => SelectedXSL);
                }
                else
                    SetProperty(ref _selectedXSLFile, value, () => SelectedXSL);
                ApplyTransform();
            }
        }
        #endregion

        /// <summary>
        /// Applies the transform to view the metadata in a browser control
        /// </summary>

        public void ApplyTransform()
        {
            if (SelectedXSL == null)
                return;
            if (this.ItemInformation == null)
                return;

            if (!string.IsNullOrEmpty(this.ItemInformation.XML))
            {
                WebPage = ItemMetadata.TransformXML(ItemInformation.XML, SelectedXSL.FileFullName);
            }
        }
        //Browse for a new stylesheet
        private XSLFile BrowseFileName()
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            dlg.DefaultExt = ".xslt";
            dlg.Filter = ("Stylesheets (*.xslt;*xsl)|*.xslt;*xsl");
            if (dlg.ShowDialog() == true)
            {
                var browsedXSLFile = new XSLFile(dlg.FileName);
                return browsedXSLFile;               
            }
            else //user cancelled
            {
                return null;
            }
        }
    }

    /// <summary>
    /// Button implementation to show the DockPane.
    /// </summary>
    internal class Dockpane1_ShowButton : Button
    {
        protected override void OnClick()
        {
            MetadataBrowserViewModel.Show();
        }
    }
}

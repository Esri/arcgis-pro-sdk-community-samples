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
using System.Xml.Linq;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows;
using System.Windows.Resources;

namespace ControlStyles.Colors
{
    internal class ArcGISProColorsDockpaneViewModel : DockPane
    {
        private const string _dockPaneID = "ControlStyles_Colors_ArcGISProColorsDockpane";
        private static object _lockObject = new object();

        protected ArcGISProColorsDockpaneViewModel() 
        {
            
            GetColors();
            SelectedColor = _proColors[14];
        //    LoadEsriColorXDoc();
            _copyColorKey = new RelayCommand(() => Utils.CopyStringToClipboard(SelectedColorKey));
            _copyReferenceDictionary = new RelayCommand(() => Utils.CopyStringToClipboard(ReferenceDictionary));
        }

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
        # region properties

        private List<ColorEntry> _proColors = null;
        
        /// <summary>
        /// List of the ArcGIS Pro defined Colors.
        /// </summary>

        public List<ColorEntry> ProColors
        {
            get { return _proColors; }
        }

        private ColorEntry _selectedColor;
        public ColorEntry SelectedColor
        {
            get { return _selectedColor; }
            set
            {
                _selectedColor = value;
                _selectedColorKey = string.Format("\"{{DynamicResource {0}}}\"", _selectedColor);
                SetProperty(ref _selectedColor, value, () => SelectedColor);
            }
        }
        private string _selectedColorKey;
        public string SelectedColorKey
        {
            get { return _selectedColorKey; }
            set
            {
                _selectedColorKey = value;
                SetProperty(ref _selectedColorKey, value, () => SelectedColorKey);
            }
        }

        private string _refDict;
        public string ReferenceDictionary
        {
            get
            {
                _refDict = string.Format("<ResourceDictionary.MergedDictionaries><extensions:DesignOnlyResourceDictionary Source=\"pack://application:,,,/ArcGIS.Desktop.Framework;component\\Themes\\Default.xaml\"/></ResourceDictionary.MergedDictionaries>");
                return _refDict;
            }           
        }  

        private RelayCommand _copyColorKey;
        private RelayCommand _copyReferenceDictionary;

        #endregion

        #region methods

        private void GetColors()
        {
            ResourceDictionary myResourceDictionary = new ResourceDictionary();
            myResourceDictionary.Source = new Uri("ArcGIS.Desktop.Framework;component\\Resources\\BasicResources.xaml",
                    UriKind.RelativeOrAbsolute);
            _proColors = new List<ColorEntry>();
            foreach (string key in myResourceDictionary.Keys)
            {
                object val = myResourceDictionary[key];
                if (val.ToString().Contains("#") && key.ToString().Contains("Esri"))
                    _proColors.Add(new ColorEntry(key.ToString(), val.ToString()));
                else
                   continue;
                
            }

            _proColors.Sort();
            this.NotifyPropertyChanged(new PropertyChangedEventArgs("Colors"));
        }

        private static IEnumerable<XElement> GetElements(XDocument projectFile, XNamespace NS, string elname)
        {
            IEnumerable<XElement> allElements = projectFile.Descendants(NS + elname);
            return allElements;
        }

        /// <summary>
        /// Represents the Copy to Clipboard command
        /// </summary>
        public ICommand CopyColorKeyCmd
        {
            get { return _copyColorKey; }
        }

        /// <summary>
        /// Represents the Copy to Clipboard command for Reference dictionary
        /// </summary>
        public ICommand CopyRefDictCmd
        {
            get { return _copyReferenceDictionary; }
        }

        #endregion

    }

    /// <summary>
    /// Button implementation to show the DockPane.
    /// </summary>
    internal class ArcGISProColorsDockpane_ShowButton : Button
    {
        protected override void OnClick()
        {
            ArcGISProColorsDockpaneViewModel.Show();
        }
    }
}

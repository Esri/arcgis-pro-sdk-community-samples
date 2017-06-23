// Copyright 2017 Esri

//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at

//       http://www.apache.org/licenses/LICENSE-2.0

//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.


using System.Collections.Generic;
using ArcGIS.Desktop.Framework.Controls;
using System.Xml.Linq;
using System.Windows.Controls;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Desktop.Editing.Attributes;

namespace InspectorTool
{
    internal class AttributeControlViewModel : EmbeddableControl
    {
        private EmbeddableControl _inspectorViewModel = null;
        private UserControl _inspectorView = null;
        private Dictionary<MapMember, List<long>> _selection = null;
        private Inspector _featureInspector = null;

        public AttributeControlViewModel(XElement options, bool canChangeOptions) : base(options, canChangeOptions)
        {
            // create a new instance for the inspector
            _featureInspector = new Inspector();
            // create an embeddable control from the inspector class to display on the pane
            var icontrol = _featureInspector.CreateEmbeddableControl();

            // get view and viewmodel from the inspector
            InspectorView = icontrol.Item2;
            InspectorViewModel = icontrol.Item1;
        }

        /// <summary>
        /// Property containing an instance for the inspector.
        /// </summary>
        public Inspector AttributeInspector
        {
            get
            {
                return _featureInspector;
            }
        }

        /// <summary>
        /// Access to the view model of the inspector
        /// </summary>
        public EmbeddableControl InspectorViewModel
        {
            get { return _inspectorViewModel; }
            set
            {
                if (value != null)
                {
                    _inspectorViewModel = value;
                    _inspectorViewModel.OpenAsync();

                }
                else if (_inspectorViewModel != null)
                {
                    _inspectorViewModel.CloseAsync();
                    _inspectorViewModel = value;
                }
                NotifyPropertyChanged(() => InspectorViewModel);
            }
        }

        /// <summary>
        /// Dictionary holding the selected features in the map to populate the tree view for 
        /// layers and respective selected features.
        /// </summary>
        public Dictionary<MapMember, List<long>> SelectedMapFeatures
        {
            get
            {
                return _selection;
            }
            set
            {
                SetProperty(ref _selection, value, () => SelectedMapFeatures);
            }
        }

        /// <summary>
        /// Property for the inspector UI.
        /// </summary>
        public UserControl InspectorView
        {
            get { return _inspectorView; }
            set { SetProperty(ref _inspectorView, value, () => InspectorView); }
        }
    }
}

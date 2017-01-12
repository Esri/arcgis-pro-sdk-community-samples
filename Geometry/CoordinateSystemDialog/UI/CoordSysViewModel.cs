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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Threading;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Desktop.Mapping.Controls;

namespace CoordinateSystemAddin.UI {
    
    internal class CoordSysViewModel : INotifyPropertyChanged {
        private bool _showVCS = false;
        private SpatialReference _sr;
        private CoordinateSystemsControlProperties _props = null;

        public CoordSysViewModel() {
            UpdateCoordinateControlProperties();
        }

        public string SelectedCoordinateSystemName => _sr != null ? _sr.Name : "";

        public SpatialReference SelectedSpatialReference
        {
            get
            {
                return _sr;
            }
            set
            {
                _sr = value;
                NotifyPropertyChanged();
            }
        }

        public bool ShowVCS
        {
            get
            {
                return _showVCS;
            }
            set
            {
                if (_showVCS != value) {
                    _showVCS = value;
                    UpdateCoordinateControlProperties();
                    NotifyPropertyChanged();
                }
            }
        }

        public CoordinateSystemsControlProperties ControlProperties
        {
            get
            {
                return _props;
            }
            set
            {
                _props = value;
                NotifyPropertyChanged();
            }
        }

        private void UpdateCoordinateControlProperties() {
            var map = MapView.Active?.Map;
            var props = new CoordinateSystemsControlProperties() {
                Map = map,
                SpatialReference = this._sr,
                ShowVerticalCoordinateSystems = this.ShowVCS
            };
            this.ControlProperties = props;
        }

        public event PropertyChangedEventHandler PropertyChanged = delegate {};

        private void NotifyPropertyChanged([CallerMemberName] string propName = "") {
            PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }
    }
}

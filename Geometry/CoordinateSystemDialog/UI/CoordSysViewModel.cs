/*

   Copyright 2026 Esri

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       https://www.apache.org/licenses/LICENSE-2.0

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
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Desktop.Mapping.Controls;

namespace CoordinateSystemAddin.UI
{

  internal class CoordSysViewModel : PropertyChangedBase
  {
    private bool _showVCS = false;
    private SpatialReference _sr;
    private CoordinateSystemsControlProperties _props = null;

    private bool _isVCSAvailable = false;

    public CoordSysViewModel()
    {
      UpdateCoordinateControlProperties();
    }

    public string SelectedCoordinateSystemName => _sr != null ? _sr.Name : "";

    public SpatialReference SelectedSpatialReference
    {
      get => _sr;
      set
      {
        SetProperty(ref _sr, value);
        IsVCSAvailable = _sr != null; 
        if (!IsVCSAvailable)
          ShowVCS = false; // set ShowVCS to false if VCS is not available
        // Add this condition to make sure the selected SpatialReference supports a vertical Coordinate system && _sr.HasVcs;
      }
    }

    public bool IsVCSAvailable
    {
      get => _isVCSAvailable;
      set
      {
        SetProperty(ref _isVCSAvailable, value);
        UpdateCoordinateControlProperties();
      }
    }

    public bool ShowVCS
    {
      get => _showVCS;
      set 
      {
        SetProperty(ref _showVCS, value);
        UpdateCoordinateControlProperties();
      }
    }

    public CoordinateSystemsControlProperties ControlProperties
    {
      get => _props;
      set => SetProperty(ref _props, value);
    }

    private void UpdateCoordinateControlProperties()
    {
      var map = MapView.Active?.Map;
      var props = new CoordinateSystemsControlProperties()
      {
        Map = map,
        SpatialReference = this._sr,
        ShowVerticalCoordinateSystems = this.ShowVCS
      };
      this.ControlProperties = props;
    }

  }
}

/*

   Copyright 2023 Esri

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
using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Extensions;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Controls;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Xml.Linq;


namespace MapToolWithEmbeddableControl
{
  internal class EmbeddableControlViewModel : EmbeddableControl
  {
    public EmbeddableControlViewModel(XElement options, bool canChangeOptions) : base(options, canChangeOptions) 
    { }


    private string _XCoord = "-159.529";
    public string XCoord
    {
      get => _XCoord;
      set => SetProperty(ref _XCoord, value);
    }
    private string _YCoord = "21.915";
    public string YCoord
    {
      get => _YCoord;
      set => SetProperty(ref _YCoord, value);
    }

    public ICommand CmdSearch
    {
      get => new RelayCommand(() =>
      {
        // Search for the point and display the point on the Map
        if (Module1.MapToolWithEmbeddableControl == null) return;
        if (!double.TryParse (XCoord, out var x)
           || !double.TryParse(YCoord, out var y))
        {
          MessageBox.Show("Invalid X/Y coordinates use WGS 1984");
          return;
        }
        var point = MapPointBuilderEx.CreateMapPoint(x, y, SpatialReferences.WGS84);
        Module1.MapToolWithEmbeddableControl.ShowPoint(point);
        MapView.Active?.ZoomToAsync(point);
      });
    }

    /// <summary>
    /// Text shown in the control.
    /// </summary>
    private string _text = "Search WGS 1984 coords";
    public string Text
    {
      get => _text;
      set => SetProperty(ref _text, value);
    }
  }
}

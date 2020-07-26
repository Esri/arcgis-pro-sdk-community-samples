//   Copyright 2020 Esri
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at

//       http://www.apache.org/licenses/LICENSE-2.0

//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License. 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Events;
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

namespace GeometryControl
{
  internal class ModifyToolWithSketchProperties : MapTool
  {
    public ModifyToolWithSketchProperties()
    {
      IsSketchTool = true;
      SketchType = SketchGeometryType.Line;
      SketchOutputMode = SketchOutputMode.Map;

      this.OverlayControlID = "GeometryControl_SketchPropertiesView";
    }

    private SubscriptionToken _TOCSelectionChangedEventToken;

    protected override Task OnToolActivateAsync(bool active)
    {
      // set sketch layer on activate
      SetSketchLayer();

      // subscribe to TOC selection changed event
      _TOCSelectionChangedEventToken = ArcGIS.Desktop.Mapping.Events.TOCSelectionChangedEvent.Subscribe(OnTOCSelectionChanged);

      return base.OnToolActivateAsync(active);
    }

    protected override Task OnToolDeactivateAsync(bool hasMapViewChanged)
    {
      // unsubscribe from TOC selection changed event
      if (_TOCSelectionChangedEventToken != null)
        ArcGIS.Desktop.Mapping.Events.TOCSelectionChangedEvent.Unsubscribe(OnTOCSelectionChanged);
      _TOCSelectionChangedEventToken = null;

      return base.OnToolDeactivateAsync(hasMapViewChanged);
    }

    protected override Task<bool> OnSketchCompleteAsync(Geometry geometry)
    {     
      return base.OnSketchCompleteAsync(geometry);
    }

    // when selection on the TOC changes, update the SketchLayer
    private void OnTOCSelectionChanged(ArcGIS.Desktop.Mapping.Events.MapViewEventArgs args)
    {
      if (args == null)
        return;

      SetSketchLayer();
    }

    // sets the SketchLayer on the overlay control to be the first item selected in the TOC.
    //  The properties of the layer's feature class determines whether the GeometryControl displays Z, M values. 
    private void SetSketchLayer()
    {
      // get the overlay viewmodel
      var vm = OverlayEmbeddableControl as SketchPropertiesViewModel;
      if (vm == null)
        return;

      // get the first layer selected in the toc
      var selectedLayer = MapView.Active.GetSelectedLayers().FirstOrDefault();
      // set the sketch layer property (in order to obtain Zs, Ms as appropriate) 
      if (selectedLayer is BasicFeatureLayer sbf)
        vm.SketchLayer = sbf;
      else
        vm.SketchLayer = null;
    }
  }
}


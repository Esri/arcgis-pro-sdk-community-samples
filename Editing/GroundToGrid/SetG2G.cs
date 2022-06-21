//   Copyright 2019 Esri
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at

//       https://www.apache.org/licenses/LICENSE-2.0

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

namespace SetGroundToGrid
{
  internal class SetG2G : Button
  {
    protected override async void OnClick()
    {
      //Get the active map view.
      var mapView = MapView.Active;
      if (mapView?.Map == null)
        return;

      var cim_g2g = await mapView.Map.GetGroundToGridCorrection();

      if (cim_g2g == null)
        cim_g2g = new CIMGroundToGridCorrection(); // CIM for ground to grid is null for new maps, so initialize it for the first time here.

      bool bInitialIsCorrecting = cim_g2g.Enabled;

      bool bInitialUsingDirectionOffset = cim_g2g.UseDirection;
      double dInitialDirectionOffsetCorrection = cim_g2g.Direction; 
      //NOTE: cim_g2g.GetDirectionOffset(); //this application does not use the helper extension methods
      //the helper extension methods are best used when READING the settings to avoid having to test for IsEnabled, && UseScale && etc.
      //the helper extension methods simplify the code and tests for those for you internally.

      bool bInitialUsingDistanceFactor = cim_g2g.UseScale;
      bool bInitialUsingConstantScaleFactor = cim_g2g.ScaleType==GroundToGridScaleType.ConstantFactor; //cim_g2g.UsingConstantScaleFactor();

      double dInitialScaleFactor = cim_g2g.ConstantScaleFactor;      
      
      //NOTE: cim_g2g.GetConstantScaleFactor(); //this application does not use the helper extension methods
      //the helper extension methods are best used when READING the settings to avoid having to test for IsEnabled, && UseScale && etc.
      //the helper extension methods simplify the code and tests for those for you internally.

      //changing all the G2G settings
      cim_g2g.Enabled = !bInitialIsCorrecting;
      cim_g2g.UseDirection = !bInitialUsingDirectionOffset;
      cim_g2g.Direction = dInitialDirectionOffsetCorrection + 10; // store and set this in decimal degrees, irrespective of project unit settings

      cim_g2g.UseScale = !bInitialUsingDistanceFactor;
      cim_g2g.ScaleType = bInitialUsingConstantScaleFactor ? GroundToGridScaleType.ComputeUsingElevation : GroundToGridScaleType.ConstantFactor;

      cim_g2g.ConstantScaleFactor = dInitialScaleFactor * 2;

      await mapView.Map.SetGroundToGridCorrection(cim_g2g);

    }
  }
}

/*

   Copyright 2022 Esri

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
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using Renderer.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Renderer
{
  internal class DotDensityRenderer 
  {
    #region Snippet Dot Density renderer.
    /// <summary>
    /// Renders a polygon feature layer with Dot Density symbols to represent quantities.
    /// ![Dot Density renderer](http://Esri.github.io/arcgis-pro-sdk/images/Renderers/dotDensity-renderer.png)
    /// </summary>
    /// <remarks></remarks>
    /// <returns></returns>
    internal static Task DotDensityRendererAsync()
    {
      //Check feature layer name
      //Code works with the USDemographics feature layer available with the ArcGIS Pro SDK Sample data
      var featureLayer = MapView.Active.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>().FirstOrDefault(f => f.Name == "USDemographics");
      if (featureLayer == null)
      {
        MessageBox.Show("This renderer works with the USDemographics feature layer available with the ArcGIS Pro SDK Sample data", "Data missing");
        return Task.FromResult(0);
      }
      return QueuedTask.Run(() =>
      {
        //Define size of the dot to use for the renderer
        int dotSize = 3;
        //Check if the TOTPOP10 field exists
        int idxField = -1;
        using (var table = featureLayer.GetTable())
        {
          var def = table.GetDefinition();
          idxField = def.FindField("TOTPOP10");
        }
        // "TOTPOP10" field was not found
        if (idxField == -1)
          return;

        //array of fields to be represented in the renderer
        var valueFields = new List<string> { "TOTPOP10" };
        //Create the DotDensityRendererDefinition object
        var dotDensityDef = new DotDensityRendererDefinition(valueFields, SDKHelpers.GetColorRamp(),
                                                                          dotSize, 30000, "Dot", "people");
        //Create the renderer using the DotDensityRendererDefinition
        CIMDotDensityRenderer dotDensityRndr = (CIMDotDensityRenderer)featureLayer.CreateRenderer(dotDensityDef);

          //if you want to customize the dot symbol for the renderer, create a "DotDensitySymbol" which is an
          //Amalgamation of 3 symbol layers: CIMVectorMarker, CIMSolidFill and CIMSolidStroke
          //Define CIMVectorMarker layer          
          var cimMarker = SymbolFactory.Instance.ConstructMarker(ColorFactory.Instance.RedRGB, dotSize);
          var dotDensityMarker = cimMarker as CIMVectorMarker;
          //Definte the placement
          CIMMarkerPlacementInsidePolygon markerPlacement = new CIMMarkerPlacementInsidePolygon { Randomness = 100, GridType = PlacementGridType.RandomFixedQuantity, Clipping = PlacementClip.RemoveIfCenterOutsideBoundary };
          dotDensityMarker.MarkerPlacement = markerPlacement;

          //Define CIMSolidFill layer
          CIMSolidFill solidFill = new CIMSolidFill { Color = new CIMRGBColor { R = 249, G = 232, B = 189, Alpha = 50 } };

          //Define CIMSolidStroke
          CIMSolidStroke solidStroke = new CIMSolidStroke { Color = ColorFactory.Instance.GreyRGB, Width = .5 };

          //Create the amalgamated CIMPolygonSymbol that includes the 3 layers
          var dotDensitySymbol = new CIMPolygonSymbol
          {
            SymbolLayers = new CIMSymbolLayer[] { dotDensityMarker, solidFill, solidStroke }
          };

          //Apply the dotDensitySymbol to the CIMDotDenstityRenderer's DotDensitySymbol property.
          dotDensityRndr.DotDensitySymbol = dotDensitySymbol.MakeSymbolReference();

          //Apply the renderer to the polygon Feature Layer.
          featureLayer.SetRenderer(dotDensityRndr);       
      });
    }

    #endregion

  }
}

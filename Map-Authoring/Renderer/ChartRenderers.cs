/*

   Copyright 2020 Esri

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
using Renderer.Helpers;

namespace Renderer
{
  internal static class ChartRenderers
  {
    #region Snippet Pie chart renderer for a feature layer
    /// <summary>
    /// Renders a feature layer using Pie chart symbols to represent data
    /// </summary>
    /// <remarks>
    /// ![Pie chart renderer](http://Esri.github.io/arcgis-pro-sdk/images/Renderers/pie-chart.png)
    /// </remarks>
    /// <returns>
    /// </returns>
    internal static Task PieChartRendererAsync()
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
        //Fields to use for the pie chart slices
        var chartFields = new List<string>
        {
                "WHITE10",
                 "BLACK10",
                 "AMERIND10",
                 "ASIAN10",
                 "HISPPOP10",
                 "HAMERIND10",
                 "HWHITE10",
                 "HASIAN10",
                 "HPACIFIC10",
                 "HBLACK10",
                  "HOTHRACE10"
        };

        PieChartRendererDefinition pieChartRendererDefn = new PieChartRendererDefinition()
        {
          ChartFields = chartFields,
          ColorRamp = SDKHelpers.GetColorRamp(),
          SizeOption = PieChartSizeOptions.Field,
          FieldName = "BLACK10",
          FixedSize = 36.89,
          DisplayIn3D = true,
          ShowOutline = true,
          Orientation = PieChartOrientation.CounterClockwise,
        };
        //Creates a "Renderer"
        var pieChartRenderer = featureLayer.CreateRenderer(pieChartRendererDefn);

        //Sets the renderer to the feature layer
        featureLayer.SetRenderer(pieChartRenderer);
      });
    }
    #endregion
    #region Snippet Bar Chart Value renderer for a feature layer
    /// <summary>
    /// Renders a feature layer using Bar chart symbols to represent data
    /// </summary>
    /// <remarks>
    /// ![bar chart renderer](http://Esri.github.io/arcgis-pro-sdk/images/Renderers/bar-chart.png)
    /// </remarks>
    /// <returns></returns>
    internal static Task BarChartRendererAsync()
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
        var chartFields = new List<string>
            {
                "WHITE10",
                 "BLACK10",
                 "AMERIND10",
                 "ASIAN10",
                 "HISPPOP10",
                 "HAMERIND10",
                 "HWHITE10",
                 "HASIAN10",
                 "HPACIFIC10",
                 "HBLACK10",
                  "HOTHRACE10"
            };

          BarChartRendererDefinition barChartRendererDefn = new BarChartRendererDefinition()
          {
            ChartFields = chartFields,
            BarWidth = 12,
            BarSpacing = 1,
            MaximumBarLength = 65,
            Orientation = ChartOrientation.Vertical,
            DisplayIn3D = true,
            ShowAxes = true,
            ColorRamp = SDKHelpers.GetColorRamp()

          };
          //Creates a "Renderer"
          var barChartChartRenderer = featureLayer.CreateRenderer(barChartRendererDefn);

          //Sets the renderer to the feature layer
          featureLayer.SetRenderer(barChartChartRenderer);
      });
    }
    #endregion
    #region Snippet Stacked bar chart renderer for a feature layer
    /// <summary>
    /// Renders a feature layer using stacked bar chart symbols to represent data
    /// </summary>
    /// <remarks>
    /// ![stacked bar chart renderer](http://Esri.github.io/arcgis-pro-sdk/images/Renderers/stacked-bar-chart.png)
    /// </remarks>
    /// <returns></returns>
    internal static Task StackedBarChartRendererAsync()
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
        var chartFields = new List<string>
            {
                "WHITE10",
                 "BLACK10",
                 "AMERIND10",
                 "ASIAN10",
                 "HISPPOP10",
                 "HAMERIND10",
                 "HWHITE10",
                 "HASIAN10",
                 "HPACIFIC10",
                 "HBLACK10",
                  "HOTHRACE10"
            };
        
          StackedChartRendererDefinition barChartpieChartRendererDefn = new StackedChartRendererDefinition()
          {
            ChartFields = chartFields,
            SizeOption = StackChartSizeOptions.SumSelectedFields,
            Orientation = ChartOrientation.Horizontal,
            ShowOutline = true,
            DisplayIn3D = true,
            StackWidth = 8,
            StackLength = 25.87,
            ColorRamp = SDKHelpers.GetColorRamp()

          };
          //Creates a "Renderer"
          var stackedBarChartChartRenderer = featureLayer.CreateRenderer(barChartpieChartRendererDefn);

          //Sets the renderer to the feature layer
          featureLayer.SetRenderer(stackedBarChartChartRenderer);
        });
    }
    #endregion
  }
}

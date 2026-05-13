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
using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Core.Assistant;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Extensions;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Internal.Core.Assistant;
using ArcGIS.Desktop.KnowledgeGraph;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AILayoutFunction
{
  [Description("Create a layout using the current map")]
  internal class AILayoutFunctionClass : AIAssistantExtension
  {
    [AIAssistantFunction, Description("Create a default layout using the current map")]
    public static async Task<AIFunctionResult> CreateDefaultLayout(
          [Description("The optional name of the layout")] string layoutName = "Default Layout",
          [Description("If true, the layout will include a title")] bool hasTitle = true,
          [Description("If true, the layout will include a legend")] bool hasLegend = false,
          [Description("If true, the layout will include a scale bar")] bool hasScaleBar = false,
          [Description("If true, the layout will include a north arrow")] bool hasNorthArrow = true)
    {
      await CreateLayout(layoutName, hasTitle, hasLegend, hasScaleBar, hasNorthArrow);

      return new AIFunctionResult("Layout created and opened");
    }

    [AIAssistantFunction, Description("Create a layout with legend using the current map")]
    public static async Task<AIFunctionResult> CreateLayoutWithLegend(
          [Description("The optional name of the layout")] string layoutName = "Layout with legend",
          [Description("If true, the layout will include a title")] bool hasTitle = true,
          [Description("If true, the layout will include a scale bar")] bool hasScaleBar = false,
          [Description("If true, the layout will include a north arrow")] bool hasNorthArrow = true)
    {
      await CreateLayout(layoutName, hasTitle, true, hasScaleBar, hasNorthArrow);

      return new AIFunctionResult("Layout with legend created and opened");
    }
    private static async Task CreateLayout(string layoutName, bool hasTitle, bool hasLegend, bool hasScaleBar, bool hasNorthArrow)
    {
      //Get the active map view.
      var map = MapView.Active.Map;

      var layout = await QueuedTask.Run(() =>
      {
        var layout = LayoutFactory.Instance.CreateLayout(8.5, 11, LinearUnit.Inches);
        layout.SetName(layoutName);

        //Add a map frame
        var mf_env = EnvelopeBuilderEx.CreateEnvelope(0.77, 1.3, 7.7, 9.7);

        var mf = ElementFactory.Instance.CreateMapFrameElement(
      layout, mf_env, map, "Default Map", false);

        if (hasTitle)
        {
          //Add a title
          var title_sym = SymbolFactory.Instance.ConstructTextSymbol(
            ColorFactory.Instance.BlackRGB, 36, "AvenirNext LT Pro Medium", "Italic");
          var title_text = new Coordinate2D(0.77, 9.7);

          ElementFactory.Instance.CreateTextGraphicElement(
             layout, TextType.PointText, title_text.ToMapPoint(), title_sym,
             layoutName, "Title Text1", false);
        }

        if (hasNorthArrow)
        {
          //Add a north arrow
          var na_frame = EnvelopeBuilderEx.CreateEnvelope(7.1, 1.3, 7.7, 1.9);

          var north_arrow_info = new NorthArrowInfo()
          {
            MapFrameName = mf.Name
          };

          ElementFactory.Instance.CreateMapSurroundElement(
              layout, na_frame, north_arrow_info, "", false,
              new ElementInfo() { Anchor = Anchor.BottomRightCorner });
        }

        if (hasScaleBar)
        {
          //Add a scale bar
          var sb_frame = EnvelopeBuilderEx.CreateEnvelope(3.09, 0.914, 5.4, 1.41);

          var sbar_info = new ScaleBarInfo()
          {
            MapFrameName = mf.Name
          };
          ElementFactory.Instance.CreateMapSurroundElement(
              layout, sb_frame, sbar_info, "", false,
              new ElementInfo() { Anchor = Anchor.BottomMidPoint });
        }

        if (hasLegend)
        {
          // Add a legend
          var legend_frame = EnvelopeBuilderEx.CreateEnvelope(0.77, 0.5, 3.0, 1.2);

          var legend_info = new LegendInfo()
          {
            MapFrameName = mf.Name
          };

          ElementFactory.Instance.CreateMapSurroundElement(
              layout, legend_frame, legend_info, "", false,
              new ElementInfo() { Anchor = Anchor.BottomLeftCorner });
        }
        return layout;
      });

      //GUI thread
      await FrameworkApplication.Current.Dispatcher.InvokeAsync(async () =>
      {
        await ProApp.Panes.CreateLayoutPaneAsync(layout);
      });
    }

  }
}

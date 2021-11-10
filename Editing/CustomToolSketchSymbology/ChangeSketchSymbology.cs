//   Copyright 2021 Esri
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

namespace CustomToolSketchSymbology
{
  //This MapTool allows you to sketch on the map when it is activated and clears the sketch on finishing or discarding the sketch
  internal class ChangeSketchSymbology : MapTool
  {
    public ChangeSketchSymbology()
    {
      IsSketchTool = true;
      SketchType = SketchGeometryType.Polygon;
      SketchOutputMode = SketchOutputMode.Map;
    }

    /// <summary>
    /// On activating this MapTool, the sketch segment symbology is modified along with the symbologies of the
    /// unselected current and unselected regular vertices of the sketch.
    /// The SketchSymbol property of the MapTool is also modified, which is different from the sketch segment and vertices.
    /// This property is used to customize the fixed part of the sketch, i.e. the part of the sketch that shows you what the 
    /// output will look like if you finish the sketch right then without doing any more edits.
    /// </summary>
    /// <param name="active"></param>
    /// <returns></returns>
    protected override Task OnToolActivateAsync(bool active)
    {
      var darkBlue = new CIMRGBColor() { R = 0, G = 76, B = 153 };
      var lightBlue = new CIMRGBColor() { R = 102, G = 178, B = 255 };
      var darkGreen = new CIMRGBColor() { R = 0, G = 153, B = 0 };
      var lightGreen = new CIMRGBColor() { R = 102, G = 255, B = 102 };
      var red = new CIMRGBColor() { R = 153, G = 0, B = 0 };
      var white = new CIMRGBColor() { R = 255, G = 255, B = 255 };

      //return base.OnToolActivateAsync(active);
      return QueuedTask.Run(() =>
      {
        //Getting the current symbology options of the segment
        var segmentOptions = GetSketchSegmentSymbolOptions();
        //Modifying the primary color, secondary color, and the width of the segment symbology options
        segmentOptions.PrimaryColor = darkBlue;
        segmentOptions.Width = 1.5;
        segmentOptions.SecondaryColor = lightBlue;

        //Creating a new vertex symbol options instances with the values you want
        //Vertex symbol options instance 1
        var vertexOptions = new VertexSymbolOptions(VertexSymbolType.RegularUnselected);
        vertexOptions.Color = darkGreen;
        vertexOptions.MarkerType = VertexMarkerType.Circle;
        vertexOptions.OutlineColor = lightGreen;
        vertexOptions.OutlineWidth = 4;
        vertexOptions.Size = 8;

        //Vertex symbol options instance 2
        var vertexOptions2 = new VertexSymbolOptions(VertexSymbolType.CurrentUnselected);
        vertexOptions2.Color = white;
        vertexOptions2.OutlineColor = red;
        vertexOptions2.MarkerType = VertexMarkerType.PushPin;
        vertexOptions2.OutlineWidth = 5;
        vertexOptions2.Size = 10;

        try
        {
          //Setting the value of the segment symbol options
          SetSketchSegmentSymbolOptions(segmentOptions);

          //Setting the value of the vertex symbol options of the regular unselected vertices using the vertexOptions instance 1 created above.
          SetSketchVertexSymbolOptions(VertexSymbolType.RegularUnselected, vertexOptions);
          //Setting symbol options of current unselected vertex
          SetSketchVertexSymbolOptions(VertexSymbolType.CurrentUnselected, vertexOptions2);

          //Similarly you can set symbol options for current selected vertex and regular selected vertex
          //SetSketchVertexSymbolOptions(VertexSymbolType.CurrentSelected, vertexOptions3);
          //SetSketchVertexSymbolOptions(VertexSymbolType.RegularSelected, vertexOptions4);

          //Modifying the SketchSymbol property of the MapTool
          var yellow = CIMColor.CreateRGBColor(255, 215, 0);
          var cimLineSymbol = SymbolFactory.Instance.ConstructLineSymbol(yellow, 4, SimpleLineStyle.DashDotDot);
          base.SketchSymbol = cimLineSymbol.MakeSymbolReference();
        }
        catch (Exception ex)
        {
              System.Diagnostics.Debug.WriteLine($@"Unexpected Exception: {ex}");
        }

      });
    }

    /// <summary>
    /// Upon completion of the sketch, it is cleared and no further action is done.
    /// </summary>
    /// <param name="geometry"></param>
    /// <returns></returns>
    protected override Task<bool> OnSketchCompleteAsync(Geometry geometry)
    {
      return base.OnSketchCompleteAsync(geometry);
    }
  }
}

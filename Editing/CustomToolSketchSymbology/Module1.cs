//   Copyright 2021 Esri
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
using System.Windows.Input;
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
  /// <summary>
  /// This sample provides a custom line construction tool and a MapTool.
  /// The custom line construction tool for polyline templates when activated, modifies the sketch symbology just for that particular tool. By the term "Sketch Symbology" here, we are referring to the symbology of the sketch segment, and vertices. The sketch vertices have 4 different symbologies for unselected vertex, selected vertex, current vertex unselected, and current vertex selected.
  /// ![UI](Screenshots/SketchSymbologyExplanation.jpg)
  /// ![UI](Screenshots/SketchSymbologyExplanation2.jpg)
  /// ![UI](Screenshots/SketchSymbologyExplanation3.jpg)
  /// This sample shows you how you can edit those symbologies. So when you activate the custom tool, the modified sketch symbology is used and when you deactivate it, it goes back to the symbology set in the application settings.
  /// 
  /// The ChangeSketchSymbology MapTool can be found in the AddIn tab on the main application ribbon.
  /// ![UI](Screenshots/MapTool.jpg)
  /// Once you activate it, it changes the sketch symbology we talked about earlier and also modifies a property called "SketchSymbol" of the MapTool, and then allows you to sketch a polygon.
  /// The sketch is then cleared upon completion. This will help you understand the difference between the SketchSymbol property and the sketch segment and sketch vertices.
  /// The SketchSymbol is used to customize the fixed part of the sketch, i.e. the part of the sketch that shows you what the feature will look like if you finish the sketch right then without doing any further edits.
  /// In this example, the SketchSegment is changed to be a yellow line with a dash dot dot style.
  /// The sketch segment is dashed dark blue and light blue line.
  /// The current unselected vertex is a red pin, and the other unselected vertices are made a circular dark and light green point.
  /// ![UI](Screenshots/MapTool2.jpg)
  /// </summary>
  /// <remarks>
  /// For sample data, download CommunitySampleData-12-15-2020.zip from https://github.com/Esri/arcgis-pro-sdk-community-samples/releases
  /// and unzip it. We will be using the project in "Working with Core Geometry and Data" folder as an example for this AddIn.
  /// 1. In Visual Studio click the Build menu. Then select Build Solution.
  /// 1. Click Start button to open ArcGIS Pro.
  /// 1. ArcGIS Pro will open. 
  /// 1. Open the Workshop.aprx file from Core Geometry and Data folder you just downloaded.  We can use any project that contains editable polyline data. We are just using Workshop.aprx as an example here.
  /// 1. Open the Create features pane and select a polyline template (in this example that will be the sdk_polylines template).
  /// 1. You will see a custom tool named "Custom line construction tool" at the end of the tools listed below the activated template.
  /// ![UI](Screenshots/CustomTool.jpg)
  /// 1. First let us observe the current sketch symbology by sketching with the default tool of that line template.
  /// ![UI](Screenshots/CustomTool2.jpg)
  /// 1. Now activate the custom tool and start sketching and observe how the sketch symbology has changed.
  /// ![UI](Screenshots/CustomTool3.jpg)
  /// 1. In this sample, the segment's primary color is changed to orange and the size is changed to 4.
  /// 1. The unselected vertex symbology is also modified to have a yellow color, total size 5, and purple outline of width 3.
  /// 1. When you finish or cancel the sketch (so as to deactivate the custom tool) and then activate any one of the other tools, you'll notice that the sketch symbology has been restored to the application settings.
  /// </remarks>
  internal class Module1 : Module
  {
    private static Module1 _this = null;

    /// <summary>
    /// Retrieve the singleton instance to this module here
    /// </summary>
    public static Module1 Current
    {
      get
      {
        return _this ?? (_this = (Module1)FrameworkApplication.FindModule("CustomToolSketchSymbology_Module"));
      }
    }

    #region Overrides
    /// <summary>
    /// Called by Framework when ArcGIS Pro is closing
    /// </summary>
    /// <returns>False to prevent Pro from closing, otherwise True</returns>
    protected override bool CanUnload()
    {
      //TODO - add your business logic
      //return false to ~cancel~ Application close
      return true;
    }

    #endregion Overrides

  }
}

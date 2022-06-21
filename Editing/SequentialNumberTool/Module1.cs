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
using System.Windows.Input;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using System.Threading.Tasks;

namespace SeqNum
{
  /// <summary>
  /// This sample is a custom edit tool that sequentially numbers point, line or polygon features along a sketch.
  /// The sample has been enhanced from previous ArcMap samples (viperpin and snakepin) to also number line and point feature classes
  /// and to format text strings for parcel pin numbering.
  /// </summary>
  /// <remarks>
  /// 1. Prepare some polygon data with an integer and text field for testing.
  /// 1. In Visual Studio, build the solution.
  /// 1. Launch the debugger to open ArcGIS Pro.
  /// 1. In Pro start with a new map and add your polygon data.  
  /// 1. Open the Modify Features dockpane from the edit tab.
  /// 1. Locate and activate the Sequential Numbering tool from the Sample Tools category in the dockpane.
  /// 1. In the tool pane, select the polygon layer name, the text field, set the start and increment values.
  /// 1. In the map sketch and finish a line accross the polygons you wish to attribute.
  /// The polygons will be attributed sequentially and the start value set to the next highest number.
  /// ![UI](Screenshots/poly_sketch.png)
  /// ![UI](Screenshots/poly_result.png)
  /// 1. Change the format string to 51-###-A.
  /// 1. Sketch and finish a line accross some polygons.
  /// 1. The polygons will be attributed sequentially. The number of # determines leading zeros in the string format.
  /// ![UI](Screenshots/poly_format_sketch.png)
  /// ![UI](Screenshots/poly_format_result.png)
  /// 
  /// Continue testing with other text format combinations, feature class types and integer fields.
  /// The text format string is only available for text string fields.
  /// </remarks>
  internal class Module1 : Module
  {
    private static Module1 _this = null;

    public static List<String> _layerList = new List<string>();
    public static List<String> _fieldList = new List<string>();
    public static string _targetLayer;
    public static string _targetField;
    public static Boolean _isTargetFieldString;

    public static string _stringFormat = "#";

    public static string _startValue = "1";
    public static string _incValue = "1";

    /// <summary>
    /// Retrieve the singleton instance to this module here
    /// </summary>
    public static Module1 Current
    {
      get
      {
        return _this ?? (_this = (Module1)FrameworkApplication.FindModule("SeqNum_Module"));
      }
    }

    #region Overrides

    protected override Task OnReadSettingsAsync(ModuleSettingsReader settings)
    {
      //_svalue = settings["seqnum_startvalue"].ToString();
      //_ivalue = settings["seqnum_incvalue"].ToString();
      return base.OnReadSettingsAsync(settings);
    }

    protected override Task OnWriteSettingsAsync(ModuleSettingsWriter settings)
    {
      //settings.Add("seqnum_startvalue", _svalue);
      //settings.Add("seqnum_incvalue", _ivalue);
      return base.OnWriteSettingsAsync(settings);
    }

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

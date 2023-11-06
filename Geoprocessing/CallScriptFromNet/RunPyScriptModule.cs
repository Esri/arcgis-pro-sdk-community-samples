//Copyright 2019 Esri

//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at

//       https://www.apache.org/licenses/LICENSE-2.0

//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific .cs governing permissions and
//   limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;

namespace CallScriptFromNet
{
  /// <summary>
  /// This Button add-in, when clicked, uses the first Polygon layer in the Map and runs a buffer operation with help of a Python script.  It reads the text stream output from python and displays it. 
  /// </summary>
  /// <remarks>
  /// 1. This solution file includes an example python script named test1.py which is included as 'Content' (Build action) in the add-in.
  /// 1. This sample also requires an ArcGIS Project with a map that has at least one polygon layer in the Map's table of content.
  /// 1. Rebuild the project in Visual Studio and debug. 
  /// 1. In ArcGIS Pro open a project with a map that has one polygon layer in the Map's TOC.
  /// 1. Make sure the Map is the active map.
  /// 1. Go to the ADD-IN Tab, find the "Python Script" group and click "Show Python Feedback" followed by "Run Py Script".
  /// 1. The "Run Python Script" Dockpane will populate the "In" and "Out" feature classes used for the script's buffer operation.  
  /// 1. Note: the "In" feature class is derived from the first feature layer in the active Map's table of content.
  /// 1. Note: the "Out" feature class is derived from the "In" feature class with a "_Buffer" appended to the feature class name.
  /// 1. Note: if the "Out" feature class exists the code will delete the feature class in order for the script to work.
  /// 1. The Feedback from Python is displayed in the dockpane:
  /// ![UI](Screenshots/Screen1.png)
  /// 1. If you have ArcGIS Pro installed and licensed on your machine you should be able to run this python script directly from a Windows command prompt.
  /// 1. To do so open a Windows Command prompt and copy/paste the content from the Dockpane's 'Command line' field.
  /// ![UI](Screenshots/Screen2.png)
  /// </remarks>
  internal class RunPyScriptModule : Module
  {
    private static RunPyScriptModule _this = null;

    /// <summary>
    /// Retrieve the singleton instance to this module here
    /// </summary>
    public static RunPyScriptModule Current
    {
      get
      {
        return _this ?? (_this = (RunPyScriptModule)FrameworkApplication.FindModule("CallScriptFromNet_Module"));
      }
    }

    #region Module Properties
    
    internal static RunPythonWithFeedbackViewModel RunPythonWithFeedbackViewModel { get; set; }

    #endregion

  }
}

// Copyright 2017 Esri 
//
// 
//   Licensed under the Apache License, Version 2.0 (the "License"); 
//   you may not use this file except in compliance with the License. 
//   You may obtain a copy of the License at 
//
//       http://www.apache.org/licenses/LICENSE-2.0 
//
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

namespace ImpersonateMapPane
{
  /// <summary>
  /// This sample illustrates how to create a new map pane that impersonates an existing map pane. A control will be added to the new pane allowing you to view the map CIM.  A custom
  /// TOC implementation will also be demonstrated.A new Visual Studio template 'ArcGIS Pro Map Pane Impersonation' has been added at ArcGIS Pro 2.0.  
  /// </summary>
  /// <remarks>
  /// 1. Download the Community Sample data (see under the 'Resources' section for downloading sample data)
  /// 1. Make sure that the Sample data is unzipped in c:\data
  /// 1. The project used for this sample is 'C:\data\Interacting with Maps\Interacting with Maps.aprx'
  /// 1. In Visual Studio click the Build menu.Then select Build Solution. 
  /// 1. Click Start button to open ArcGIS Pro.
  /// 1. ArcGIS Pro will open, select the Interacting with Maps.aprx project
  /// 1. Select the 'Add-in' tab on the ArcGIS Pro ribbon. See the 'Open ImpersonateMapPane1' button
  /// ![UI](Screenshots/ImpersonateMapPane_2.png)
  /// 1. Click the button and see the Impersonate pane open.  Reposition the pane to view it side by side with the 2D map
  /// ![UI](Screenshots/ImpersonateMapPane_4.png)</remarks>
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
        return _this ?? (_this = (Module1)FrameworkApplication.FindModule("ImpersonateMapPane_Module"));
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

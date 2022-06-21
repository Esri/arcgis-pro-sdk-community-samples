/*

   Copyright 2019 Esri

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
using ArcGIS.Desktop.Mapping;

namespace TransferAttributes
{
  /// <summary>
  /// This sample demonstrates two ways to transfer attributes to a target feature using the application Field Mapping options set between two layers. 
  /// The Field Mapping options are where a user can specify how attribute field values on a source layer are processed and copied to fields on a target layer. 
  /// This sample also includes access to the Field Mapping page to allow editing of the options. 
  /// </summary>
  /// <remarks>
  /// 1. Download the Community Sample data (see under the 'Resources' section for downloading sample data)
  /// 1. Make sure that the Sample data is unzipped in c:\data
  /// 1. Before you run the sample verify that the project "C:\Data\Interacting With Maps\Interacting with Maps.aprx" is present since this is required to run the sample.
  /// 1. In Visual Studio click the Build menu. Then select Build Solution.
  /// 1. Launch the debugger to open ArcGIS Pro.
  /// 1. ArcGIS Pro will open. 
  /// 1. Open the "C:\Data\Interacting With Maps\Interacting with Maps.aprx" project.
  /// 1. Click on the Add-in tab on the ribbon and verify the add-in has loaded. 
  /// ![UI](Screenshots/Screen1.png)
  /// 1. Move to the TOC List by Selection tab and ensure that the Portland Precincts layer is the only selectable layer. 
  /// ![UI](Screenshots/Screen2.png)
  /// 1. Move back to the TOC Drawing Order tab and turn off all layers except the Portland Precincts and the Topographic layers. 
  /// 1. Right click on the Portland Precincts layer and select Zoom to Layer
  /// 1. Click on the Add-in tab on the ribbon and select the 'Transfer Attributes between Features' tool. 
  /// 1. Click on the map over a precinct. Then click on the map a second time over a different precinct. 
  /// ![UI](Screenshots/Screen4.png)
  /// 1. The second precinct should now have the attributes of the first precinct. The symbology of the feature should have also changed. 
  /// ![UI](Screenshots/Screen5.png)
  /// 1. Move back to the Add-in tab on the ribbon and select the 'Transfer Attributes from Templates' tool. 
  /// 1. The Transfer Attributes from Template UI displays in a dock pane.  
  /// ![UI](Screenshots/Screen6.png)
  /// 1. Use the Field Mapping button on the burger button (or from the Add-in ribbon) to investigate the current field mappings between layers. Add additional 
  /// field mappings if required. 
  /// 1. Choose the layer Portland Precincts and the Central Precinct template.
  /// 1. Click in the precinct polygon that you changed with the previous tool.  The feature will now have the attributes of the Central Precinct template.
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
        return _this ?? (_this = (Module1)FrameworkApplication.FindModule("TransferAttributes_Module"));
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

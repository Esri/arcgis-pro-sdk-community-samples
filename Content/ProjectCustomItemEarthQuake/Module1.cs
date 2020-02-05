/*

   Copyright 2019 Esri

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

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

namespace ProjectCustomItemEarthQuake
{
    /// <summary>
    /// This sample covers how to 'customize' Pro's access to content by using custom items and project custom items.  Specifically we are reading a 'custom' formatted XML file containing earthquake data.
    /// </summary>
    /// <remarks>
    /// 1. Download the Community Sample data (see under the 'Resources' section for downloading sample data)
    /// 1. Make sure that the Sample data is unzipped in c:\data 
    /// 1. The data used in this sample is located in this folder 'C:\Data\CustomItem\QuakeCustomItem'
    /// 1. In Visual Studio click the Build menu. Then select Build Solution.
    /// 1. Click Start button to open ArcGIS Pro.
    /// 1. In ArcGIS Pro open C:\Data\CustomItem\QuakeCustomItem\QuakeCustomItem.aprx
    /// 1. Open ArcGIS Pro's Catalog dockpane
    /// 1. Under Folders browse to QuakeCustomItem and open the folder.  
    /// 1. Notice that earthquake.quake has a custom icon.  This is the result of a -custom item- implementation for all .quake file extensions.
    /// ![UI](Screenshots/Screen1.png)  
    /// 1. Open config.daml in the solution and find the update to the -esri_customItems- category, specifically the -acme_quake_handler- component.  This component allows to specify a -fileExtension- -quake- and also a -className- -ProjectCustomItemEarthQuake.Items.QuakeProjectItem- for the codebehind implementation.
    /// 1. Open the Items\QuakeProjectItem.cs source in the solution.  Notice that this class derives from -CustomProjectItemBase- which provides the most of the functionality required for project custom items.  Notice some overrides to modify the out-of-box behavior like for example the -bex the dog- icon.
    /// 1. Back in ArcGIS Pro's Catalog pane open -earthquake.quake-.  Notice the list of earthquake events that a -children- of the -earthquake.quake- file.
    /// ![UI](Screenshots/Screen1.png)   
    /// 1. Open the Items\QuakeProjectItem.cs source in the solution.  Notice that this class overrides the -Fetch- function which uses the -AddRangeToChildren- function to add children to the -earthquake.quake- parent.
    /// ![UI](Screenshots/Screen2.png)   
    /// 1. Navigate back to the *Bex the dog* icon, right click on this item to bring up the context menu and then click on *Add To Project* to add the *Item* to the current project.
    /// ![UI](Screenshots/Screen3.png)
    /// ![UI](Screenshots/Screen4.png)
    /// 1. Rename *earthquakes.quake* by using the rename Context Menu button or by simply clicking on the *Custom Project Item* name to enable editing of the name
    /// ![UI](Screenshots/Screen5.png)
    /// ![UI](Screenshots/Screen6.png)
    /// 1. Click the "Open Quake Event" button. Browse to the 'C:\Data\CustomItem\QuakeCustomItem' folder. You will be able to see the .quake item.  Double click the .quake item to browse into it to see the quake events
    ///![UI](Screenshots/Screen6.png)
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
        return _this ?? (_this = (Module1)FrameworkApplication.FindModule("ProjectCustomItemEarthQuake_Module"));
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

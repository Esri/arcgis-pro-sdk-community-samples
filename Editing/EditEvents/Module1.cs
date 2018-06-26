/*

   Copyright 2018 Esri

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
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using System.Threading.Tasks;
using ArcGIS.Desktop.Mapping;

namespace EditEventsSample
{
	/// <summary>
	/// The EditEventsSpy dockpane listens to the editing Row, Completing, and Completed events
	/// </summary>
	/// <remarks>
	/// 1. Download the Community Sample data (see under the 'Resources' section for downloading sample data).  The sample data contains required data for this sample add-in.  Make sure that the Sample data is unzipped in c:\data and "C:\Data\Interacting with Maps" is available.
	/// 1. In Visual Studio click the Build menu. Then select Build Solution.
	/// 1. Click Start button to open ArcGIS Pro.
	/// 1. ArcGIS Pro will open.
	/// 1. Open the "C:\Data\Interacting with Maps\Interacting with Maps.aprx" project which contains the required data needed for this sample.
	/// 1. Make sure "Portland Crimes" is the active map and the "Contents" dockpane is open.
	/// 1. Select "Crimes" on the Contents table of content.
	/// 1. From the "Add-in" tab select "Show EditorEventsSpy".
	/// ![UI](Screenshots/Screen1.png)
	/// 1. Click Start Events to start listening for the edit events. Click the Create, Change, or Delete buttons to execute edits that trigger the events. 
	/// 1. They will be reported to you on the EditEventsSpy dockpane 'as they happen'.<br/>
	/// ![UI](Screenshots/Screen2.png)
	/// 1. Check the Cancel edits, Validate edits, and Fail validation checkboxes (in any combination) to apply that action in the respective row event handler. Note that canceling the edit or canceling the edit because of failed validation terminates the entire edit operation and no further events fire for that operation.<br/>
	/// 1. By default, the Create will create two features so you can see the two row events.
	/// ![UI](Screenshots/Screen3.png)
	/// 1. The created features will be selected so clicking Change or Delete will apply the respective edit to the newly created features. Feel free to select others.<br/>
	/// 1. You can also try commenting out the cancel logic in the row event handler and, instead, canceling the edit in the EditCompletingEvent.
	/// </remarks>
	internal class Module1 : Module
  {
    private static Module1 _this = null;
    public readonly string CrimesLayerName = "Crimes";

    /// <summary>
    /// Retrieve the singleton instance to this module here
    /// </summary>
    public static Module1 Current
    {
      get
      {
        return _this ?? (_this = (Module1)FrameworkApplication.FindModule("EditEventsSample_Module"));
      }
    }

    
    /// <summary>
    /// Gets the crimes layer or null
    /// </summary>
    public FeatureLayer Crimes
    {
      get
      {
        return MapView.Active?.Map.GetLayersAsFlattenedList().FirstOrDefault(fl => fl.Name == CrimesLayerName) as FeatureLayer;
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

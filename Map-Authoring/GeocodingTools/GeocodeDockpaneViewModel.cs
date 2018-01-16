/*

   Copyright 2017 Esri

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
using System.Threading.Tasks;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;

namespace GeocodingTools
{
  /// <summary>
  /// Provide geocoding functionality by using the <see cref="ArcGIS.Desktop.Mapping.Controls.LocatorControl"/>. 
  /// </summary>
  internal class GeocodeDockpaneViewModel : DockPane
  {
    private const string _dockPaneID = "GeocodingTools_GeocodeDockpane";

    protected GeocodeDockpaneViewModel() { }

    /// <summary>
    /// Show the DockPane.
    /// </summary>
    internal static void Show()
    {
      DockPane pane = FrameworkApplication.DockPaneManager.Find(_dockPaneID);
      if (pane == null)
        return;

      pane.Activate();
    }


    /// <summary>
    /// when the dockpane is activated (or inactivated), publish the LocatorActivateEvent.  This sends a message to the Locator control indicating it is to be the
    /// active (or inactive) Locator control. 
    /// </summary>
    /// <param name="isActive"></param>
    protected override void OnActivate(bool isActive)
    {
      ArcGIS.Desktop.Mapping.Controls.LocatorActivateEvent.Publish(new ArcGIS.Desktop.Mapping.Controls.LocatorActivateEventArgs(isActive));
    }
  }

  /// <summary>
  /// Button implementation to show the DockPane.
  /// </summary>
	internal class GeocodeDockpane_ShowButton : Button
  {
    protected override void OnClick()
    {
      GeocodeDockpaneViewModel.Show();
    }
  }
}

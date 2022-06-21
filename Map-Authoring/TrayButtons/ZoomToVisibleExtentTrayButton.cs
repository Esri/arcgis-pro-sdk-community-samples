/*

   Copyright 2022 Esri

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
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Mapping;
using System.Linq;
using System.Threading.Tasks;

namespace TrayButtons
{
  internal class ZoomToVisibleExtentTrayButton : MapTrayButton
  {
    /// <summary>
    /// Invoked after construction, and after all DAML settings have been loaded. 
    /// Use this to perform initialization such as setting ButtonType.
    /// </summary>
    protected override void Initialize()
    {
      base.Initialize();

      // set the button type
      //  change for different button types
      ButtonType = TrayButtonType.Button;

      // ClickCommand is used for TrayButtonType.Button only
      ClickCommand = new RelayCommand(DoClick);
    }
    /// <summary>
    /// Override to perform some button initialization.  This is called the first time the botton is loaded.
    /// </summary>
    protected override void OnButtonLoaded()
    {
      base.OnButtonLoaded();
    }

    /// <summary>
    /// This method gets all visible layers in the map and zooms to their combined extent.
    /// </summary>
    /// <returns></returns>
    public Task<bool> ZoomToVisibleLayersAsync()
    {
      if (this.MapView == null)
        return Task.FromResult(false);

      //Zoom to all visible layers in the map.
      var visibleLayers = this.MapView.Map.Layers.Where(l => l.IsVisible);
      return this.MapView.ZoomToAsync(visibleLayers);
    }

    #region TrayButtonType.Button
    private void DoClick()
    {
      // When the tray button is clicked zoom the map to the extent of visible layers in the map. 
      ZoomToVisibleLayersAsync();
  
    }
    #endregion

  }
}

//   Copyright 2017 Esri
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at

//       http://www.apache.org/licenses/LICENSE-2.0

//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License. 

using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;

namespace ChangeColorizerForRasterLayer
{
    /// <summary>
    /// This sample demonstrates how to use the raster colorizer definitions to create a specific colorizer, and apply the new colorizer to the selected raster layer.  
    /// The sample includes these functions:
    /// 1.) Creates a new image service layer and add the layer to the current map.
    /// 2.) Displays a collection of colorizers in a combo box that can be applied to the selected layer.  
    /// 3.) Sets the selected colorizer to the layer.
    /// </summary>
    /// <remarks>
    /// 1. In Visual Studio click the Build menu. Then select Build Solution.
    /// 2. Click Start button to open ArcGIS Pro.
    /// 3. ArcGIS Pro will open. 
    /// 4. Open a new map view.
    /// 5. Click on the ADD-IN tab. 
    /// 6. Click the Add Raster Layer button to add a new image service layer to the map.
    ///    (Make sure the layer is selected on the Contents pane).
    /// ![UI](Screenshots/Screen1.png)
    /// 7. Click the drop down arrow on the right of the "Apply Colorizers" combo box to show the list of applicable colorizers.
    /// ![UI](Screenshots/Screen2.png)
    /// 8. Select different colorizers from the list to apply to the layer. 
    ///    You will see the layer is rendered with different customized colorizers that you selected.  
    /// 9. You can try the "Apply colorizers" functionality on your own layers. 
    ///    (Be sure the selected layer is either raster layer, image service layer, or mosaic layer).
    /// ![UI](Screenshots/Screen3.png)
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
        return _this ?? (_this = (Module1)FrameworkApplication.FindModule("ChangeColorizerForRasterLayer_Module"));
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

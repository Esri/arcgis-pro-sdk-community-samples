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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using System.Threading.Tasks;

namespace MaskRaster
{
  /// <summary>
  /// This sample shows how to author a tool that can be used to mask raster pixel values in a ractangle 
  /// defined by the user. The masked output is saved as a new raster dataset in the project folder. This 
  /// sample only works on a single raster layer at a time.
  /// Note: You will need write access to the project folder in order to use this sample. The sample saves
  /// your input image before masking it so if you use a large image the save can take a while.
  /// </summary>
  /// <remarks>
  /// 1. Download the Community Sample data (see under the 'Resources' section for downloading sample data)
  /// 1. Make sure that the Sample data is unzipped in C:\Data 
  /// 1. The project used for this sample is 'C:\Data\RasterSample\RasterSample.aprx'
  /// 1. In Visual Studio click the Build menu. Then select Build Solution.
  /// 1. Click Start button to open ArcGIS Pro.
  /// 1. ArcGIS Pro will open. 
  /// 1. Open the 'RasterSample.aprx' project or to use your own data open a map view and add a raster to the map.
  /// 1. Select the raster layer in the TOC.
  /// 1. Click on the Add-In tab on the ribbon.
  /// ![UI](Screenshots/Screenshot1.png)
  /// 1. Within this tab there is a Mask Raster Tool. Click it to activate the tool.
  /// 1. In the map draw a rectangle around the area of the raster you want to mask.
  /// ![UI](Screenshots/Screenshot2.png)
  /// 1. A copy of the source raster dataset of the layer you selected will be saved in your project folder, the copy will be processed to mask pixels and a new layer will be added to your map using the processed copy.
  /// ![UI](Screenshots/Screenshot3.png)
  /// 1. You need to adjust the newly added raster layer's symbology to see the full masking effect.
  /// ![UI](Screenshots/Screenshot4.png)
  /// 1. Press the escape key if you want to deactivate the tool.
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
                return _this ?? (_this = (Module1)FrameworkApplication.FindModule("MaskRaster_Module"));
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

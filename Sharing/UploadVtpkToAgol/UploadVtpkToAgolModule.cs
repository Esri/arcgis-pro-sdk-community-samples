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
using System.Windows.Input;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using System.Threading.Tasks;

namespace UploadVtpkToAgol
{
    /// <summary>
    /// This sample shows how to add a vector tile package file to a map both from disk and ArcGIS Online.  The sample also has code to 'upload' a vtpk file to ArcGIS Online.  
    /// </summary>
    /// <remarks>
    /// 1. Make sure that the Sample data is unzipped in c:\data     
    /// 1. The data used for this sample is 'C:\Data\VectorTileDemos\AlaskaGeology.vtpk'    
    /// 1. In Visual Studio click the Build menu. Then select Build Solution.  
    /// 1. Click Start button to open ArcGIS Pro.  
    /// 1. ArcGIS Pro will open.   
    /// 1. Open a new map project.   
    /// 1. Click on the *Upload to ArcGIS Online* tab on the ribbon.  
    /// 1. Within this tab there is a *Show &amp; Upload Vector Tile* button.  Click the button to display the *Show &amp; Upload Vector Tile* dockpane.  
    /// 1. On the *Show &amp; Upload Vector Tile* dockpane click the open file button and open *C:\Data\VectorTileDemos\AlaskaGeology.vtpk*.  
    /// 1. Click the *Add To Map* button to add the vector tile package to the current map.  
    /// ![UI](Screenshots/Screenshot1.png)  
    /// 1. Click the *Upload to ArcGIS Online (AGOL)* button to upload the vector tile package (referenced under 'Select a VTPK File').  
    /// 1. The upload status will be updated once the upload completes.  
    /// 1. Click the "Query AGOL' button to download the previously uploaded VTPK vector tile service that was created by AGOL using the uploaded vector tile package.  
    /// 1. Look in the code to see the query conditions.  
    /// ![UI](Screenshots/Screenshot2.png)   
    /// </remarks>
    internal class UploadVtpkToAgolModule : Module
    {

        private static UploadVtpkToAgolModule _this = null;

        /// <summary>
        /// Retrieve the singleton instance to this module here
        /// </summary>
        public static UploadVtpkToAgolModule Current
        {
            get
            {
                return _this ?? (_this = (UploadVtpkToAgolModule)FrameworkApplication.FindModule("UploadVtpkToAgol_Module"));
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

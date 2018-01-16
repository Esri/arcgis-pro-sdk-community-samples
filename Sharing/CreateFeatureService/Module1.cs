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

namespace CreateFeatureService
{
	/// <summary>
	/// This sample provides a dockpane allowing to create a feature service from the csv file that has been uploaded to AGOL or portal.
	/// see [ArcGIS REST API / Publish Item](http://resources.arcgis.com/en/help/arcgis-rest-api/index.html#//02r300000080000000)
	/// </summary>
	/// <remarks>
	/// 1. Download the Community Sample data (see under the 'Resources' section for downloading sample data)
    /// 1. Make sure that the Sample data is unzipped in c:\data 
    /// 1. The project used for this sample is 'C:\Data\FeatureTest\FeatureTest.aprx'
	/// 1. In Visual Studio click the Build menu. Then select Build Solution.
	/// 1. Click Start button to open ArcGIS Pro.
    /// 1. ArcGIS Pro will open, select the FeatureTest.aprx project
	/// 1. Click on the Add-in tab on the ribbon and then on the "Click to create service" button which opens the 'Create a feature service' dockpane.
	/// ![UI](Screenshot/Screen1.png)  
	/// 1. Click the '1. Use Active Portal' button to fill in the current active portal and user name.
	/// ![UI](Screenshot/Screen11.png)  
	/// 1. Log into your active portal via a browser using the same credentials.  
	/// 1. Go to the 'My Content' page on the portal and use the 'Add Item' function to upload the PointOfInterest.csv file which can be found in the solution folder for this add-in sample.
	/// ![UI](Screenshot/Screen2.png)  
	/// 1. Click the '2. Get CSV Content' button to populate the 'Content Item to Publish' list and select 'PointOfInterest' from the drop-down list.
	/// ![UI](Screenshot/Screen3.png)  
	/// 1. Click the '3. Analyze CSV Source' button to populate the 'Publish Parameter' field.
	/// 1. Copy the content of the 'Publish Parameter' field into a json editor (i.e. Visual Studio)
	/// 1. Edit the json according to ArcGIS REST API requirements to ensure that your data can be published.  In order to publish the sample PointOfInterest.csv file you can simply click the 'Fix it' button to paste in the content from PointOfInterest.json which is also provided in this solution.
	/// ![UI](Screenshot/Screen4.png) 
	/// 1. The 'Fix It' button pastes the edited content into the  'Publish Parameter' field 
    /// 1. Click the '4. Submit' button.
	/// ![UI](Screenshot/Screen5.png) 
	/// 1. Copy the link returned by the successful Publish operation and use 'Map | Add Data From Path' dialog to paste in the link to the newly created feature data.  
	/// 1. Open the Content dockpane and zoom the newly added KauaiPOI layer.
	/// ![UI](Screenshot/Screen6.png) 
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
                return _this ?? (_this = (Module1)FrameworkApplication.FindModule("CreateFeatureService_Module"));
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

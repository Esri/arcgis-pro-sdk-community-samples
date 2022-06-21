//   Copyright 2019 Esri
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at

//       https://www.apache.org/licenses/LICENSE-2.0

//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License. 

using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;

namespace DeleteFeaturesBasedOnSubtype
{
	/// <summary>
	/// This addin lists all subtypes for a Feature Class and allows deleting all features with a specific subtype
	/// </summary>    
	/// <remarks>
	/// 1. Download the Community Sample data (see under the 'Resources' section for downloading sample data).  The sample data contains required data for this sample add-in.  Make sure that the Sample data is unzipped in c:\data and "C:\Data\Interacting with Maps" is available.
	/// 1. In Visual Studio click the Build menu. Then select Build Solution.
	/// 1. Click Start button to open ArcGIS Pro.
	/// 1. ArcGIS Pro will open.
	/// 1. Open the "C:\Data\Interacting with Maps\Interacting with Maps.aprx" project which contains the required data needed for this sample.
	/// 1. Make sure the Map Pane and the corresponding Table of Contents (TOC) Pane are Open and the Map Pane is Active
	/// 1. Select "Crimes" on the Contents table of content.
	/// ![UI](Screenshots/Screen1.png)
	/// 1. Click on the Add-In Tab and note the "Delete All Features Having Subtype" dropdown.
	/// 1. Note that when no Layer is selected in the TOC, clicking on the combobox will show an empty list. An empty list will also be shown if the Layer is not a Feature Layer i.e. if it is a standalone table.
	/// 1. Select the  "Crimes" feature layer because it has a defined subtype. 
	/// 1. Opening the Combobox will list all the subtypes for the FeatureClass.
	/// ![UI](Screenshots/Screen2.png)
	/// 1. On Selecting a Subtype, all the features with that subtype will be deleted from the FeatureClass and thereby cleared from the Map.
	/// 1. Finally you can use the "Undo" button to restore the subtype data you just deleted.
	/// ![UI](Screenshots/Screen3.png)    
	/// </remarks>
	internal class DeleteBasedOnSubtypeModule : Module
    {
        private static DeleteBasedOnSubtypeModule _this = null;

        /// <summary>
        /// Retrieve the singleton instance to this module here
        /// </summary>
        public static DeleteBasedOnSubtypeModule Current
        {
            get
            {
                return _this ?? (_this = (DeleteBasedOnSubtypeModule)FrameworkApplication.FindModule("acme_Module_113056"));
            }
        }
    }
}

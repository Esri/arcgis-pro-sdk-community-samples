//   Copyright 2015 Esri
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
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;

namespace DeleteFeaturesBasedOnSubtype
{
    /// <summary>
    /// This addin will list all subtypes for a Feature Class and allow deletion of all features which have the selected subtype
    /// </summary>    
    /// <remarks>
    /// 1. In Visual Studio click the Build menu. Then select Build Solution.
    /// 2. Click Start button to open ArcGIS Pro.
    /// 3. ArcGIS Pro will open. 
    /// 4. Open a map view that contains at least one Feature Layer whose source points to a Feature Class (From either a File Geodatabase or an Enterprise Geodatabase)
    /// 5. Make sure the Map Pane and the corresponding Table of Contents (TOC) Pane are Open and the Map Pane is Active
    /// 6. Click on the Add-In Tab
    /// 7. Observe that when no Layer is selected in the TOC, clicking on the combobox will show an empty list. An empty list will also be shown if the Layer is not a Feature Layer i.e. if it is a standalone table.
    /// 8. Select a Feature Layer which has a Feature Class as the Data Source from the TOC pane 
    /// 9. Opening the Combobox will list all the subtypes for the FeatureClass
    /// 10. On Selecting a Subtype, all the features with that subtype will be deleted from the FeatureClass and thereby cleared from the Map
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

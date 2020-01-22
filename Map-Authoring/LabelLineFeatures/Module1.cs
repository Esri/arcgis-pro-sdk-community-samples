/*

   Copyright 2020 Esri

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

namespace LabelLineFeatures
{
    /// <summary>
    /// This sample demonstrates how to select line features and label them. New label classes are created in code that are used to label the selected features. Two methods to label selected features are demonstrated.
    /// </summary>
    /// <remarks>
    /// Using the sample:
    /// 1. Download the Community Sample data (see under the 'Resources' section for downloading sample data). The sample data contains a dataset called 'USA'. Make sure that the Sample data is unzipped in C:\data and "C:\Data\Admin\AdminData.gdb" is available.
    /// 1. In Visual Studio click the Build menu. Then select Build Solution.
    /// 1. Click Start button to open ArcGIS Pro.
    /// 1. ArcGIS Pro will open. 
    /// 1. Open a new Map project. 
    /// 1. Add the US Rivers feature class from USA dataset.
    /// 1. In the Add-in tab, notice the two groups created: Label using OIDs and Label using attributes.
    /// ![UI](Screenshots/LabelLineFeatures.png)
    /// 1. **Label using OIDs:** Click the "Label using OIDs" tool to activate it. Select River features in the map using the tool. This will label the selected features with the River feature class' "Miles" field. A new Label class (LabelSelectedFeaturesWithLength) is created for this. This Label class' SQL Query is updated to use the OIDs of the selected features. This method cannot be used if you select large datasets. The SQL Query that uses the selected OIDs could exceed the SQL string length limit if too many features are selected. 
    /// ![UI](Screenshots/LabelSelectedFeaturesWithLength.png)
    /// 1. **Label using attributes:**
    /// 1. The following workflow requires a new field to be added to the "U.S. Rivers (Generalized)" feature class.  Right click on "U.S. Rivers (Generalized)" on the map's TOC and chose "Design | Fields".  Now add the 'LabelOn' field as shown below:
    /// ![UI](Screenshots/AddLabelOnField.png)
    /// 1. Click the "Label using attributes" tool to activate it. Select River features in the map using this tool.  This will label the selected features with the River feature class' "Miles" field. The tool first edits the "LabelOn" field. The selected features will have the LabelOn attribute value changed to Yes. A new label class (LabelSelectedManyFeaturesWithLength) is created. This Label class' SQL Query queries for features with the LabelOn attribute value of "Yes".  The advantage of this method is that you can use this to label large datasets.  The SQL Query on the Label Class will not change since this method edits the attribute table. The selected features will have their LabelOn attribute set to Yes.
    /// ![UI](Screenshots/LabelSelectedManyFeaturesWithLength.png)
    /// 1. The "Reset LabelOn field" button can be used to change all the features with LabelOn value set to Yes to be No. This will clear all the labels for the Rivers feature class.
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
                return _this ?? (_this = (Module1)FrameworkApplication.FindModule("LabelLineFeatures_Module"));
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

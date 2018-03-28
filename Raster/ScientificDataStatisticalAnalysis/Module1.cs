//   Copyright 2018 Esri
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

namespace ScientificDataStatisticalAnalysis
{
    /// <summary>
    /// This sample demonstrates how to leverage the raster function template to simplify the statistical analysis workflows for multidimensional data.
    /// The sample includes these functions:
    /// 1. Group raster items in an image service layer using a SQL expression.
    /// 2. Perform statistical calculations on the grouped items from a list of operations.
    /// Supported operations: Majority, Maximum, Mean, Median, Minimum, Minority, Range, StandardDeviation, Sum, and Variety.
    /// </summary>
    /// <remarks>
    /// 1. In Visual Studio click the Build menu. Then select Build Solution.
    /// 2. Create a new ArcGIS Pro project. Insert a new map if one does not exist.
    /// 3. Import the Scientific_data_calculation template to the Project category's Project1 sub-category on Raster Functions pane.
    ///    (Note: The Scientific_data_calculation template is provided with the add-in located in the Visual Studio project folder.) 
    ///    1) Click Raster Functions button on Imagery tab to open the Raster Functions pane. 
    ///    2) On Raster Functions pane, click the Project category. 
    ///    3) Click the Import functions button on the right of the Project1 sub-cagetory (the down arrow button), browse to the template file and add it. 
    ///    4) Save ArcGIS Pro project.
    /// 4. Click on the Add-In tab.
    /// 5. Add a scientific multidimensional image service to the map view. 
    ///    By default, the Scientific_data_calculation template performs calculation on selected variable within a certain time period, 
    ///    therefore Variable and time (i.e.,StdTime) fields in the attribute table are required.   
    ///    Edit the template properties to fit your own data format.
    /// 6. Enter your definition query SQL expression, then click "Enter" on the key board to confirm your editing.
    ///    Note: You can build your query or write a SQL expression through the layer properties definition query tab, 
    ///    then copy the SQL expression and paste it to the definition query textbox. 
    ///    Example: Variable = 'Water Temperature' And StdZ = 0 And StdTime between date '2018-1-1 0:0:0' And date '2018-3-6 0:0:0'.
    /// 7. Click the drop down arrow on the right of the "Operations" combo box to show the list of supported operations.
    /// 8. Select an operation from the list. This will apply the operation on the image service layer.
    /// 9. To visualize the results better, make sure the image service layer is selected in the Contents pane.
    /// 10. Click the DRA (Dynamic range adjustment) button on the Appearance tab.
    /// 11. Selecting different operations in the dropdown will apply those operations on the image service layer.
    /// 12. To change your query, edit your definition query SQL expression in the textbox, and click "Enter", 
    ///     this will apply the new definition query and selected operation to the layer.
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
            return _this ?? (_this = (Module1)FrameworkApplication.FindModule("ScientificDataStatisticalAnalysis_Module"));
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

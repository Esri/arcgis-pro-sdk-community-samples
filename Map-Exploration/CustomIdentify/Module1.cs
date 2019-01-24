//   Copyright 2019 Esri
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
using ArcGIS.Core.Data;
using ArcGIS.Desktop.Mapping;

namespace CustomIdentify
{  /// <summary>
   /// This sample shows how to author a tool that can be used to identify features and display the content in a pop-up window. 
   /// The identify will also show Relationship Class information if one exists in that feature class. The result will be displayed in a pop-up window using html and dojo.
   /// Note: The identify is authored to query the features in the topmost layer of the map.
   /// </summary>
   /// <remarks>
   /// 1. In Visual Studio click the Build menu. Then select Build Solution.
   /// 2. Click Start button to open ArcGIS Pro.
   /// 3. ArcGIS Pro will open. 
   /// 4. Open a map view. The map should contain a few feature classes, preferably they should also contain Relates to other Feature classes.
   /// 5. Click on the Add-In tab on the ribbon.
   /// 5. Within this tab there is a Custom Identify tool. Click it to activate the tool.
   /// 6. In the map click and drag a box around the features you want to identify.
   /// 7. The pop-up window should display and you should see the results of the identify. The features you selected could have relates to other feature classes. 
   /// This will be displayed also in a hierarchical manner. 
   /// 8. As you click through the pop-up results the content is being generated dynamically for each feature.
   ///
   ///
   ///![UI](screenshots/CustomIdentify.png)
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
                return _this ?? (_this = (Module1)FrameworkApplication.FindModule("CustomIdentify_Module"));
            }
        }
   

        public static Dictionary<string, FeatureClass> GetMapLayersFeatureClassMap(Geodatabase geodatabase)
        {
            Dictionary<string, FeatureClass> lyrFeatureClassMap = new Dictionary<string, FeatureClass>();

            Map map = MapView.Active.Map;
            if (map == null)
                return null;
            var layers = map.GetLayersAsFlattenedList().OfType<FeatureLayer>();

            foreach (var lyr in layers)
            {
                string fc = lyr.GetFeatureClass().GetName();
                FeatureClass featureClass = geodatabase.OpenDataset<FeatureClass>(fc);

                if (featureClass != null)
                    lyrFeatureClassMap.Add(lyr.Name, featureClass);

            }


            return lyrFeatureClassMap;
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

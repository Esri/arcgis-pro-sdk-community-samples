// Copyright 2017 Esri

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

namespace InspectorTool
{
    /// <summary>
    /// This sample demonstrates an Editing tool using the inspector class to modify data.
    /// </summary>
    /// <remarks>
    /// 1. Download the Community Sample data (see under the 'Resources' section for downloading sample data)
    /// 1. Make sure that the Sample data is unzipped in c:\data 
    /// 1. The project used for this sample is 'C:\Data\FeatureTest\FeatureTest.aprx'
    /// 1. In Visual Studio click the Build menu. Then select Build Solution.
    /// 1. Click Start button to open ArcGIS Pro.
    /// 1. ArcGIS Pro will open, select the FeatureTest.aprx project
    /// 1. Activate the EDIT ribbon and click the modify button.
    /// 1. Navigate to the Pro SDK Samples node in the Modify Features pane.
    /// ![UI](Screenshots/InspectorTool.PNG)
    /// 1. Click the 'Select Inspector Tool' and draw rectangle around point features.
    /// ![UI](Screenshots/SelectionPointFeatures.PNG)
    /// 1. Click on an ObjectID in the tree view.
    /// ![UI](Screenshots/SelectedFeaturesTreeViewAndInspector.PNG)
    /// 1. Change the text for the 'Description' attribute.
    /// ![UI](Screenshots/ChangeDescriptionValue.PNG)
    /// </remarks>
    internal class InspectorToolModule : Module
    {
        /// <summary>
        /// This tool shows how to:
        /// * Use an embeddable control for sketch tools
        /// * Use the inspector class to show and edit attributes
        /// </summary>
        ///
        private static InspectorToolModule _this = null;

        /// <summary>
        /// Retrieve the singleton instance to this module here
        /// </summary>
        public static InspectorToolModule Current
        {
            get
            {
                return _this ?? (_this = (InspectorToolModule)FrameworkApplication.FindModule("InspectorTool_Module"));
            }
        }

        #region Overrides
        protected override bool Initialize()
        {
            return true;
        }


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

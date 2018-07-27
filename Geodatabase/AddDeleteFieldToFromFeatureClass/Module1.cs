/*

   Copyright 2018 Esri

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

namespace AddDeleteFieldToFromFeatureClass
{
    /// <summary>
    /// This sample shows how to use Geo Processing to add and remove a fields from a geodatabase.
    /// </summary>
    /// <remarks>
    /// 1. Download the Community Sample data (see under the "Resources" section for downloading sample data).  The sample data contains a project called "FeatureTest.aprx" with data suitable for this sample.  Make sure that the Sample data is unzipped in c:\data and "C:\Data\FeatureTest\FeatureTest.aprx" is available.
    /// 1. Open this solution in Visual Studio 2015.
    /// 1. Click the build menu and select Build Solution.
    /// 1. Click the Start button to open ArCGIS Pro. ArcGIS Pro will open.  
    /// 1. Open the "C:\Data\FeatureTest\FeatureTest.aprx" project.
    /// 1. Open the "Contents Dock Pane" by selecting the View Tab on the Pro ribbon and then clicking the Contents button to open the Contents dock pane".
	/// 1. Click on the Add-in tab and verify that a "FeatureClass Schema" group was added.
    /// 1. Note that the "Add and Delete new field" buttons are disabled as long as no feature layer on the "Contents dock pane" has been selected.
	/// 1. This logic has been implemented in config.daml through the condition="esri_mapping_singleFeatureLayerSelectedCondition" attribute.
	/// ![UI](Screenshots/Screen0.png)
    /// 1. Open the "Attribute table" for the "TestPoints" layer.  This will later allow you to view schema changes in real time.
    /// ![UI](Screenshots/Screen1.png)
    /// 1. Note the field names in the attribute table for the "TestPoints" layer.
    /// 1. Click the "Add a new field" button and check that the field "Alias Name Added Field" was added to the attribute table.
    /// ![UI](Screenshots/Screen2.png)
    /// 1. Click the "Delete the new Field" button and check that the field "Alias Name Added Field" is successfully removed from the attribute table.
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
                return _this ?? (_this = (Module1)FrameworkApplication.FindModule("AddDeleteFieldToFromFeatureClass_Module"));
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

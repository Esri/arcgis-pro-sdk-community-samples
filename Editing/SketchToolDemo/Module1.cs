/*

   Copyright 2019 Esri

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       https://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

   See the License for the specific language governing permissions and
   limitations under the License.

*/
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using System.Threading.Tasks;
using ArcGIS.Desktop.Editing.Events;
using ArcGIS.Desktop.Framework.Dialogs;
using System.Linq;

namespace SketchToolDemo
{
    /// <summary>
    /// This sample demonstrates a 'cut' tool that allows an editor to sketch a line over a set of existing features, if a polygon is intersected by the sketch line, the polygon will be cut by the sketched line.  
    /// </summary>
    /// <remarks>
    /// 1. Download the Community Sample data (see under the 'Resources' section for downloading sample data)
    /// 1. Make sure that the Sample data is unzipped in c:\Data
    /// 1. Before you run the sample verify that the project C:\Data\FeatureTest\FeatureTest.aprx" is present since this is required to run the sample.
    /// 1. In Visual Studio click the Build menu. Then select Build Solution.
    /// 1. Launch the debugger to open ArcGIS Pro.
    /// 1. ArcGIS Pro will open. 
    /// 1. Open the "C:\Data\FeatureTest\FeatureTest.aprx" project.
    /// 1. Click on the Edit tab on the ribbon and select the 'CutTool' from the 2D edit tool gallery. 
	/// ![UI](Screenshots/Screen1.png)
    /// 1. Sketch a line across an existing polygon.
    /// ![UI](Screenshots/Screen2.png)
    /// 1. Complete the sketch using a double click.
    /// ![UI](Screenshots/Screen3.png)
    /// 1. Verify that the polygon was split properly.
    /// ![UI](Screenshots/Screen4.png)
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
                return _this ?? (_this = (Module1)FrameworkApplication.FindModule("SketchToolDemo_Module"));
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

protected override bool Initialize()
{
    // subscribe to the completed edit operation event
    EditCompletedEvent.Subscribe(ReportNumberOfRowsChanged);

    return true;
}


        #endregion Overrides

/// <summary>
/// Method containing actions as the result of the EditCompleted (the operation) event.
/// This method reports the total number of changed rows/features.
/// </summary>
/// <param name="editArgs">Argument containing the layers where edits occurred and what types of
/// changes.</param>
/// <returns></returns>
private Task<bool> ReportNumberOfRowsChanged(EditCompletedEventArgs editArgs)
{
    // get the dictionary containing the modifies on the current feature 
    // operation 
    var editChanges = editArgs.Modifies;

    // use this variable to store the total number of modifies
    int countOfModifies = editChanges.ToDictionary().Values.Sum(list => list.Count);

    if (countOfModifies > 0)
        MessageBox.Show($"{countOfModifies.ToString()} features changed");
    else
        MessageBox.Show("The current edit operation did not contain any row/feature modification.");

    return Task.FromResult(true);
}
    }
}

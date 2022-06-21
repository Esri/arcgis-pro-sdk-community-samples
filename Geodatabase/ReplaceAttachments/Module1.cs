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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using System.Threading.Tasks;

namespace ReplaceAttachments
{
    /// <summary>
    /// This is a sample to update attachments with a given name
    /// with new attachment data, in all Tables/FeatureClasses 
    /// related to the Table/FeatureClass associated with the
    /// layer selected
    /// </summary>
    /// <remarks>    
    /// 1. Download the Community Sample data (see under the 'Resources' section for downloading sample data).  The sample data contains a folder called 'C:\Data\Working with Core Geometry and Data' with sample data required for this solution.  Make sure that the Sample data is unzipped in c:\data and "C:\Data\Working with Core Geometry and Data" is available.
    /// 1. In Visual Studio click the Build menu. Then select Build Solution.
    /// 1. Click Start button to debug ArcGIS Pro.
    /// 1. In ArcGIS Pro open the Project called "Workshop.aprx" in the "C:\Data\Working with Core Geometry and Data" folder.  This project and data is required because it contains data that is attachment enabled.
	/// 1. Use the ArcGIS Pro "Explore" button and identify the polygon overlaying part of Wyoming. 
    /// ![UI](Screenshots/Screen1.png)
	/// 1. Note the attachment called 'redlands.png' which is displayed below the tabular data in the identify layer popup.
    /// 1. Click the 'Show Replace Attachments Addin' button to show the "Replace Attachments" dockpane.
    /// ![UI](Screenshots/Screen2.png)
    /// 1. Use the pull-downs on the "Replace Attachments" dockpane to make selections as shown on the screen below.
	/// ![UI](Screenshots/Screen3.png)
    /// 1. Click on the "File Selection" Button on the right of "Path of the New Attachment File" and select the image file "c:\data\Redlands.png"
    /// ![UI](Screenshots/Screen4.png)
    /// 1. Click the "Go" button to replace all attachments of the given name with the new attachment data.
	/// 1. Use the ArcGIS Pro "Explore" button and identify the polygon 
	/// 1. Note the changed attachment
    /// ![UI](Screenshots/Screen5.png)
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
                return _this ?? (_this = (Module1)FrameworkApplication.FindModule("ReplaceAttachments_Module"));
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

        /// <summary>
        /// Generic implementation of ExecuteCommand to allow calls to
        /// <see cref="FrameworkApplication.ExecuteCommand"/> to execute commands in
        /// your Module.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        protected override Func<Task> ExecuteCommand(string id)
        {

            //TODO: replace generic implementation with custom logic
            //etc as needed for your Module
            var command = FrameworkApplication.GetPlugInWrapper(id) as ICommand;
            if (command == null)
                return () => Task.FromResult(0);
            if (!command.CanExecute(null))
                return () => Task.FromResult(0);

            return () =>
            {
                command.Execute(null); // if it is a tool, execute will set current tool
                return Task.FromResult(0);
            };
        }
        #endregion Overrides

    }
}

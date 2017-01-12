//   Copyright 2014 Esri
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
using System.Windows;

namespace ControlStyles
{
    /// <summary>
    /// This sample demonstrates how to create UI elements in your Add-in that have the ArcGIS Pro "look and feel". 
    /// For example, when you load this sample you will be able to get the xaml code to create a "Esri Simple Button".  
    /// Xaml code for buttons, check boxes, colors can be obtained from this sample. 
    /// </summary>
    /// <remarks>
    /// 1. In Visual Studio click the Build menu. Then select Build Solution.
    /// 2. Click Start button to open ArcGIS Pro.
    /// 3. ArcGIS Pro will open. Click on the new UI Design tab created by this add-in. Two buttons are created on the ribbon in a ArcGIS Pro UI Design group - Control Styles and Colors. 
    /// ![UI](screenshots/UIDesign.png)
    /// 4. Control Styles button: Clicking this button launches the ArcGIS Pro Control Styles dockpane. Select the control you are interested in to view its available styles. You can then click on the Copy button next to the Xaml code for that style to paste into your add-in.
    /// ![Controls](screenshots/controls.png)
    /// 5. Colors button: Clicking this button launches the ArcGIS Pro Colors dockpane. Select any of the Esri colors available in the gallery. You can then click on the Copy button next to the Xaml code for that color to paste into your add-in.
    /// ![Colors](screenshots/colors.png)
    /// 6. Make sure when working with styles that your look and feel supports the ArcGIS Pro Dark Theme as well as the Default Theme.
    /// ![Colors](screenshots/DarkTheme.png)
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
                return _this ?? (_this = (Module1)FrameworkApplication.FindModule("ControlStyles_Module"));
            }
        }
    }


    public static class Utils
    {
        //public static IEnumerable<T> Yield<T>(this T item)
        //{
        //    yield return item;
        //}

        /// <summary>
        /// Represents the Copy to Clipboard method
        /// </summary>
        public static void CopyStringToClipboard(string clipBoardString)
        {
            Clipboard.SetText(clipBoardString);
        }

    }
}

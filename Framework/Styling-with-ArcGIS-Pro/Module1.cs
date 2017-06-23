//   Copyright 2017 Esri
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
    /// Xaml code for buttons, check boxes, DataGrids, TextBlock styles can be obtained from this sample. 
    /// </summary>
    /// <remarks>
    /// 1. In Visual Studio click the Build menu. Then select Build Solution.
    /// 1. This solution is using the AvalonEdit and Extended.Wpf.Toolkit Nugets. If needed, you can install the Nugets from the "Nuget Package Manager Console" by using this script: "Install-Package AvalonEdit" and "Install-Package Extended.Wpf.Toolkit".
    /// 1. Click Start button to open ArcGIS Pro.
    /// 1. ArcGIS Pro will open. Click on the new UI Design tab created by this add-in. Four buttons are created on the ribbon in an ArcGIS Pro UI Design group - Control Styles, TextBlock Styles, Backstage Styles and ProWindow Styles. 
    /// ![UI](screenshots/UIDesign.png)
    /// 1. Control Styles button: Clicking this button launches the ArcGIS Pro Control Styles dockpane. Select the control you are interested in to view its available styles. You can then click on the Copy button next to the Xaml code for that style to paste into your add-in.
    /// ![Controls](screenshots/controls.png)
    /// 1. TextBlock Styles button: Clicking this button launches the ArcGIS Pro TextBlock Styles dockpane. You can see the various TextBlock styles available with Pro.
    /// ![TextBlock](screenshots/TextBlock.png)
    /// 1. Backstage Styles button: Clicking this button will launch a Backstage page that lists the backstage TextBlock styles provided in Pro.
    /// ![Backstage](screenshots/Backstage.png)
    /// 1. ProWindow Styles button: Clicking this button will launch a ProWindow that lists the dialog TextBlock styles provided in Pro.
    /// ![ProWindow](screenshots/ProWindow.png)
    /// 1. Make sure when working with styles that your look and feel supports the ArcGIS Pro Dark Theme as well as the Default Theme.
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

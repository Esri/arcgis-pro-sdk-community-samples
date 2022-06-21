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

using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;

namespace Localization
{
    /// <summary>
    /// This localization sample shows how to 'Globalize' an add-in by providing an add-in with 
    /// text translated into different languages and showing support for different regions (locale).
    /// A ProTutorial based on this sample is also available.
    /// </summary>
    /// <remarks>
    /// 1. In Visual Studio click the Build menu. Then select Build Solution.
    /// 2. Click Start button to open ArcGIS Pro.
    /// 3. Open the "Add-ins tab" on the ArcGIS Pro ribbon to see the sample button and dockpane in English.  Close ArcGIS Pro.
    /// 
    /// __4. Testing other languages__: in order to test other languages you need to install the proper language pack in ArcGIS Pro first.  Once you have additional languages installed you can change ArcGIS Pro's default language in the ArcGIS Pro Options dialog: 
    /// 
    /// In order to view this sample you have to install the German language pack for ArcGIS Pro and Windows.  Change the language option to German and restart ArcGIS Pro.  Open the 'Registrierkarte Add-in' tab and verify that the language of you add-in is correct.
    /// ![package](Images/Localization/Test4.png)
    /// 
    /// Please note that the date and the currency field does not reflect he language change.  The reason is that their settings are not defined by the language but instead by the region.
    /// 
    /// __5. Testing other region settings__: in order to change your region please use the Windows control panel's Region settings dialog and change the format to 'German' (please note that you might have to install a language pack for this to work):  
    /// ![package](Images/Localization/Region.png)
    /// 
    /// After you change your region setting, debug your add-in (with the language setting for German still in place) and verify that the date and currency columns are now defined consistently with the region settings of Windows. 
    /// ![package](Images/Localization/Test5.png)
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
                return _this ?? (_this = (Module1)FrameworkApplication.FindModule("Localization_Module"));
            }
        }
    }
}

/*

   Copyright 2017 Esri

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
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using Licensing.UI;

namespace Licensing {

    /// <summary>
    /// Show how to make a "Configurable Extension" that can be added to the Pro Backstage Licensing Tab
    /// </summary>
    /// <remarks>
    /// At 10.x, add-ins implemented the IExtensionConfig interface to create a "configurable extension".
    /// A configurable extension in 10.x is listed on the ArcMap Extensions dialog box where users can toggle its enabled state on or off. Configurable extensions can execute their own proprietary licensing logic to determine their enabled state within their respective IExtensionConfig implementations.  
    /// In ArcGIS Pro, the configurable extension mechanism is likewise supported. Add-ins that implement  the configurable extension pattern in Pro are shown on the licensing tab on the ArcGIS Pro application backstage in the list of "External Extensions".  
    /// - When a user attempts to enable a configurable extension from backstage, the 3rd party developer can execute custom licensing code via IExtensionConfig (same as at 10x) to determine whether or not the enabling action is authorized.
    /// - This Add-in mimics implementation of a proprietary licensing scheme that is activated whenever the user attempts to enable the Add-in from the Pro Licensing tab. When the user clicks the "Enabled" check box (for the sample's external extension list entry on the licensing tab), a pop-up prompts the user for a valid Product ID to enable the extension. A valid Product ID will be any number that is divisible by 2. If a valid id is provided the extension is enabled. If a valid id is not provided then the Add-in remains disabled.
    /// ![UI](Screenshots/Screen1.png)
    /// - The extension state (Enabled, Disabled) is propagated to the UI and functionality of the Add-in (in this case a button that does a feature select) via a custom condition. The state that controls the condition is activated and deactivated in conjunction with the Enabled, Disabled extension state of the Add-in.
    /// ![UI](Screenshots/Screen2.png)
    /// - Please also refer to the companion ProGuide at [ProGuide: License Your Add-in](https://github.com/Esri/arcgis-pro-sdk/wiki/ProGuide-License-Your-Add-in) for more information
    /// </remarks>
    internal class Module1 : Module, IExtensionConfig {
        private static Module1 _this = null;
        private static string _authorizationId = "";
        private static ExtensionState _extensionState = ExtensionState.Disabled;

        internal Module1() {
            //TODO - read authorization id from....
            //file, url, etc. as required

            //preset _authorizationID to a number "string" divisible by 2 to have 
            //the Add-in initially enabled
            CheckLicensing(_authorizationId);
        }
        /// <summary>
        /// Retrieve the singleton instance to this module here
        /// </summary>
        public static Module1 Current
        {
            get
            {
                return _this ?? (_this = (Module1)FrameworkApplication.FindModule("Licensing_Module"));
            }
        }

        /// <summary>
        /// The current Authorization ID
        /// </summary>
        internal static string AuthorizationId {
            get {
                return _authorizationId;
            }
            set {
                _authorizationId = value;
            }
        }

        #region IExtensionConfig

        /// <summary>
        /// Implement to override the extensionConfig in the DAML
        /// </summary>
        public string Message {
            get { return "";}
            set{  }
        }

        /// <summary>
        /// Implement to override the extensionConfig in the DAML
        /// </summary>
        public string ProductName {
            get { return ""; }
            set { }
        }

        /// <summary>
        /// Handle enable/disable request from the UI
        /// </summary>
        public ExtensionState State
        {
            get {
                return _extensionState;
            }
            set {
                if (value == ExtensionState.Unavailable) {
                    return; //Leave the state Unavailable
                }
                else if (value == ExtensionState.Disabled) {
                    _extensionState = value;
                }
                else {
                    //check if we allow Enabling of our Add-in
                    if (!CheckLicensing(_authorizationId)) {
                        var regWindow = new RegistrationWindow();
                        regWindow.ShowDialog();
                    }
                }
            }
        }

        #endregion

        /// <summary>
        /// Execute our authorization check
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        internal static bool CheckLicensing(string id) {
            int val = 0;
            if (string.IsNullOrEmpty(id) || !Int32.TryParse(id, out val)) {
                val = -1; //bogus
            }
            //Any number divisible by 2
            if (val % 2 == 0) {
                FrameworkApplication.State.Activate("acme_module_licensed");
                _extensionState = ExtensionState.Enabled;
            }
            else {
                FrameworkApplication.State.Deactivate("acme_module_licensed");
                _extensionState = ExtensionState.Disabled;
            }
            return val % 2 == 0;
        }

        #region Overrides
        /// <summary>
        /// Called by Framework when ArcGIS Pro is closing
        /// </summary>
        /// <returns>False to prevent Pro from closing, otherwise True</returns>
        protected override bool CanUnload() {
            //TODO - add your business logic
            //return false to ~cancel~ Application close
            return true;
        }

        #endregion Overrides

    }
}

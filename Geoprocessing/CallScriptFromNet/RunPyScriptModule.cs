//Copyright 2014 Esri

//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at

//       http://www.apache.org/licenses/LICENSE-2.0

//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific .cs governing permissions and
//   limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;

namespace CallScriptFromNet
{
    /// <summary>
    /// This Button add-in, when clicked, calls a Python script, reads the text stream output from the script and uses it. From simplicity, output text is sent to windows messagebox.
    /// However, you can execute complex Python code in the Python script and call the script from within the button. 
    /// </summary>
    /// <remarks>
    /// 1. This solution file includes an example python script named test1.py
    /// 1. This sample also requires that you install the recommended version of Python for ArcGIS Pro and add python.exe to you path
    /// 1. Open the 'RunPyScriptButton' class and update the path to test1.py to point to the sample script file in your solution
    /// 1. Build the solution - make sure it compiles successfully.
    /// 1. Open ArcGIS Pro - go to the ADD-IN Tab, find RunPyScriptButton in Group 1 group.
    /// 1. Click on the button - wait few seconds - a message box will show up with a message of "Hello - this message is from a TEST Python script"
    /// ![UI](Screenshots/Screen.png)
    /// </remarks>
    internal class RunPyScriptModule : Module
    {
        private static RunPyScriptModule _this = null;

        /// <summary>
        /// Retrieve the singleton instance to this module here
        /// </summary>
        public static RunPyScriptModule Current
        {
            get
            {
                return _this ?? (_this = (RunPyScriptModule)FrameworkApplication.FindModule("CallScriptFromNet_Module"));
            }
        }
    }
}

//Copyright 2019 Esri

//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at

//       https://www.apache.org/licenses/LICENSE-2.0

//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific .cs governing permissions and
//   limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;

namespace CallScriptFromNet
{
    /// <summary>
    /// This Button add-in, when clicked, calls a Python script, reads the text stream
    /// output from the script and uses it. From simplicity, output text is sent to windows messagebox.
    /// However, you can execute complex Python code in the Python script and call the script
    /// from within the button. 
    /// </summary>
    /// <remarks>
    /// 1. Make sure there is a Python script under ..\CallScriptFromNet\CallScriptFromNet folder.
    /// 2. Example script included in that folder is named test.py
    /// 3. Build the solution - make sure it compiles successfully.
    /// 4. Open ArcGIS Pro - go to Add-In Tab, find RunPyScriptButton in Group 1 group.
    /// 5. Click on the button - wait few seconds - a message box will show up with a message of "Hello - this message is from a TEST Python script"
    /// </remarks>
    internal class RunPyScriptButton : Button
    {
        /// <summary>
        /// Clicking on the button start a process with python and path to script as command.
        /// </summary>
        protected override void OnClick()
        {
            // TODO: fix the path to test1.py so that it points to the proper file location

            var pathProExe = System.IO.Path.GetDirectoryName((new System.Uri(Assembly.GetEntryAssembly().Location)).AbsolutePath);
            if (pathProExe == null) return;
            pathProExe = Uri.UnescapeDataString(pathProExe);
            pathProExe = System.IO.Path.Combine(pathProExe, @"Python\envs\arcgispro-py3");
            System.Diagnostics.Debug.WriteLine(pathProExe);
            var pathPython = System.IO.Path.GetDirectoryName((new System.Uri(Assembly.GetExecutingAssembly().Location)).AbsolutePath);
            if (pathPython == null) return;
            pathPython = Uri.UnescapeDataString(pathPython);
            System.Diagnostics.Debug.WriteLine(pathPython);

            var myCommand = string.Format(@"/c """"{0}"" ""{1}""""",
                System.IO.Path.Combine(pathProExe, "python.exe"),
                System.IO.Path.Combine(pathPython, "test1.py"));
            System.Diagnostics.Debug.WriteLine(myCommand);
            var procStartInfo = new System.Diagnostics.ProcessStartInfo("cmd", myCommand);

            procStartInfo.RedirectStandardOutput = true;
            procStartInfo.RedirectStandardError = true;
            procStartInfo.UseShellExecute = false;

            procStartInfo.CreateNoWindow = true;

            System.Diagnostics.Process proc = new System.Diagnostics.Process();
            proc.StartInfo = procStartInfo;
            proc.Start();

            string result = proc.StandardOutput.ReadToEnd();
            string error = proc.StandardError.ReadToEnd();
            if (!string.IsNullOrEmpty(error)) result += string.Format("{0} Error: {1}", result, error);

            System.Windows.MessageBox.Show(result);
        }
    }
}

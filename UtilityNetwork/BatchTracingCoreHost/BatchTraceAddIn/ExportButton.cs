/*

   Copyright 2026 Esri

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
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using BatchTracingCoreCommon.Classes;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;

namespace BatchTraceAddIn
{
    internal class ExportButton : Button
    {
        protected override void OnClick()
        {
            var ofd = new OpenFileDialog() { Filter = "JSON File (*.json)|*.json" };
            var dialogResponse = ofd.ShowDialog();
            if (!dialogResponse.HasValue || !dialogResponse.Value)
                return;

            IDictionary<string, object> configuration = null;
            try
            {
                configuration = Helpers.LoadConfiguration(ofd.FileName);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

            if (configuration == null)
                return;

            if (!configuration.TryGetValue("type", out object analysisType) || analysisType is null | analysisType.ToString() != "Trace")
            {
                MessageBox.Show("Invalid Analysis Type", "Must select a batch trace configuration with the \"Trace\" analysis type.", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            try
            {
                var startTime = DateTime.Now;

                var task = QueuedTask.Run(() =>
                    BatchTrace.Execute("Batch Export", configuration, false, true));
                task.Wait();

                var timeDiff = DateTime.Now - startTime;
                MessageBox.Show("Success", string.Format("Trace complete: {0} total seconds", timeDiff.TotalSeconds), MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                MessageBox.Show("Error Tracing", ex.Message, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}

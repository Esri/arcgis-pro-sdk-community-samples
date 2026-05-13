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
using System.IO;
using System.Windows;
using MessageBox = System.Windows.MessageBox;

namespace BatchTraceAddIn
{
    internal class BatchTraceButton : Button
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
                MessageBox.Show(ex.Message, "Error Loading Configuration", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (configuration == null)
                return;

            StreamWriter writer = null;
            try
            {
                var baseFileName = Path.GetFileNameWithoutExtension(ofd.FileName);
                var saveFileDialog = new SaveFileDialog { Filter = "Log file (*.log)|*.log", Title = "Output logfile", OverwritePrompt = true, FileName = string.Format("{0}.log", baseFileName) };
                var saveResponse = saveFileDialog.ShowDialog();
                if (saveResponse.HasValue && saveResponse.Value)
                {
                    var outputStream = saveFileDialog.OpenFile();
                    writer = new StreamWriter(outputStream);
                    writer.AutoFlush = true;
                    Console.SetOut(writer);
                }

                try
                {
                    var startTime = DateTime.Now;

                    var task = QueuedTask.Run(() =>
                        Helpers.PerformAnalysis(ofd.FileName));
                    task.Wait();

                    var timeDiff = DateTime.Now - startTime;
                    MessageBox.Show("Complete", string.Format("Trace complete: {0} total seconds", timeDiff.TotalSeconds), MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                    MessageBox.Show(ex.Message, "Error Tracing", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            finally
            {
                var standardOutput = new StreamWriter(Console.OpenStandardOutput());
                standardOutput.AutoFlush = true;
                Console.SetOut(standardOutput);

                if (writer != null)
                {
                    writer.Close();
                    writer.Dispose();
                    writer = null;
                }
            }
        }
    }
}

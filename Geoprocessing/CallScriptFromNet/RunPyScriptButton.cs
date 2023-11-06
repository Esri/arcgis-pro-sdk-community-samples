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
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Data.DDL;
using ArcGIS.Core.Data.UtilityNetwork.Trace;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;

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
    protected override async void OnClick()
    {
      try
      {
        RunPythonWithFeedbackViewModel.Show();
        RunPythonWithFeedbackViewModel.ClearStatusMessage();

        // we use the first polygon layer from our Map
        var polygonLayer = MapView.Active?.Map?.GetLayersAsFlattenedList().OfType<FeatureLayer>()
                            .Where(lyr => (lyr.ShapeType == esriGeometryType.esriGeometryPolygon) && lyr.IsVisible).FirstOrDefault();
        if (polygonLayer == null)
        {
          MessageBox.Show("The active Map has to contain one polygon layer that is used for the buffer operation");
          return;
        }
        // use propy.bat to run the python scrip file
        // this file is always under Pro's bin folder: Python\Scripts\propy.bat
        var pathProBin = System.IO.Path.GetDirectoryName((new System.Uri(Assembly.GetEntryAssembly().Location)).AbsolutePath);
        if (pathProBin == null) return;
        pathProBin = Uri.UnescapeDataString(pathProBin);
        var propyScriptPath = System.IO.Path.Combine(pathProBin, @"Python\Scripts\propy.bat");
        System.Diagnostics.Debug.WriteLine(propyScriptPath);

        // fix the path to test1.py so that it points to the proper file location
        // since test1.py is add-in 'content' there is a copy of the file in the
        // executable location of the add-in assembly
        var pythonScriptPath = System.IO.Path.GetDirectoryName((new System.Uri(Assembly.GetExecutingAssembly().Location)).AbsolutePath);
        if (pythonScriptPath == null) throw new Exception("Unable to get Assembly.GetExecutingAssembly().Location");
        pythonScriptPath = Uri.UnescapeDataString(pythonScriptPath);
        System.Diagnostics.Debug.WriteLine(pythonScriptPath);
        var script = System.IO.Path.Combine(pythonScriptPath, "test1.py");
        if (!System.IO.File.Exists(script)) throw new Exception($@"Unable to find the python script file here: {script}");
        
        // for python scripting we need to pass in the complete
        // path for all data 
        var connection = await QueuedTask.Run(() =>
        {
          System.Diagnostics.Trace.WriteLine(polygonLayer.GetDataConnection().GetType());
          return polygonLayer.GetDataConnection() as CIMStandardDataConnection;
        });
        if (connection == null) throw new Exception($@"The code only supports file Geodatabases for you selected polygon layer [{polygonLayer.Name}]");
        var baseData = connection.WorkspaceConnectionString.Replace("DATABASE=", "");
        var polygonLayerPath = System.IO.Path.Combine(baseData, connection.Dataset);
        var inputLayer = System.IO.Path.Combine(baseData, $@"{polygonLayer.Name}");
        var outputLayer = System.IO.Path.Combine(baseData, $@"{polygonLayer.Name}_Buffer");
        System.Diagnostics.Debug.WriteLine(outputLayer);

        RunPyScriptModule.RunPythonWithFeedbackViewModel.InLayer = inputLayer;
        RunPyScriptModule.RunPythonWithFeedbackViewModel.OutLayer = outputLayer;

        DeleteFC(outputLayer);
        var myArguments = $@"""{script}"" ""{polygonLayerPath}"" ""{outputLayer}""";
        if (RunPyScriptModule.RunPythonWithFeedbackViewModel != null)
        {
          RunPyScriptModule.RunPythonWithFeedbackViewModel.CommandLine =
            $@"""{propyScriptPath}"" {myArguments}";
        }
        var process = new RunProcess();
        var processOutcome = process.RunProcessGrabOutput(propyScriptPath,
          myArguments, pathProBin);
        
          if (!string.IsNullOrEmpty(processOutcome.Output))
          {
            RunPythonWithFeedbackViewModel.AddStatusMessage(processOutcome.Output);
          }
          if (!string.IsNullOrEmpty(processOutcome.Error))
          {
            RunPythonWithFeedbackViewModel.AddStatusMessage(processOutcome.Error);
          }
          if (processOutcome.ErrCode != 0)
          {
            RunPythonWithFeedbackViewModel.AddStatusMessage($@"Script returned error code: {processOutcome.ErrCode}");
          }
      }
      catch (Exception ex)
      {
        MessageBox.Show (ex.ToString ());
        RunPythonWithFeedbackViewModel.AddStatusMessage(ex.ToString ());
      }
    }

    private async void DeleteFC(string polygonLayerPath)
    {
      var gdb = System.IO.Path.GetDirectoryName(polygonLayerPath);
      var fcName = System.IO.Path.GetFileName(polygonLayerPath);
      var fileGeodatabase = new FileGeodatabaseConnectionPath(new Uri(gdb));
      await QueuedTask.Run(() =>
      {
        using var geodatabase = new Geodatabase(fileGeodatabase);
        // Use the geodatabase.
        IReadOnlyList<FeatureClassDefinition> fcDefinitions = geodatabase.GetDefinitions<FeatureClassDefinition>();
        foreach (FeatureClassDefinition fcDef in fcDefinitions)
        {
          if (fcDef.GetName().Equals(fcName))
          {
            DeleteFeatureClass(geodatabase, geodatabase.OpenDataset<FeatureClass>(fcName));
          }
        }
      });
    }

    public static bool DeleteFeatureClass(Geodatabase geodatabase, FeatureClass featureClass)
    {
      // Create a FeatureClassDescription object
      FeatureClassDescription featureClassDescription = new(featureClass.GetDefinition());

      // Create a SchemaBuilder object
      SchemaBuilder schemaBuilder = new(geodatabase);

      // Add the deletion fo the feature class to our list of DDL tasks
      schemaBuilder.Delete(featureClassDescription);

      // Execute the DDL
      bool success = schemaBuilder.Build();
      return success;
    }

  }
}

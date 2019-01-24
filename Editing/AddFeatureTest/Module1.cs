/*

   Copyright 2019 Esri

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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Threading.Tasks;
using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Extensions;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Desktop.Core.Geoprocessing;
using System.Threading;

namespace AddFeatureTest
{
  /// <summary>
	/// This sample shows how to create new featureclasses with attributes and also how to add new features to those newly created featureclasses.
	/// </summary>
	/// <remarks>
	/// 1. In Visual studio rebuild the solution.
	/// 1. Debug the add-in by clicking the "Start" button.
	/// 1. ArcGIS Pro opens, select a new Map project
	/// 1. Zoom the new map into your local neighborhood, because the newly created features will be in close proximity.
	/// ![UI](Screenshots/Screen1.png)
	/// 1. Click the "Create Featureclasses" button to create a point and a polygon feature class.  Both are added to your map as well. 
	/// ![UI](Screenshots/Screen2.png)
	/// 1. Click the "Add Features" button to add 5 feature to each newly create feature class. 
	/// ![UI](Screenshots/Screen3.png)
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
        return _this ?? (_this = (Module1)FrameworkApplication.FindModule("AddFeatureTest_Module"));
      }
    }

    public static string PointFcName = "Pnt1";
    public static string PolyFcName = "Poly1";

    internal static async Task CreateFcWithAttributesAsync(string fcName, EnumFeatureClassType fcType)
    {
      // Create feature class T1
      await Module1.CreateFeatureClass(fcName, fcType);
      // double check to see if the layer was added to the map
      var fcLayer = MapView.Active.Map.GetLayersAsFlattenedList().Where((l) => l.Name == fcName).FirstOrDefault() as BasicFeatureLayer;
      if (fcLayer == null)
      {
        MessageBox.Show($@"Unable to find {fcName} in the active map");
        return;
      }
      {
        // add a description field to the layer
        var dataSource = await Module1.GetDataSource(fcLayer);
        System.Diagnostics.Debug.WriteLine($@"{dataSource} was found ... adding a Field");
        await
          Module1.
            ExecuteAddFieldToolAsync(fcLayer,
            new KeyValuePair<string, string>("Description", "Desc."), "Text", 50);
      }
    }

    public enum EnumFeatureClassType
    {
      POINT,
      MULTIPOINT,
      POLYLINE,
      POLYGON
    }

    /// <summary>
    /// Create a feature class in the default geodatabase of the project.
    /// </summary>
    /// <param name="featureclassName">Name of the feature class to be created.</param>
    /// <param name="featureclassType">Type of feature class to be created. Options are:
    /// <list type="bullet">
    /// <item>POINT</item>
    /// <item>MULTIPOINT</item>
    /// <item>POLYLINE</item>
    /// <item>POLYGON</item></list></param>
    /// <returns></returns>
    public static async Task CreateFeatureClass(string featureclassName, EnumFeatureClassType featureclassType)
    {
      List<object> arguments = new List<object>
      {
        // store the results in the default geodatabase
        CoreModule.CurrentProject.DefaultGeodatabasePath,
        // name of the feature class
        featureclassName,
        // type of geometry
        featureclassType.ToString(),
        // no template
        "",
        // no z values
        "DISABLED",
        // no m values
        "DISABLED"
      };
      await QueuedTask.Run(() =>
      {
        // spatial reference
        arguments.Add(SpatialReferenceBuilder.CreateSpatialReference(3857));
      });
      IGPResult result = await Geoprocessing.ExecuteToolAsync("CreateFeatureclass_management", Geoprocessing.MakeValueArray(arguments.ToArray()));
    }

    public static async Task<string> GetDataSource(BasicFeatureLayer theLayer)
    {
      try
      {
        return await ArcGIS.Desktop.Framework.Threading.Tasks.QueuedTask.Run(() =>
        {
          var inTable = theLayer.Name;
          var table = theLayer.GetTable();
          var dataStore = table.GetDatastore();
          var workspaceNameDef = dataStore.GetConnectionString();
          var workspaceName = workspaceNameDef.Split('=')[1];
          var fullSpec = System.IO.Path.Combine(workspaceName, inTable);
          return fullSpec;
        });
      }
      catch (Exception ex)
      {
        MessageBox.Show(ex.ToString());
        return string.Empty;
      }
    }

    public static async Task<string> ExecuteAddFieldToolAsync(
      BasicFeatureLayer theLayer, 
      KeyValuePair<string, string> field, 
      string fieldType, int? fieldLength = null, 
      bool isNullable = true)
    {
      return await QueuedTask.Run(() =>
       {
         try
         {
           var inTable = theLayer.Name;
           var table = theLayer.GetTable();
           var dataStore = table.GetDatastore();
           var workspaceNameDef = dataStore.GetConnectionString();
           var workspaceName = workspaceNameDef.Split('=')[1];

           var fullSpec = System.IO.Path.Combine(workspaceName, inTable);
           System.Diagnostics.Debug.WriteLine($@"Add {field.Key} from {fullSpec}");

           var parameters = Geoprocessing.MakeValueArray(fullSpec, field.Key, fieldType.ToUpper(), null, null,
                 fieldLength, field.Value, isNullable ? "NULABLE" : "NON_NULLABLE");
           var cts = new CancellationTokenSource();
           var results = Geoprocessing.ExecuteToolAsync("management.AddField", parameters, null, cts.Token,
                 (eventName, o) =>
                 {
                   System.Diagnostics.Debug.WriteLine($@"GP event: {eventName}");
                 });
           var isFailure = results.Result.IsFailed || results.Result.IsCanceled;
           return !isFailure ? "Failed" : "Ok";
         }
         catch (Exception ex)
         {
           MessageBox.Show(ex.ToString());
           return ex.ToString();
         }
       });
    }

    public static Task<bool> FeatureClassExistsAsync(string fcName)
    {
      return QueuedTask.Run(() =>
      {
        try
        {
          using (Geodatabase projectGDB = new Geodatabase(new FileGeodatabaseConnectionPath(new Uri(Project.Current.DefaultGeodatabasePath))))
          {
            using (FeatureClass fc = projectGDB.OpenDataset<FeatureClass>(fcName))
            {
              return fc != null;
            }
          }
        }
        catch (Exception ex)
        {
          System.Diagnostics.Debug.WriteLine ($@"FeatureClassExists Error: {ex.ToString()}");
          return false;
        }
      });
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

    #endregion Overrides

  }
}

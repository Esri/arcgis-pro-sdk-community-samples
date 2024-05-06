/*

   Copyright 2024 Esri

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
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DatagridMultiSelect
{
	/// <summary>
	/// This sample shows how to use the WPF DataGrid's 'Multi-Select' feature to select multiple rows from a DataGrid in MVVM.  The DataGrid is displayed in a DockPane.
  /// </summary>
	/// <remarks>
	/// 1. Download the Community Sample data (see under the "Resources" section for downloading sample data).  The sample data contains a project called "FeatureTest.aprx" with data suitable for this sample.  Make sure that the Sample data is unzipped in c:\data and "C:\Data\FeatureTest\FeatureTest.aprx" is available.
	/// 1. Open this solution in Visual Studio.
	/// 1. Click the build menu and select Build Solution.
	/// 1. Launch the debugger to open ArCGIS Pro. ArcGIS Pro will open.  
	/// 1. Open the "C:\Data\FeatureTest\FeatureTest.aprx" project.
  /// 1. Open the Add-in Tab and click on the 'Show DataGrid Dockpane' button to open the DataGrid Dockpane.
  /// 1. Click the 'Select by Rectangle' button and draw a rectangle around the features in the map.  You will see the selected features listed in the DataGrid Dockpane.
  /// ![UI](Screenshots/Screenshot1.png)
	/// 1. Now use the DataGrid's 'Multi-Select' feature to select multiple features from different layers.  You will see the selected features listed in the DataGrid Dockpane by using the 'Control key' while click rows to be selected.
	/// ![UI](Screenshots/Screenshot2.png)
	/// 1. The multi selection result is displayed on the bottom of the Dockpane.
	/// </remarks>
	internal class Module1 : Module
  {
    private static Module1 _this = null;

    public static readonly object _lock_feature_data = new();
    public ObservableCollection<FeatureData> FeatureData { get; private set; } = new ObservableCollection<FeatureData>();
    public object LockFeatureData => _lock_feature_data;

    /// <summary>
    /// Retrieve the singleton instance to this module here
    /// </summary>
    public static Module1 Current => _this ??= (Module1)FrameworkApplication.FindModule("DatagridMultiSelect_Module");

    #region Overrides

    protected override bool Initialize()
    {
      ArcGIS.Desktop.Mapping.Events.MapSelectionChangedEvent.Subscribe((args) =>
      {
        var sel = args.Selection;
        var ss_dict = sel.ToDictionary();
        lock(_lock_feature_data)
          FeatureData.Clear();
        QueuedTask.Run(() =>
        {
          foreach (var kvp in ss_dict)
          {
            if (kvp.Key is FeatureLayer fl)
            {
              var def = fl.GetDefinition() as CIMFeatureLayer;
              var display_fld_name = def.FeatureTable.DisplayField;
              var display_idx = -1;
              using (var fdef = fl.GetTable().GetDefinition())
              {
                display_idx = fdef.FindField(display_fld_name);
              }
              using (var rc = fl.GetSelection().Search())
              {
                while (rc.MoveNext())
                {
                  var feat = rc.Current as Feature;
                  var feat_data = new FeatureData()
                  {
                    ObjectId = feat.GetObjectID(),
                    DisplayName = feat[display_idx]?.ToString() ?? "null",
                    LayerName = fl.Name,
                    ShapeType = fl.ShapeType.ToString().Replace("esriGeometry","")
                  };
                  lock (_lock_feature_data)
                    FeatureData.Add(feat_data);
                }
              }
            }
          }
        });
      });

      ArcGIS.Desktop.Mapping.Events.ActiveMapViewChangedEvent.Subscribe((args) =>
      {
        if (args.IncomingView == null && args.OutgoingView != null)
        {
          lock (_lock_feature_data)
            FeatureData.Clear();
        }
      });
      return true;
    }
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

  internal class FeatureData
  {
    public long ObjectId { get; set; }
    public string DisplayName { get; set; }
    public string LayerName { get; set; }
    public string ShapeType { get; set; }

    public override string ToString()
    {
      return $"{this.ObjectId}: '{DisplayName}' {LayerName}";
    }
  }
}

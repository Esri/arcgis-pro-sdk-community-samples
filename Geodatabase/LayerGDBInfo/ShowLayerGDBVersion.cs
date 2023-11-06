/*

   Copyright 2023 Esri

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
using ArcGIS.Core.Data.PluginDatastore;
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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LayerGDBInfo
{  
  /// <summary>
  /// Extension methods for feature layer
  /// </summary>
  internal static class FeatureLayerExtensions
  {
    public static GeodatabaseType? GetGeodatabaseType(this BasicFeatureLayer layer, Datastore gdb)
    {
      if (layer.ConnectionStatus == ConnectionStatus.Broken)
        return null;
      //new at 2.3
      if (gdb is PluginDatastore)
      {
        return null;
      }
      //Note shapefile will be "FileSystemDatastore"
      if (gdb is Geodatabase geodatabase)
      {
        return geodatabase.GetGeodatabaseType();
      }
      return null;
    }

    public static RegistrationType? GetRegistrationType(this BasicFeatureLayer layer)
    {
      if (layer.ConnectionStatus == ConnectionStatus.Broken)
        return null;

      RegistrationType? regType = null;
      using (var dataset = layer.GetTable())
      {
        regType = dataset.GetRegistrationType();
      }
      return regType;
    }

  }

  internal class ShowLayerGDBVersion : Button
  {
    protected override async void OnClick()
    {
      try
      {
        await QueuedTask.Run(() =>
        {
          BasicFeatureLayer theLayer = null;
          var selectedLayer = MapView.Active.GetSelectedLayers().OfType<BasicFeatureLayer>().FirstOrDefault();
          if (selectedLayer != null) theLayer = selectedLayer;

          // if we have no selected layer we try to get the first FeatureLayer in the map
          theLayer ??= MapView.Active.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>().FirstOrDefault();
          if (theLayer == null)
            throw new Exception("The Map has to contain at least one FeatureLayer.");

          if (theLayer.ConnectionStatus == ConnectionStatus.Broken)
            throw new Exception($@"The connection to the datasource for {theLayer.Name} is broken, therefore Layer info is not available");

          using var table = theLayer.GetTable();
          using var geodatabase = table.GetDatastore() ?? throw new Exception($@"Feature Layer {theLayer.Name} is not stored in a Geodatabase");

          var gdbType = theLayer.GetGeodatabaseType(geodatabase);
          var regType = theLayer.GetRegistrationType();

          switch (gdbType.Value)
          {
            case GeodatabaseType.LocalDatabase:    
              // Local database Workspace.
              //     Geodatabases that are local to your machine, e.g., a File Geodatabase.
              MessageBox.Show($@"Feature Layer {theLayer.Name} is a local GDB datasource");
              break;
            case GeodatabaseType.RemoteDatabase:
              // Remote database Workspace.
              //     Geodatabases that require a remote connection. e.g., ArcSDE.
              {
                var dbConnectionProps = (DatabaseConnectionProperties)geodatabase.GetConnector();
                var nl = System.Environment.NewLine;
                var dbConnectionDisp = $@"{theLayer.Name}:{nl} Database:{dbConnectionProps.Database}";
                dbConnectionDisp += $@" Instance: {dbConnectionProps.Instance}";
                dbConnectionDisp += $@" DBMS: {dbConnectionProps.DBMS}";
                dbConnectionDisp += $@" RegType: {regType}";
                dbConnectionDisp += $@" Version: {dbConnectionProps.Version}";
                MessageBox.Show(dbConnectionDisp);
              }
              break;
            case GeodatabaseType.FileSystem:
              // File system Workspace.
              //     File-based workspaces. e.g., shapefiles.
              MessageBox.Show($@"Feature Layer {theLayer.Name} is a file system datasource");
              break;
            case GeodatabaseType.Service:
              // Feature service Workspace.
              MessageBox.Show($@"Feature Layer {theLayer.Name} is a service based datasource");
              break;
            case GeodatabaseType.Memory:
              // Memory Workspace.
              MessageBox.Show($@"Feature Layer {theLayer.Name} is a memory datasource");
              break;
            default:
              MessageBox.Show($@"Feature Layer {theLayer.Name} is a plugin datasource");
              break;
          }
        });
      }
      catch (Exception ex)
      {
        MessageBox.Show (ex.Message);
      }
    }
  }
}

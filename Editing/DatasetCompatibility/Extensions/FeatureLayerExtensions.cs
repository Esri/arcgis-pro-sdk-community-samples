/*

   Copyright 2018 Esri

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
using ArcGIS.Core.Data;
using ArcGIS.Core.Data.PluginDatastore;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatasetCompatibility.Extensions
{
  /// <summary>
  /// Extension methods for feature layer
  /// </summary>
  internal static class FeatureLayerExtensions
  {

    public static GeodatabaseType? GetGeodatabaseType(this BasicFeatureLayer layer)
    {
      if (layer.ConnectionStatus == ConnectionStatus.Broken)
        return null;
      GeodatabaseType? gdbType = GeodatabaseType.FileSystem;
      using(var dataset = layer.GetTable())
      {
        using (var gdb = dataset.GetDatastore())
        {
          //new at 2.3
          if (gdb is PluginDatastore)
          {
            return null;
          }
          //Note shapefile will be "FileSystemDatastore"
          if (gdb is Geodatabase)
          {
            gdbType = ((Geodatabase)gdb).GetGeodatabaseType();
          }
        }
      }
      return gdbType;
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

    public static EditOperationType? GetEditOperationType(this BasicFeatureLayer layer)
    {
      if (layer.ConnectionStatus == ConnectionStatus.Broken)
        return null;
      var gdbType = layer.GetGeodatabaseType();
      var regType = layer.GetRegistrationType();

      //Plugin - new at 2.3
      if (gdbType == null)
        return null;
      //FileSystem
      if (gdbType == GeodatabaseType.FileSystem)
        return EditOperationType.Long;
      //LocalDatabase
      if (gdbType == GeodatabaseType.LocalDatabase)
        return EditOperationType.Long;
      //RemoteDatabase, Versioned
      if (gdbType == GeodatabaseType.RemoteDatabase && (
        regType == RegistrationType.Versioned || regType == RegistrationType.VersionedWithMoveToBase))
        return EditOperationType.Long;
      //RemoteDatabase, NonVersioned
      if (gdbType == GeodatabaseType.RemoteDatabase && regType == RegistrationType.Nonversioned)
        return EditOperationType.Short;
      //Service, NonVersioned
      if (gdbType == GeodatabaseType.Service && regType == RegistrationType.Nonversioned)
        return EditOperationType.Short;

      //Service, Versioned, Default
      EditOperationType forBranch = EditOperationType.Long;
      if (gdbType == GeodatabaseType.Service && regType == RegistrationType.Versioned)
      {
        using(var dataset = layer.GetTable())
        using (var gdb = dataset.GetDatastore() as Geodatabase)
        using (var vmgr = gdb.GetVersionManager())
        using (var vers = vmgr.GetCurrentVersion())
        using (var parent = vers.GetParent())
        {
          //non-default supports undo/redo and save/discard. Default does not
          forBranch = parent != null ? EditOperationType.Long : EditOperationType.Short;
        }
      }
      return forBranch;
    }

    
  }
}

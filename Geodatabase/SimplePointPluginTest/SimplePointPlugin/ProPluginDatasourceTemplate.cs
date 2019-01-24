/*

   Copyright 2018 Esri

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
using System.Threading.Tasks;
using ArcGIS.Core.Data;
using ArcGIS.Core.Data.PluginDatastore;
using ArcGIS.Core.Geometry;
using Microsoft.VisualBasic.FileIO;
using ArcGIS.Core.Hosting.Threading.Tasks;
using System.Threading;
using System.Runtime.InteropServices;

namespace SimplePointPlugin
{
  /// <summary>
  /// Implements a custom plugin datasource for reading csv files
  /// </summary>
  /// <remarks>A per thread instance will be created (as needed) by Pro.</remarks>
  public class ProPluginDatasourceTemplate : PluginDatasourceTemplate
  {

    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    [DllImport("kernel32.dll")]
    internal static extern uint GetCurrentThreadId();

    private string _filePath = "";
    private uint _thread_id;

    private Dictionary<string, PluginTableTemplate> _tables;

    /// <summary>
    /// Open the specified workspace
    /// </summary>
    /// <param name="connectionPath">The path to the workspace</param>
    /// <remarks>
    /// .NET Clients access Open via the ArcGIS.Core.Data.PluginDatastore.PluginDatastore class
    /// whereas Native clients (Pro internals) access via IWorkspaceFactory</remarks>
    public override void Open(Uri connectionPath)
    {
 
      if (!System.IO.Directory.Exists(connectionPath.LocalPath))
      {
        throw new System.IO.DirectoryNotFoundException(connectionPath.LocalPath);
      }
      //initialize
      //Strictly speaking, tracking your thread id is only necessary if
      //your implementation uses internals that have thread affinity.
      _thread_id = GetCurrentThreadId();
      _tables = new Dictionary<string, PluginTableTemplate>();
      _filePath = connectionPath.LocalPath;
    }

    public override void Close()
    {
      //Dispose of any cached table instances here
      foreach(var table in _tables.Values)
      {
        ((ProPluginTableTemplate)table).Dispose();
      }
      _tables.Clear();
    }

    /// <summary>
    /// Open the specified table
    /// </summary>
    /// <param name="name">The name of the table to open</param>
    /// <remarks>For the sample, you can also pass in the name of the csv file<br/>
    /// e.g. "TREE_INSPECTIONS" or "tree_inspections.csv" will both work</remarks>
    /// <returns><see cref="PluginTableTemplate"/></returns>
    public override PluginTableTemplate OpenTable(string name)
    {
      //This is only necessary if your internals have thread affinity
      //
      //If you are using shared data (eg "static") it is your responsibility
      //to manage access to it across multiple threads.
      if (_thread_id != GetCurrentThreadId())
      {
        throw new ArcGIS.Core.CalledOnWrongThreadException();
      }

      var table_name = System.IO.Path.GetFileNameWithoutExtension(name).ToUpper();

      //ensure the file name has a "csv" suffix
      var file_name = System.IO.Path.ChangeExtension(name, ".csv");

      if (!this.GetTableNames().Contains(table_name))
        throw new GeodatabaseTableException($"The table {table_name} was not found");

      //If you do ~not~ want to cache the csv for the lifetime of
      //your workspace instance then return a new table on each request. The edge case
      //for this sample being that the contents of the folder or individual csv's can
      //change after the data is loaded and those changes will not be reflected in a
      //given workspace instance until it is closed and re-opened.
      //
      //return new ProPluginTableTemplate(path, table_name, SpatialReferences.WGS84);

      if (!_tables.Keys.Contains(table_name))
      {
        string path = System.IO.Path.Combine(_filePath, file_name);
        _tables[table_name] = new ProPluginTableTemplate(path, table_name, SpatialReferences.WGS84);
      }
      return _tables[table_name];
    }

    /// <summary>
    /// Get the table names available in the workspace
    /// </summary>
    /// <returns><see cref="IReadOnlyList{string}"/></returns>
    public override IReadOnlyList<string> GetTableNames()
    {
      var fileNames =
        System.IO.Directory.GetFiles(_filePath, "*.csv", System.IO.SearchOption.TopDirectoryOnly)
            .Select(fn => System.IO.Path.GetFileNameWithoutExtension(fn).ToUpper());

      //there is an edge case where files could have been deleted after they
      //were opened...so union in the cache names
      var cachedTables = _tables.Keys;
      
      return fileNames.Union(cachedTables).OrderBy(name => name).ToList();
    }

    /// <summary>
    /// Returns whether or not SQL queries are supported on the plugin
    /// </summary>
    /// <remarks>Returning false (default) means that the WhereClause of an
    /// incoming query filter will always be empty (regardless of what clients
    /// set it to)</remarks>
    /// <returns>true or false</returns>
    public override bool IsQueryLanguageSupported()
    {
      return true;
    }

  }
}

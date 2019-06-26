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
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using ArcGIS.Core.Data;
using ArcGIS.Core.Data.PluginDatastore;
using ArcGIS.Core.Geometry;
using ProMdbAccessDb;

namespace ProMdbPluginDatasource
{
    /// <summary>
    /// Defines the data source to access Microsoft Access MDB personal geodatabases and
    /// made available to ArcGIS Pro via a plug-in data source add-in.
    /// </summary>
    public class ProMdbPluginDatasourceTemplate : PluginDatasourceTemplate
    {
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        [DllImport("kernel32.dll")]

        internal static extern uint GetCurrentThreadId();
        //private static int InstanceCount = 0;

        private string _filePath = "";
        private uint _threadId;

        private ProMdbAccessDb.ProMdbAccessDb _accessDb = null;
        private string _accessError = "Access DB has not been opened";

        private Dictionary<string, PluginTableTemplate> _tables;

        /// <summary>
        /// Opens the datasource allowing access to a Microsoft Access personal geodatabase
        /// </summary>
        /// <param name="connectionPath">path to the mdb file which has to be renamed to ArcGisMdb</param>
        public override void Open(Uri connectionPath)
        {
            //TODO Initialize your plugin instance. Individual instances
            //of your plugin may be initialized on different threads
            if (!System.IO.File.Exists(connectionPath.LocalPath))
            {
                throw new System.IO.DirectoryNotFoundException(connectionPath.LocalPath);
            }
            //initialize
            //Strictly speaking, tracking your thread id is only necessary if
            //your implementation uses internals that have thread affinity.
            _threadId = GetCurrentThreadId();
            _tables = new Dictionary<string, PluginTableTemplate>();
            _filePath = connectionPath.LocalPath;
            InitAccessDB(_filePath);
        }

        private void InitAccessDB(string path)
        {
            if (_accessDb == null)
            {
                try
                {
                    _accessDb = new ProMdbAccessDb.ProMdbAccessDb(path);
                }
                catch (Exception ex)
                {
                    _accessError = ex.Message;
                    _accessDb = null;
                }
            }
        }

        /// <summary>
        /// Called when the datasource is closed ... free reference data
        /// </summary>
        public override void Close()
        {
            //Dispose of any cached table instances here
            foreach (var table in _tables.Values)
            {
                ((ProMdbPluginTableTemplate)table).Dispose();
            }
            _tables.Clear();
			if (_accessDb != null)
			{
				_accessDb.Close();
				_accessDb = null;
			}
        }

        /// <summary>
        /// Implements the opening of a table using a given path.  
        /// </summary>
        /// <param name="tablePath">table name or 'Path' to the table which is comprised of the access database file followed by the tablename</param>
        /// <returns>Table template that matches the table name (cached)</returns>
        public override PluginTableTemplate OpenTable(string tablePath)
        {
            var tableName = System.IO.Path.GetFileNameWithoutExtension(tablePath);
            if (!this.GetTableNames().Contains(tableName))
                throw new GeodatabaseTableException($"The table {tableName} was not found");
            if (!_tables.Keys.Contains(tableName))
            {
                _tables[tableName] = new ProMdbPluginTableTemplate(_accessDb, _tableInfos[tableName]);
            }
            return _tables[tableName];
        }

        private Dictionary<string, ProMdbTableInfo> _tableInfos = null;

        /// <summary>
        /// returns a list of table names
        /// </summary>
        /// <returns>list of strings (table names)</returns>
        public override IReadOnlyList<string> GetTableNames()
        {
            if (_tableInfos != null) return _tableInfos.Keys.ToList();
            if (_accessDb == null)
            {
                throw new Exception(_accessError);
            }
            _tableInfos = _accessDb.GetSpatialTables();
            return _tableInfos.Keys.ToList();
        }

        /// <summary>
        /// returns true if query language is supported (search using QueryFilter)
        /// </summary>
        /// <returns>true</returns>
        public override bool IsQueryLanguageSupported()
        {
            //default is false
            return true;
        }
    }
}

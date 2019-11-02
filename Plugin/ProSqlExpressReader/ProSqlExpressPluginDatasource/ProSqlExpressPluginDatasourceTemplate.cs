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
using ProSqlExpressDb;

namespace ProSqlExpressPluginDatasource
{
    /// <summary>
    /// Defines the data source to sql SQL Express personal geodatabases and
    /// made available to ArcGIS Pro via a plug-in data source add-in.
    /// </summary>
    public class ProSqlExpressPluginDatasourceTemplate : PluginDatasourceTemplate
    {
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        [DllImport("kernel32.dll")]

        internal static extern uint GetCurrentThreadId();
        //private static int InstanceCount = 0;

        private uint _threadId;

        private ProSqlExpressDb.ProSqlExpressDb _sqlDb = null;
        private string _sqlError = "Sql DB has not been opened";

        private Dictionary<string, PluginTableTemplate> _tables;

        /// <summary>
        /// Opens the datasource allowing sql to a Microsoft Access personal geodatabase
        /// </summary>
        /// <param name="connectionPath">path to the sql file which has to be renamed to ArcGisSql</param>
        public override void Open(Uri connectionPath)
        {
            var localPath = connectionPath.LocalPath.Replace("||", ";");
            System.Diagnostics.Debug.WriteLine($@"*** {localPath}");
            var parts = localPath.Split('|');
            var sqlConStr = localPath;
            if (parts.Length >= 2)
            {
                // in this case we get the following format:
                // sqlexpress file path '|' connection string '|' tablename
                localPath = parts[0];
                sqlConStr = parts[1];
            }
            //TODO Initialize your plugin instance. Individual instances
            //of your plugin may be initialized on different threads
            if (!System.IO.File.Exists(localPath))
            {
                throw new System.IO.DirectoryNotFoundException(connectionPath.LocalPath);
            }
            //initialize
            //Strictly speaking, tracking your thread id is only necessary if
            //your implementation uses internals that have thread affinity.
            _threadId = GetCurrentThreadId();
            _tables = new Dictionary<string, PluginTableTemplate>();
            InitAccessDB(sqlConStr);
        }

        private void InitAccessDB(string path)
        {
            if (_sqlDb == null)
            {
                try
                {
                    _sqlDb = new ProSqlExpressDb.ProSqlExpressDb(path);
                }
                catch (Exception ex)
                {
                    _sqlError = ex.Message;
                    _sqlDb = null;
                }
            }
        }

        /// <summary>
        /// Called when the datasource is closed ... free reference data
        /// </summary>
        public override void Close()
        {
            //Dispose of any cached table instances here
        }

        /// <summary>
        /// Implements the opening of a table using a given path.  
        /// </summary>
        /// <param name="tablePath">table name or 'Path' to the table which is comprised of the sql database file followed by ; and the tablename</param>
        /// <returns>Table template that matches the table name (cached)</returns>
        public override PluginTableTemplate OpenTable(string tablePath)
        {
			//var tableName = System.IO.Path.GetFileNameWithoutExtension(tablePath);
			//if (!this.GetTableNames().Contains(tableName))
			//    throw new GeodatabaseTableException($"The table {tableName} was not found");
			var ti = TableInfos.Where((i) => i.TableName.EndsWith(tablePath)
											|| i.Path.EndsWith(tablePath)).FirstOrDefault();
			if (ti == null)
				throw new GeodatabaseTableException($"The table {tablePath} was not found");
			_tables[ti.TableName] = new ProSqlPluginTableTemplate(_sqlDb, ti);
            return _tables[ti.TableName];
        }

        private List<ProSqlExpressTableInfo> _tableInfos = null;

		public List<ProSqlExpressTableInfo> TableInfos
		{
			get
			{
				if (_tableInfos == null || _tableInfos.Count == 0)
				{
					if (_sqlDb == null)
					{
						throw new Exception(_sqlError);
					}
					_tableInfos = _sqlDb.GetSpatialTables();
				}
				return _tableInfos;
			}
		}

        /// <summary>
        /// returns a list of table names
        /// </summary>
        /// <returns>list of strings (table names)</returns>
        public override IReadOnlyList<string> GetTableNames()
        {
            return TableInfos.Select((i)=> i.Path).ToList();
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

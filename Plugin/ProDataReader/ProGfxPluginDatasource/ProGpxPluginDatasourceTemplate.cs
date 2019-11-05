/*

   Copyright 2017 Esri

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

namespace ProGpxPluginDatasource
{
    public class ProGpxPluginDatasourceTemplate : PluginDatasourceTemplate
    {
		[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
		[DllImport("kernel32.dll")]

		internal static extern uint GetCurrentThreadId();

		private string _filePath = "";
		private uint _threadId;

		private Dictionary<string, PluginTableTemplate> _tables;

		public override void Open(Uri connectionPath)
        {
			//TODO Initialize your plugin instance. Individual instances
			//of your plugin may be initialized on different threads
			if (!System.IO.File.Exists(connectionPath.LocalPath))
			{
				throw new System.IO.FileNotFoundException(connectionPath.LocalPath);
			}
			//initialize
			//Strictly speaking, tracking your thread id is only necessary if
			//your implementation uses internals that have thread affinity.
			_threadId = GetCurrentThreadId();
			_tables = new Dictionary<string, PluginTableTemplate>();
			_filePath = connectionPath.LocalPath;
		}

        public override void Close()
		{
			if (_tables == null) return;
			//Dispose of any cached table instances here
			foreach (var table in _tables.Values)
			{
				((ProGpxPluginTableTemplate)table).Dispose();
			}
			_tables.Clear();
		}

        public override PluginTableTemplate OpenTable(string tableName)
		{
			if (!this.GetTableNames().Contains(tableName))
				throw new GeodatabaseTableException($"The table {tableName} was not found");
			return _tables[tableName];
		}

        public override IReadOnlyList<string> GetTableNames()
        {
			if (_tables.Count > 0) return _tables.Keys.ToList();
			var tableName = System.IO.Path.GetFileNameWithoutExtension(_filePath);
			if (System.IO.File.Exists(_filePath))
			{
                // there is only one 'table' which is the one gpx file
                // but there are two representations: points and lines:
                // the table type is appended to the tablename like this: "|Point" or "|Line"
                _tables.Add(tableName+ "|Point", new ProGpxPluginTableTemplate(_filePath+ "|Point"));
                _tables.Add(tableName+ "|Line", new ProGpxPluginTableTemplate(_filePath+ "|Line"));
            }
			return _tables.Keys.ToList();
		}

        public override bool IsQueryLanguageSupported()
        {
			//default is false
			return true;
        }
    }
}

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
using ImageMetadata;

namespace ProJpgPluginDatasource
{
    public class ProJpgPluginDatasourceTemplate : PluginDatasourceTemplate
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
			var localPath = connectionPath.LocalPath;
			if (!System.IO.Path.GetExtension(localPath).Equals (".jpg", StringComparison.CurrentCultureIgnoreCase))
			{
				localPath = System.IO.Directory.GetParent(localPath).FullName;
			}
			if (!(System.IO.File.Exists(localPath) || System.IO.Directory.Exists(localPath)))
			{
				throw new System.IO.DirectoryNotFoundException(connectionPath.LocalPath);
			}
			//initialize
			//Strictly speaking, tracking your thread id is only necessary if
			//your implementation uses internals that have thread affinity.
			_threadId = GetCurrentThreadId();
			_tables = new Dictionary<string, PluginTableTemplate>();
			_filePath = localPath;
		}

        public override void Close()
        {
			if (_tables == null) return;
			//Dispose of any cached table instances here
			foreach (var table in _tables.Values)
			{
				((ProJpgPluginTableTemplate)table).Dispose();
			}
			_tables.Clear();
		}

		/// <summary>
		/// Implements the opening of a table using a given path.  
		/// </summary>
		/// <param name="tableName">table name or path is the file path to the jpg</param>
		/// <returns>Table template that matches the table name (cached)</returns>
		public override PluginTableTemplate OpenTable(string tableName)
        {
			if (!this.GetTableNames().Contains(tableName))
				throw new GeodatabaseTableException($"The table {tableName} was not found");
			return _tables[tableName];
		}

        public override IReadOnlyList<string> GetTableNames()
        {
			if (_tables.Count > 0) return _tables.Keys.ToList();
			if (System.IO.Directory.Exists(_filePath))
			{
				var jpgFiles = System.IO.Directory.GetFiles(_filePath, $@"*.jpg", System.IO.SearchOption.TopDirectoryOnly);
				List<XimgInfo> ximgInfos = new List<XimgInfo>();
				foreach (var jpgFullName in jpgFiles)
				{
					var xInfo = new XimgInfo(jpgFullName);
					System.Diagnostics.Debug.WriteLine($@"Image {xInfo.IsImage}");
					if (!xInfo.IsImage || !xInfo.IsGpsEnabled) continue;
					ximgInfos.Add(new XimgInfo(jpgFullName));
				}
				var tableName = System.IO.Path.GetFileName (_filePath);
				_tables.Add(tableName, new ProJpgPluginTableTemplate(ximgInfos, tableName));
			}
			else
			{
				// there is only one 'table' which is the image
				var tableName = System.IO.Path.GetFileNameWithoutExtension(_filePath);
				var lst = new List<XimgInfo> { new XimgInfo(_filePath) };
				_tables.Add (tableName, new ProJpgPluginTableTemplate(lst, tableName));
			}
			return _tables.Keys.ToList();
        }

        public override bool IsQueryLanguageSupported()
        {
            return true;
        }
    }
}

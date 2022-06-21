/*

   Copyright 2017 Esri

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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArcGIS.Core.Data;
using ArcGIS.Core.Data.PluginDatastore;
using ArcGIS.Core.Geometry;

namespace ProDataReaderProGpxPluginDatasource
{
    public class ProGpxPluginCursorTemplate : PluginCursorTemplate
	{
		private Queue<int> _oids;
		private IEnumerable<string> _columns;
		private SpatialReference _srout;
		private IPluginRowProvider _provider;
		private int CurrentId { get; set; }
		private static readonly object _lock = new object();

		internal ProGpxPluginCursorTemplate(IPluginRowProvider provider, IEnumerable<int> oids,
											IEnumerable<string> columns, SpatialReference srout)
		{
			_provider = provider;
			_oids = new Queue<int>(oids);
			_columns = columns;
			_srout = srout;
			CurrentId = -1;
		}

		public override PluginRow GetCurrentRow()
		{
			int id = -1;
			//The lock shouldn't be necessary if your cursor is a per thread instance
			//(like the sample is)
			lock (_lock)
			{
				id = CurrentId;
			}
			return _provider.FindRow(id, _columns, _srout);
		}

		public override bool MoveNext()
		{
			if (_oids.Count == 0)
				return false;

			//The lock shouldn't be necessary if your cursor is a per thread instance
			//(like the sample is)
			lock (_lock)
			{
				CurrentId = _oids.Dequeue();
			}
			return true;
		}
	}
}

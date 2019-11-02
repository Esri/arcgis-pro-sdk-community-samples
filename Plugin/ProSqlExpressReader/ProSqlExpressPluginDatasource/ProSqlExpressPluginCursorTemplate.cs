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

namespace ProSqlExpressPluginDatasource
{
    /// <summary>
    /// 
    /// </summary>
    public class ProSqlPluginCursorTemplate : PluginCursorTemplate
    {
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        [DllImport("kernel32.dll")]

        internal static extern uint GetCurrentThreadId();

        private Queue<int> _oids;
        private IEnumerable<string> _columns;
        private SpatialReference _srout;
        private IPluginRowProvider _provider;
        private int CurrentId { get; set; }
        private static readonly object _lock = new object();

        internal ProSqlPluginCursorTemplate(IPluginRowProvider provider, IEnumerable<int> oids,
                                         IEnumerable<string> columns, SpatialReference srout)
        {
            _provider = provider;
            _oids = new Queue<int>(oids);
            _columns = columns;
            _srout = srout;
            CurrentId = -1;
        }

        /// <summary>
        /// Get the current row when using the MoveNext cursor method
        /// </summary>
        /// <returns>PluginRow holding the attributes of the current row</returns>
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

        /// <summary>
        /// Move cursor to the next record
        /// </summary>
        /// <returns>true if the oid list has another record</returns>
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

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
using System.Text;
using System.Threading.Tasks;

namespace ProMdbAccessDb
{

    /// <summary>
    /// tracks metadata needed to handle a Pro MDB table / feature class
    /// </summary>
    public class ProMdbTableInfo
    {
        /// <summary>
        /// Name of the table/feature class
        /// </summary>
        public string TableName { get; set; }
        /// <summary>
        /// Name of the Geometry column
        /// </summary>
        public string GeometryFieldName { get; set; }
        /// <summary>
        /// Geometry Type: point/line/polygon
        /// </summary>
        public int GeometryType { get; set; }
        /// <summary>
        /// Layer extent
        /// </summary>
        public double ExtentLeft { get; set; }
        /// <summary>
        /// Layer extent
        /// </summary>
        public double ExtentBottom { get; set; }
        /// <summary>
        /// Layer extent
        /// </summary>
        public double ExtentRight { get; set; }
        /// <summary>
        /// Layer extent
        /// </summary>
        public double ExtentTop { get; set; }
        /// <summary>
        /// Spatial reference of feature class: this is a string ... we need to use CreateSpatialReference
        /// </summary>
        public string SpatialRefString { get; set; }
        /// <summary>
        /// Meta data for the table's attribute columns
        /// </summary>
        public ProMdbFieldInfo FieldInfo { get; set; }
    }

}

/*

   Copyright 2020 Esri

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
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Editing.Attributes;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoUseSelection.Helpers
{
	public static class Extensions
	{
		public static void Difference(this EditOperation editOp, BasicFeatureLayer layer,
																						 IEnumerable<long> oids, Geometry diffGeom)
		{
			foreach (var oid in oids)
				editOp.Difference(layer, oid, diffGeom);
		}

		public static void Difference(this EditOperation editOp, BasicFeatureLayer layer,
																												 long oid, Geometry diffGeom)
		{
			var insp = new Inspector();
			insp.Load(layer, oid);

			//do the difference
			var geom = GeometryEngine.Instance.Difference((Geometry)insp["SHAPE"], diffGeom);
			insp["SHAPE"] = geom;
			//call modify
			editOp.Modify(insp);
		}
	}
}

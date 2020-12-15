/*

   Copyright 2020 Esri

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
using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Extensions;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;

namespace GraphicOverlay4Debug
{
	internal class SketchPolygon : MapTool
	{
		public SketchPolygon()
		{
			IsSketchTool = true;
			SketchType = SketchGeometryType.Polygon;
			SketchOutputMode = SketchOutputMode.Map;
		}

		protected override Task OnToolActivateAsync(bool active)
		{
			return base.OnToolActivateAsync(active);
		}

		protected override Task<bool> OnSketchCompleteAsync(Geometry geometry)
		{
			QueuedTask.Run(() =>
			{
				Module1.AddOverlay(geometry, Module1.GetPolygonSymbolRef());
			});
			return base.OnSketchCompleteAsync(geometry);
		}
	}
}

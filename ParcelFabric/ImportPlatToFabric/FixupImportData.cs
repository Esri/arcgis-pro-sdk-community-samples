/*

   Copyright 2022 Esri

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
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;

namespace ImportPlatToFabric
{
	internal class FixupImportData : Button
	{
		private const string LyrNameImport = "ImportPlats";

		protected override async void OnClick()
		{
			var map = MapView.Active?.Map;
			if (map == null) return;
			try
			{
				var lyr = map.GetLayersAsFlattenedList().FirstOrDefault((fl) => fl.Name == LyrNameImport) as FeatureLayer;
        var msg = await QueuedTask.Run(() =>
        {
          var attribs = new Dictionary<long, Dictionary<string, object>>();
          using (var cursor = lyr.Search())
            while (cursor.MoveNext())
            {
              using (var feat = cursor.Current as Feature)
              {
                var path = feat.GetShape() as Polyline;
                var ls = LineBuilderEx.CreateLineSegment(path.Points[0], path.Points[path.PointCount-1]);
                attribs.Add(feat.GetObjectID(), new Dictionary<string, object>() { { "Direction", ls.Angle } });
              }
            }

          // create an edit operation
          var editOperation = new EditOperation() { Name = $@"Update {LyrNameImport}" };
          foreach (var key in attribs.Keys)
					{
            editOperation.Modify(lyr, key, attribs[key]);
          }
          if (!editOperation.Execute()) return editOperation.ErrorMessage;
          return "update complete";
        });
        MessageBox.Show(msg);
      }
			catch (Exception ex)
			{
        MessageBox.Show($@"Error: {ex}");
			}
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Data.Mapping;
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

namespace QueryGraphicBoundary
{
	internal class AddAnnoOutlines : Button
	{
		protected override void OnClick()
		{
			var anno_layer = MapView.Active.Map.GetLayersAsFlattenedList()
				.OfType<AnnotationLayer>().FirstOrDefault();
			if (anno_layer == null)
				return;
			List<Polygon> outlines = new List<Polygon>();

			QueuedTask.Run(() =>
			{
				var rc = anno_layer.GetFeatureClass().Search();
				while(rc.MoveNext())
				{
					var af = rc.Current as AnnotationFeature;
					//var geom = af.GetGraphicOutline();
					//if (geom != null)
					//{
					//	var poly = GeometryEngine.Instance.Buffer(geom, 0.1) as Polygon;
					//	outlines.Add(poly);
					//}
				}
				//Module1.Current.AddOutlines(outlines);
			});
		}
	}
}

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
using ArcGIS.Desktop.Internal.Mapping;
using ArcGIS.Desktop.Mapping;

namespace QueryGraphicBoundary
{
	internal class AddShapeOutline : Button
	{
		protected override void OnClick()
		{
			//Make sure at least one feature layer or anno layer is
			//selected in the TOC
			var feat_layer = MapView.Active.GetSelectedLayers()
				.OfType<BasicFeatureLayer>().FirstOrDefault();
			if (feat_layer == null)
				return;//Nothing is selected in the TOC

			List<Polygon> polys = new List<Polygon>();

			QueuedTask.Run(() =>
			{
				//change the Object ID to a different feature as needed...
				int oid = 2;//Hard-coded feature choice
				double refscale = -1;
				//MaskOption mask = MaskOption.ExactSimplified;
				double marginMeters = 0.2;
				bool convexHull = true;
				bool buffer = false;
				bool isAnno = feat_layer is AnnotationLayer;


				//if (feat_layer is AnnotationLayer annoLayer)
				//{
				//	refscale = ((AnnotationFeatureClass)annoLayer.GetFeatureClass())
				//										 .GetDefinition().GetReferenceScale();
				//}
				var outline = DrawingOutlineType.Exact;
				var geom = feat_layer.QueryDrawingOutline(oid, MapView.Active, outline);
				var final_mask = Module1.Current.PostProcess(
					geom, feat_layer.ShapeType, marginMeters, isAnno, convexHull);

				//var geom = MappingHelpers.QueryGraphicsOutline(
				//	feat_layer, oid, refscale, MapView.Active.Map.SpatialReference, 
				//	mask, marginMeters);
				if (final_mask != null)
				{
					//var sr_in = geom.SpatialReference;
					//var sr = MapView.Active.Map.SpatialReference;
					//var proj_geom = GeometryEngine.Instance.Project(geom, sr);
					//var poly = GeometryEngine.Instance.Buffer(proj_geom, 0.2) as Polygon;

					//Add the resulting outline geometries to the overlay
					polys.Add(final_mask as Polygon);
					Module1.Current.AddOutlines(polys, outline, convexHull);
				}
				
					

					//var geom = af.GetGraphicOutline();
					//if (geom != null)
					//{
					//	var poly = GeometryEngine.Instance.Buffer(geom, 0.1) as Polygon;
					//	outlines.Add(poly);
					//}
				
				
			});
		}
	}
}

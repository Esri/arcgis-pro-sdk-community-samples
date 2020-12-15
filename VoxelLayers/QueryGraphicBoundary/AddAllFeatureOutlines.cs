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
using ArcGIS.Desktop.Internal.Mapping;
using ArcGIS.Desktop.Mapping;

namespace QueryGraphicBoundary
{
	internal class AddAllFeatureOutlines : Button
	{
		protected override void OnClick()
		{
			//Gets all the layers from the TOC - feature layers and anno layers
			var feat_layers = MapView.Active.Map.GetLayersAsFlattenedList()
				.OfType<BasicFeatureLayer>();
			
			List<Polygon> outlines = new List<Polygon>();
			var mv = MapView.Active;
			QueuedTask.Run(() =>
			{
				SpatialReference sr = mv.Map.SpatialReference;
				double marginMeters = 0.2;
				bool convexHull = false;
				var maskOption = DrawingOutlineType.Exact;

				var polys = new List<Polygon>();
				foreach (var fl in feat_layers)
				{
					if (!fl.IsVisible)
						continue;
					bool isAnno = fl is AnnotationLayer;

					double refScale = -1;
					//if (fl is AnnotationLayer)
					//{
					//	//use anno reference scale
					//	using (var fc = ((AnnotationLayer)fl).GetFeatureClass() 
					//	            as ArcGIS.Core.Data.Mapping.AnnotationFeatureClass)
					//	using (var fc_def = fc.GetDefinition())
					//	{
					//		refScale = fc_def.GetReferenceScale();
					//		//sr = fc_def.GetSpatialReference();
					//	}
					//}
					//else
					//{
					//	refScale = mv.Camera.Scale;
					//}

					var rc = fl.Search();
					while(rc.MoveNext())
					{
						var oid = rc.Current.GetObjectID();
						try
						{
							var geom = fl.QueryDrawingOutline(oid, MapView.Active, maskOption) as Polygon;
							if (geom !=null && !geom.IsEmpty )
							{
								var final_mask = Module1.Current.PostProcess(
									 geom, fl.ShapeType, marginMeters, isAnno, convexHull) as Polygon;
								polys.Add(final_mask);
							}
							else
							{
								System.Diagnostics.Debug.WriteLine("===============");
								System.Diagnostics.Debug.WriteLine($"{fl.Name}: oid:{oid}");
								System.Diagnostics.Debug.WriteLine("Warning: geometry is null");
							}
							
						}
						catch(Exception ex)
						{
							System.Diagnostics.Debug.WriteLine("===============");
							System.Diagnostics.Debug.WriteLine($"{fl.Name}: oid:{oid}");
							System.Diagnostics.Debug.WriteLine("Exception: ");
							System.Diagnostics.Debug.WriteLine(ex.ToString());

						}


						//var geom = MappingHelpers.QueryGraphicsOutline(
						//	fl, oid, refScale, sr, maskOption, marginMeters) as Polygon;
						//if (geom != null || !geom.IsEmpty)
						//{
						//	polys.Add(geom);
						//}


					}
				}
				//Add the result "outlines" to the map overlay
				Module1.Current.AddOutlines(polys, maskOption, convexHull);
			});

		}
	}
}

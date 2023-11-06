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
using System.Windows.Input;
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

namespace QueryGraphicBoundary
{

	public enum MaskKind
	{
		Box = 0,
		ConvexHull,
		Exact,
		ExactSimplified
	}

  /// <summary>
  /// This sample demonstrates the use of the BasicFeatureLayer QueryDrawingOutline(...) API methods
  /// Various cartographic workflows require updating and replacing existing feature masks to accomodate edits to the underlying (masked) features &lt;br/&gt;The &lt;b&gt;UpdateOutline&lt;/b&gt; tool uses QueryDrawingOutline to generate the feature outline geometry. The "PostProcess" logic in the Module1 class replicates the post process logic of the Geoprocessing 
  /// &lt;a href="https://pro.arcgis.com/en/pro-app/tool-reference/cartography/feature-outline-masks.htm"&gt; Feature Outline Masks (Cartography)&lt;/a&gt; tool. The sample works with any point, line, polygon, or annotation feature dataset
  /// </summary>
  /// <remarks>
  /// 1. Download the Community Sample data (see under the 'Resources' section for downloading sample data)
  /// 1. Make sure that the Sample data is unzipped in c:\data 
  /// 1. The project used for this sample is 'C:\Data\FeatureTest\FeatureTest.aprx'
  /// 1. In Visual Studio click the Build menu. Then select Build Solution.
  /// 1. Launch the debugger to open ArcGIS Pro.
  /// 1. ArcGIS Pro will open, select the FeatureTest.aprx project
  /// 1. Click on the 'Test Mask' tab.  
	/// ![UI](Screenshots/Screen1.png)
	/// 1. Change the type of Mask and a Margin before clicking on the 'Update Outline Tool' button.
  /// 1. Click on a map feature to select that map feature for the Outline drawing.
  /// ![UI](Screenshots/Screen2.png)
  /// </remarks>
  internal class Module1 : Module
	{
		private static Module1 _this = null;
		private List<IDisposable> _graphics = new List<IDisposable>();

		/// <summary>
		/// Retrieve the singleton instance to this module here
		/// </summary>
		public static Module1 Current
		{
			get
			{
				return _this ?? (_this = (Module1)FrameworkApplication.FindModule("QueryGraphicBoundary_Module"));
			}
		}

		private MaskKind _maskKind = default(MaskKind);

		public MaskKind SelectedMaskKind
		{
			get
			{
				return _maskKind;
			}
			set
			{
				_maskKind = value;
			}
		}

		private double _margin = 0.0;

		public double Margin
		{
			get
			{
				return _margin;
			}
			set
			{
				_margin = value;
			}
		}

		/// <summary>
		/// Adds the outlines to the current map view graphics overlay
		/// </summary>
		/// <param name="boundaries"></param>
		/// <param name="maskKind"></param>
		/// <param name="clearGraphics"></param>
		public void AddOutlines(List<Polygon> boundaries, MaskKind maskKind, bool clearGraphics = false)
		{
			if (clearGraphics)
				ClearGraphics();
			CIMColor color = ColorFactory.Instance.BlueRGB;//ExactSimplified
			switch(maskKind)
			{
				case MaskKind.Box:
					color = ColorFactory.Instance.CreateRGBColor(4, 117, 34);
					break;
				case MaskKind.ConvexHull:
					color = ColorFactory.Instance.RedRGB;
					break;
				case MaskKind.Exact:
					color = ColorFactory.Instance.CreateColor(
						System.Windows.Media.Colors.Purple);
					break;
			}

			var outline = SymbolFactory.Instance.ConstructPolygonSymbol(
													 null,
													 SymbolFactory.Instance.ConstructStroke(
														 color, 1.5)).MakeSymbolReference();
			foreach(var bnd in boundaries)
			{
				var polyGraphic = new CIMPolygonGraphic()
				{
					Polygon = bnd,
					Symbol = outline
				};
				_graphics.Add(MapView.Active.AddOverlay(polyGraphic));
			}
		}

		/// <summary>
		/// Clear graphics from the map view overlay
		/// </summary>
		public void ClearGraphics()
		{
			foreach(var graphic in _graphics)
			{
				graphic.Dispose();
				
			}
			_graphics.Clear();
		}

		/// <summary>
		/// Post process the outline geometry per the <see cref="MaskKind"/> value.
		/// </summary>
		/// <param name="outline">The outline geometry to post-process</param>
		/// <param name="shapeType">Point, Line, or Poly</param>
		/// <param name="margin">A margin to be applied to the outline</param>
		/// <param name="maskKind">The type of outline to create</param>
		/// <param name="isanno">Flag indicating whether the outline came from an annotation
		/// feature or not</param>
		/// <remarks>The MaskKind options replicate the same options found on the GP
		/// Feature Outline Masks tool</remarks>
		/// <returns>The post-processed outline geometry</returns>
		public Geometry PostProcess(Geometry outline, esriGeometryType shapeType, double margin, MaskKind maskKind, bool isanno = false)
		{
			Geometry finalMask = outline;
			if (isanno)
			{
				if (maskKind == MaskKind.ConvexHull)
				{
					finalMask = ConvexHull(outline, shapeType, true);
					outline = finalMask;
				}
				finalMask = Buffer(outline, shapeType, margin, maskKind, true);
			}
			else if (shapeType == esriGeometryType.esriGeometryPoint ||
				       shapeType == esriGeometryType.esriGeometryMultipoint)
			{
				finalMask = Buffer(outline, shapeType, margin, maskKind);
				outline = finalMask;

				if (maskKind == MaskKind.ConvexHull)
				{
					finalMask = ConvexHull(outline, shapeType);
				}
			}
			else
			{
				finalMask = Buffer(outline, shapeType, margin, maskKind);
			}
			return finalMask;
		}

		private Geometry ConvexHull(Geometry geom, esriGeometryType shapeType, bool isAnno = false)
		{
			Geometry convex_hull = geom;
			if (isAnno)
			{
				var geom_anno = geom;
				convex_hull = GeometryEngine.Instance.ConvexHull(geom_anno);
			}
			else if (shapeType == esriGeometryType.esriGeometryPoint ||
				shapeType == esriGeometryType.esriGeometryMultipoint)
			{
				var poly = geom as Polygon;
				//convex hull on each external ring
				var hulls = new List<Polygon>();
				foreach(var ring in poly.GetExteriorRings(true))
				{
					hulls.Add(GeometryEngine.Instance.ConvexHull(ring) as Polygon);
				}
				if (hulls.Count == 1)
					convex_hull = hulls[0];
				else
				{
					//union the individual convex hulls
					convex_hull = GeometryEngine.Instance.Union(hulls);
				}
			}
			return convex_hull;
		}

		private Geometry Buffer(Geometry geom, esriGeometryType shapeType, double margin,
			MaskKind maskKind, bool isAnno = false)
		{
			
			var poly_outline = geom as Polygon;
			if (poly_outline.HasCurves)
			{
				poly_outline = 
					GeometryEngine.Instance.DensifyByDeviation(geom, 0.1 * margin) as Polygon;
			}
			
			if (margin > 0.0)
			{
				if (maskKind == MaskKind.Box || maskKind == MaskKind.ConvexHull)
				{
					//strip out interior polygons
					var ext_poly = PolygonBuilderEx.CreatePolygon(poly_outline.GetExteriorRings(true), AttributeFlags.None);

					var joins = maskKind == MaskKind.Box ?
											LineJoinType.Miter : LineJoinType.Bevel;
					var buff_out = GeometryEngine.Instance.GraphicBuffer(ext_poly, margin, joins,
						LineCapType.Butt, 4, 0.05 * margin, 64);
					poly_outline = GeometryEngine.Instance.Generalize(buff_out, 0.01 * margin) as Polygon;
				}
				else
				{
					var buff_out = GeometryEngine.Instance.Buffer(poly_outline, margin);
					if (maskKind == MaskKind.ExactSimplified)
					{
						poly_outline = GeometryEngine.Instance.Generalize(buff_out, 0.05 * margin) as Polygon;
					}
					else
					{
						poly_outline = buff_out as Polygon;
					}
				}
			}
			//simplify if needed
			//return GeometryEngine.Instance.SimplifyAsFeature(poly_outline);
			return poly_outline;
		}

		#region Overrides
		/// <summary>
		/// Called by Framework when ArcGIS Pro is closing
		/// </summary>
		/// <returns>False to prevent Pro from closing, otherwise True</returns>
		protected override bool CanUnload()
		{
			//TODO - add your business logic
			//return false to ~cancel~ Application close
			return true;
		}

		#endregion Overrides

	}
}

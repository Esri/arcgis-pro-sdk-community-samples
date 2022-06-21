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
using ActiproSoftware.Windows.Extensions;

namespace GraphicOverlay4Debug
{
    /// <summary>
    /// "GraphicOverlay4Debug" shows how to use the graphics overlay to add point, line, and polygon features mainly to allow 'visual debugging' of geometries in your mapview.
    /// </summary>
    /// <remarks>
    /// 1. In Visual Studio click the Build menu. Then select Build Solution.
    /// 1. Launch the debugger to ArcGIS Pro.
    /// 1. In ArcGIS Pro open a new project with an empty map and after the map is displayed select the 'Visual Debug' tab
    /// ![UI](Screenshots/Screen1.png)
    /// 1. Select the 'Sketch Point' tool and start to sketch points on the map.  Each sketched point will be added to the map as a graphic overlay.
    /// ![UI](Screenshots/Screen2.png)
    /// 1. Select the 'Sketch Line' and 'Sketch Polygon' tools to add lines and polygons to the graphic overlay.
    /// ![UI](Screenshots/Screen3.png)
    /// 1. Click on the 'Clear Graphics' button to clear all overlay graphics.
	/// In order to make use the 'visual debug' capabilities you can simply:
	/// 1. Copy the two regions from the sample module.cs file into your module.cs: #region Overlay Helpers, and #region Symbol Helpers
	/// 1. Then paste the following code snippet in order to display your geometry as a graphic overlay
	/// 1. QueuedTask.Run(() => Module1.AddOverlay(geometry, Module1.GetPointSymbolRef())); // for points
	/// 1. QueuedTask.Run(() => Module1.AddOverlay(geometry, Module1.GetLineSymbolRef())); // for lines
	/// 1. QueuedTask.Run(() => Module1.AddOverlay(geometry, Module1.GetPolygonSymbolRef())); // for polygons
	/// 1. Module1.ClearGraphics(); // to clear the graphic overlay
    /// </remarks>
	internal class Module1 : Module
	{
		private static Module1 _this = null;

		/// <summary>
		/// Retrieve the singleton instance to this module here
		/// </summary>
		public static Module1 Current
		{
			get
			{
				return _this ?? (_this = (Module1)FrameworkApplication.FindModule("GraphicOverlay4Debug_Module"));
			}
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

        #region Overlay Helpers

        private static IList<IDisposable> _graphics = new List<IDisposable>();

        internal static void AddOverlay(Geometry geom, CIMSymbolReference symRef)
        {
            var mapView = MapView.Active;
            if (mapView == null) return;
            if (geom is Multipatch)
            {
                var cimMpGraphic = new CIMMultiPatchGraphic
                {
                    MultiPatch = geom as Multipatch,
                    Symbol = symRef,
                    Transparency = 64
                };
                _graphics.Add (mapView.AddOverlay(cimMpGraphic));
            }
            else
            {
                _graphics.Add (mapView.AddOverlay(geom, symRef));
            }
        }

        internal static void ClearGraphics()
        {
            foreach (var g in _graphics) g.Dispose();
            _graphics.Clear();
        }

        #endregion Overlay Helpers

        #region Symbol Helpers


        private static CIMSymbolReference _polySymbolRef = null;

        internal static CIMSymbolReference GetPolygonSymbolRef()
        {
            if (_polySymbolRef != null) return _polySymbolRef;
            //Creating a polygon with a red fill and blue outline.
            CIMStroke outline = SymbolFactory.Instance.ConstructStroke(
                 ColorFactory.Instance.BlueRGB, 2.0, SimpleLineStyle.Solid);
            _polySymbolRef = SymbolFactory.Instance.ConstructPolygonSymbol(
                 ColorFactory.Instance.CreateRGBColor(255, 190, 190, 50),
                 SimpleFillStyle.Solid, outline).MakeSymbolReference();
            return _polySymbolRef;
        }

        private static CIMSymbolReference _lineSymbolRef = null;

        /// <summary>
        /// Get a line symbol
        /// Must be called from the MCT.  Use QueuedTask.Run.
        /// </summary>
        /// <returns></returns>
        internal static CIMSymbolReference GetLineSymbolRef()
        {
            if (_lineSymbolRef != null) return _lineSymbolRef;
            _lineSymbolRef = SymbolFactory.Instance.ConstructLineSymbol(
                                ColorFactory.Instance.RedRGB, 4,
                                SimpleLineStyle.Solid).MakeSymbolReference();
            return _lineSymbolRef;
        }

        private static CIMSymbolReference _point3DSymbolRef = null;
        private static CIMSymbolReference _point2DSymbolRef = null;

        internal static CIMSymbolReference GetPointSymbolRef()
        {
            if (Is3D)
            {
                if (_point3DSymbolRef != null) return _point3DSymbolRef;
                var pointSymbol = GetPointSymbol("ArcGIS 3D", @"Pushpin 2");
                CIMPointSymbol pnt3DSym = pointSymbol.Symbol as CIMPointSymbol;
                pnt3DSym.SetSize(200);
                pnt3DSym.SetRealWorldUnits(true);
                _point3DSymbolRef = pnt3DSym.MakeSymbolReference();
                return _point3DSymbolRef;
            }
            if (_point2DSymbolRef != null) return _point2DSymbolRef;
            CIMPointSymbol pntSym = SymbolFactory.Instance.ConstructPointSymbol(ColorFactory.Instance.RedRGB, 8, SimpleMarkerStyle.Circle);
            _point2DSymbolRef = pntSym.MakeSymbolReference();
            return _point2DSymbolRef;
        }

        private static SymbolStyleItem GetPointSymbol(string styleProjectItemName, string symbolStyleName)
        {
            var style3DProjectItem = Project.Current.GetItems<StyleProjectItem>().Where(p => p.Name == styleProjectItemName).FirstOrDefault();
            var symbolStyle = style3DProjectItem.SearchSymbols(StyleItemType.PointSymbol, symbolStyleName).FirstOrDefault();
            return symbolStyle;
        }

        internal static bool Is3D => (MapView.Active?.ViewingMode == MapViewingMode.SceneGlobal || MapView.Active?.ViewingMode == MapViewingMode.SceneLocal);

        #endregion Symbol Helper

    }
}

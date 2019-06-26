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
using System.Text;
using System.Threading.Tasks;
using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Data.Realtime;
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

namespace RealtimeAnalysis
{
	internal class ViewshedAnalysis : Button
	{
		public RealtimeCursor _rtCursor = null;
		private Dictionary<int, Viewshed> _vwDict = null;
		protected async override void OnClick()
		{
			MapView mapView = MapView.Active;
			Map map = mapView.Map;

			#region initial setup
			if ((_rtCursor != null) && (_rtCursor.GetState() == RealtimeCursorState.Subscribed))
			{
				_rtCursor.Unsubscribe();
				foreach (var vs in mapView.GetExploratoryAnalysisCollection())
					await mapView.RemoveExploratoryAnalysis(vs);

				_vwDict = null;
				return;
			}

			_vwDict = new Dictionary<int, Viewshed>() { { 1, null }, { 2, null } };
			StreamLayer streamFLyr = map.Layers[0] as StreamLayer;
			//FeatureLayer vehicleFLyr = map.Layers[1] as FeatureLayer;//.Where(x => x.Name.Equals("Vehicles", StringComparison.InvariantCultureIgnoreCase)) as FeatureLayer;
			MapPoint searchPoint = null;
			#endregion

			await QueuedTask.Run(async () =>
			{
				RealtimeFeatureClass reatltimeFeatureClass = streamFLyr.GetFeatureClass();

				//RealtimeCursor
				_rtCursor = streamFLyr.Subscribe(null, true);

				//Waiting for new streamed features
				while (await _rtCursor.WaitForRowsAsync())
				{
					while (_rtCursor.MoveNext())
					{
						switch (_rtCursor.Current.GetRowSource())
						{
							case RealtimeRowSource.EventInsert:
								var featureId = (int)_rtCursor.Current["TrackID"];
								searchPoint = await ShowViewshed(mapView, featureId);
								continue;
							case RealtimeRowSource.EventDelete:
							default:
								continue;
						};
					}
				}
			});
		}

		private async void CreateLayer()
		{
			Map map = MapView.Active.Map;

			#region Create layer with create-params
			var flyrCreatnParam = new FeatureLayerCreationParams(new Uri(@"c:\data\world.gdb\cities"))
			{
				Name = "World Cities",
				IsVisible = false,
				MinimumScale = 1000000,
				MaximumScale = 5000,
				DefinitionFilter = new CIMDefinitionFilter()
				{
					DefinitionExpression = "Population > 100000",
					Name = "More than 100k"
				},
				RendererDefinition = new SimpleRendererDefinition()
				{
					SymbolTemplate = SymbolFactory.Instance.ConstructPointSymbol(CIMColor.CreateRGBColor(255, 0, 0), 8, SimpleMarkerStyle.Hexagon).MakeSymbolReference()
				}
			};

			var featureLayer = LayerFactory.Instance.CreateLayer<FeatureLayer>(flyrCreatnParam, map, LayerPosition.AutoArrange);
			#endregion

			#region Create a subtype group layer
			var subtypeGroupLayerCreateParam = new SubtypeGroupLayerCreationParams
			(
				new Uri(@"c:\data\SubtypeAndDomain.gdb\Fittings")
			);

			#region Define Subtype layers
			subtypeGroupLayerCreateParam.SubtypeLayers = new List<SubtypeFeatureLayerCreationParams>()
	  {
        //define first subtype layer with unique value renderer
        new SubtypeFeatureLayerCreationParams()
		{
		  SubtypeId = 1,
		  RendererDefinition = new UniqueValueRendererDefinition(new string[] { "type" })
		},

        //define second subtype layer with simple symbol renderer
        new SubtypeFeatureLayerCreationParams()
		{
		  SubtypeId = 2,
		  RendererDefinition = new SimpleRendererDefinition()
		  {
			  SymbolTemplate = SymbolFactory.Instance.ConstructPointSymbol(CIMColor.CreateRGBColor(255, 0, 0), 8, SimpleMarkerStyle.Hexagon).MakeSymbolReference()
		  }
		}
	  };
			#endregion

			#region Define additional parameters
			subtypeGroupLayerCreateParam.DefinitionFilter = new CIMDefinitionFilter()
			{
				Name = "IsActive",
				DefinitionExpression = "Enabled = 1"
			};
			subtypeGroupLayerCreateParam.IsVisible = true;
			subtypeGroupLayerCreateParam.MinimumScale = 50000;
			#endregion

			SubtypeGroupLayer subtypeGroupLayer =
			  LayerFactory.Instance.CreateLayer<SubtypeGroupLayer>(subtypeGroupLayerCreateParam, map);
			#endregion
		}

		private async Task<MapPoint> ShowViewshed(MapView mapView, int featureId)
		{
			MapPoint searchPoint = ((RealtimeFeature)_rtCursor.Current).GetShape() as MapPoint;
			searchPoint = GeometryEngine.Instance.Project(searchPoint, mapView.Map.SpatialReference) as MapPoint;
			var observer = new Camera(searchPoint.X, searchPoint.Y, 200, -22, searchPoint.SpatialReference, CameraViewpoint.LookAt);
			observer.ViewportHeight = 84;
			observer.ViewportWidth = 134;
			observer.Z = 100;
			observer.Pitch = 0;
			var viewshedAnalysis = new Viewshed(observer, 45, 60, 10, 600);

			if (_vwDict[featureId] != null) await mapView.RemoveExploratoryAnalysis(_vwDict[featureId]);
			_vwDict[featureId] = viewshedAnalysis;
			await mapView.AddExploratoryAnalysis(viewshedAnalysis);
			return searchPoint;
		}
	}
}

//   Copyright 2021 Esri
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at

//       https://www.apache.org/licenses/LICENSE-2.0

//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License. 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
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


namespace CustomToolSketchSymbology
{
	//The custom tool
	internal class CustomLineConstructionTool : MapTool
	{

		public CustomLineConstructionTool()
		{
			IsSketchTool = true;
			UseSnapping = true;
			// Select the type of construction tool you wish to implement.  
			// Make sure that the tool is correctly registered with the correct component category type in the daml 
			//SketchType = SketchGeometryType.Point;
			SketchType = SketchGeometryType.Line;
			// SketchType = SketchGeometryType.Polygon;
			//Gets or sets whether the sketch is for creating a feature and should use the CurrentTemplate.
			UsesCurrentTemplate = true;
			//Gets or sets whether the tool supports firing sketch events when the map sketch changes. 
			//Default value is false.
			FireSketchEvents = true;
		}

		/// <summary>
		/// Upon activating the custom tool, the segment symbology is modified in this method and the regular unselected vertices symbology is also modified.
		/// </summary>
		/// <param name="active"></param>
		/// <returns></returns>
		protected override Task OnToolActivateAsync(bool active)
		{
			return QueuedTask.Run(() =>
			{
				//Getting the current symbology options of the segment
				var segmentOptions = GetSketchSegmentSymbolOptions();
				//Modifying the primary color and the width of the segment symbology options
				var orange = new CIMRGBColor();
				orange.R = 255;
				orange.G = 165;
				orange.B = 0;
				segmentOptions.PrimaryColor = orange;
				segmentOptions.Width = 4;

				//Creating a new vertex symbol options instance with the values you want
				var vertexOptions = new VertexSymbolOptions(VertexSymbolType.RegularUnselected);
				var yellow = new CIMRGBColor();
				yellow.R = 255;
				yellow.G = 215;
				yellow.B = 0;
				var purple = new CIMRGBColor();
				purple.R = 148;
				purple.G = 0;
				purple.B = 211;
				vertexOptions.AngleRotation = 45;
				vertexOptions.Color = yellow;
				vertexOptions.MarkerType = VertexMarkerType.Star;
				vertexOptions.OutlineColor = purple;
				vertexOptions.OutlineWidth = 3;
				vertexOptions.Size = 5;

				try
				{
					//Setting the value of the segment symbol options
					SetSketchSegmentSymbolOptions(segmentOptions);
					//Setting the value of the vertex symbol options of the regular unselected vertices using the vertexOptions instance created above.
					SetSketchVertexSymbolOptions(VertexSymbolType.RegularUnselected, vertexOptions);
				}
				catch (Exception ex)
				{
					System.Diagnostics.Debug.WriteLine($@"Unexpected Exception: {ex}");
				}
			});
		}

		/// <summary>
		/// Called when the sketch finishes. This is where we will create the sketch operation and then execute it.
		/// </summary>
		/// <param name="geometry">The geometry created by the sketch.</param>
		/// <returns>A Task returning a Boolean indicating if the sketch complete event was successfully handled.</returns>
		protected override Task<bool> OnSketchCompleteAsync(Geometry geometry)
		{
			if (CurrentTemplate == null || geometry == null)
				return Task.FromResult(false);

			// Create an edit operation
			var createOperation = new EditOperation();
			createOperation.Name = string.Format("Create {0}", CurrentTemplate.Layer.Name);
			createOperation.SelectNewFeatures = true;

			// Queue feature creation
			createOperation.Create(CurrentTemplate, geometry);

			// Execute the operation
			return createOperation.ExecuteAsync();
		}
	}
}

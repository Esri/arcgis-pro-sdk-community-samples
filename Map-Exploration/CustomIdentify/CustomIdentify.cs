//   Copyright 2019 Esri
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
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;

namespace CustomIdentify
{
  /// <summary>
  /// Implementation of custom pop-up tool.
  /// </summary>
  class CustomIdentify : MapTool
  {
	/// <summary>
	/// Define the tool as a sketch tool that draws a rectangle in screen space on the view.
	/// </summary>
	public CustomIdentify()
	{
	  IsSketchTool = true;
	  SketchType = SketchGeometryType.Rectangle;
	  SketchOutputMode = SketchOutputMode.Screen;
	}
	/// <summary>
	/// Called when a sketch is completed.
	/// </summary>
	protected override async Task<bool> OnSketchCompleteAsync(Geometry geometry)
	{
	  var sb = new StringBuilder();
	  var popupContent = await QueuedTask.Run(() =>
	  {
		var popupContents = new List<PopupContent>();
		var mapView = MapView.Active;
		if (mapView != null)
		{
		  //Get the features that intersect the sketch geometry.
		  var features = mapView.GetFeatures(geometry).ToDictionary();
		  if (features.Count > 0)
		  {
			foreach (var kvp in features)
			{
			  var bfl = kvp.Key;
			  var oids = kvp.Value;
			  sb.AppendLine($@"{bfl}: {oids.Count} selected");
			  foreach (var objectID in oids)
			  {
				// for each MapMember/object id combo (within the geometry) 
				// create a DynamicPopupContent in the list of popupContents 
				popupContents.Add(new DynamicPopupContent(bfl, objectID));
			  }
			}
		  }
		}
		return popupContents;
	  });
	  var clickPoint = MouseCursorPosition.GetMouseCursorPosition();
	  var popupDef = new PopupDefinition()
	  {
		Append = true,      // if true new record is appended to existing (if any)
		Dockable = true,    // if true popup is dockable - if false Append is not applicable
		Position = clickPoint,  // Position of top left corner of the popup (in pixels)
		Size = new System.Windows.Size(200, 400)    // size of the popup (in pixels)
	  };
	  MessageBox.Show($@"Pop-up selection:{Environment.NewLine}{sb.ToString()}");
	  MapView.Active.ShowCustomPopup(popupContent, null, true, popupDef);
	  return true;
	}

  }
}

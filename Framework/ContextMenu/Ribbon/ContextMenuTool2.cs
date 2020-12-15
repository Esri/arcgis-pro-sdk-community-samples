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

namespace ContextMenu.Ribbon
{
	internal class ContextMenuTool2 : MapTool
	{
		public System.Windows.Point ClientPoint { get; set; }

		public ContextMenuTool2()
		{
			IsSketchTool = true;
			SketchType = SketchGeometryType.Rectangle;
			SketchOutputMode = SketchOutputMode.Map;
		}

		protected override Task OnToolActivateAsync(bool active)
		{
			return base.OnToolActivateAsync(active);
		}

		protected override void OnToolMouseDown(MapViewMouseButtonEventArgs e)
		{
			if (e.ChangedButton == System.Windows.Input.MouseButton.Right)
				e.Handled = true;
		}

		protected override Task HandleMouseDownAsync(MapViewMouseButtonEventArgs e)
		{
			this.ClientPoint = e.ClientPoint;
			ShowContextMenu();
			return Task.CompletedTask;
		}

		protected override Task<bool> OnSketchCompleteAsync(Geometry geometry)
		{
			return base.OnSketchCompleteAsync(geometry);
		}

		private void ShowContextMenu()
		{
			var contextMenu = FrameworkApplication.CreateContextMenu(
														 "ContextMenu_Menus_ToolContextMenu", () => ClientPoint);
			contextMenu.Closed += (o, e) =>
			{
				//TODO, any clean up associated with your context menu closing
			};
			contextMenu.IsOpen = true;
		}
	}
}

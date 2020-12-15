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
	/// <summary>
	/// Example 1 - shows use of a predefined menu in-conjunction with the
	/// map tool ContextMenuID
	/// </summary>
	internal class ContextMenuTool1 : MapTool
	{
		public ContextMenuTool1()
		{
			IsSketchTool = true;
			SketchType = SketchGeometryType.Rectangle;
			SketchOutputMode = SketchOutputMode.Map;
			//Get the menu id from the Config.daml <menu .../> declaration
			//No right-click behavior needed - uses built-in behavior
			//to show the menu
			this.ContextMenuID = "ContextMenu_Menus_ToolContextMenu";
		}

		protected override Task OnToolActivateAsync(bool active)
		{
			return base.OnToolActivateAsync(active);
		}

		protected override Task<bool> OnSketchCompleteAsync(Geometry geometry)
		{
			return base.OnSketchCompleteAsync(geometry);
		}
	}
}

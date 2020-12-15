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

namespace ContextMenu.Menus
{
	/// <summary>
	/// Boiler plate menu implementation from the SDK Menu Item Template&#42;
	/// </summary>
	/// <remarks>&#42;Code popping up a message box has been added</remarks>
	internal class ToolContextMenu_button1 : Button
	{
		protected override void OnClick()
		{
			MessageBox.Show(this.Caption, FrameworkApplication.ActiveTool.Caption);
		}
	}

	internal class ToolContextMenu_button2 : Button
	{
		protected override void OnClick()
		{
			MessageBox.Show(this.Caption, FrameworkApplication.ActiveTool.Caption);
		}
	}

	internal class ToolContextMenu_button3 : Button
	{
		protected override void OnClick()
		{
			MessageBox.Show(this.Caption, FrameworkApplication.ActiveTool.Caption);
		}
	}

}

/*

   Copyright 2020 Esri

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

namespace DemoUseSelection
{
  /// <summary>
  /// This sample shows two tools that are using keys to toggle into selection mode while the tool is active.<br/>
  /// The first is the use of ActivateSelectAsync in conjunction with UseSelection in a couple of example editing tools.
  /// The two tools both do the same thing - a Difference operation against any selected polygons intersected by the sketch.<p/>
  /// DifferenceTool 1 shows use of UseSelection = <b>true</b> and leverages built-in behavior of the SHIFT key to toggle the tool into selection mode.<br/>
  /// DifferenceTool 2 also uses UseSelection = <b>true</b> and a custom key - "W" to toggle into select mode. It also
  /// shows how to handle the SHIFT key to prevent the default behavior from interfering with the tool's use of "W".<br/>
  /// Developers can experiment by setting UseSelection to <b>false</b> to observe how that changes the behavior of the tools.<p/>
  /// The second thing demonstrated by the sample is the use of <b>Project.Current.IsEditingEnabled</b> in-conjunction
  /// with <b>Project.Current.SetIsEditingEnabledAsync(true | false)</b> to detect if a project has editing enabled and to toggle
  /// editing on|off. This is new at <b>2.6</b>
  /// </summary>
  /// <remarks>
  /// 1. In Visual Studio click the Build menu. Then select Build Solution.  
  /// 1. Click Start button to open ArcGIS Pro.
  /// 1. ArcGIS Pro will open. 
  /// 1. Open any project which features you can edit, for example: 'C:\Data\FeatureTest\FeatureTest.aprx' 
  /// 1. Click the Edit tab.  Notice that edit functions like create/modify are enabled.
  /// ![UI](Screenshots/Screen1.png)
  /// 1. Click the SEL tab.  Then click 'Disable Editing' to disable editing.
  /// ![UI](Screenshots/Screen2.png)
  /// 1. Click the Edit tab.  Notice that edit functions like create/modify are now disabled. 
  /// ![UI](Screenshots/Screen3.png)
  /// 1. Click the SEL tab.  Then click 'Enable Editing' to re-able editing.  Notice that the 'Difference' custom tools are now re-abled as well. 
  /// ![UI](Screenshots/Screen4.png)
  /// 1. To utilize the "Edit" button you have to set the "Disable/Enable Editing" option in ArcGIS Pro options as shown here.
  /// ![UI](Screenshots/Screen5.png)
  /// 1. Click 'Edit' to enable editing.  Notice that the 'Enable/Disable Editing' buttons honor the 'Edit' button status and vice versa.
  /// ![UI](Screenshots/Screen6.png)
  /// </remarks>
  internal class Module1 : Module
  {
	private static Module1 _this = null;

	public const string ExploreTool = "esri_mapping_exploreTool";
	/// <summary>
	/// Retrieve the singleton instance to this module here
	/// </summary>
	public static Module1 Current => _this ?? (_this = (Module1)FrameworkApplication.FindModule("DemoUseSelection_Module"));

	public void DeactivateSelf()
	{
	  FrameworkApplication.SetCurrentToolAsync(ExploreTool);
	}

	public bool IsShiftKey(MapViewKeyEventArgs k)
	{
	  return (k.Key == System.Windows.Input.Key.LeftShift ||
										   k.Key == System.Windows.Input.Key.RightShift);
	}

	public bool IsControlKey(MapViewKeyEventArgs k)
	{
	  return (k.Key == System.Windows.Input.Key.LeftCtrl ||
									   k.Key == System.Windows.Input.Key.RightCtrl);
	}

	public bool IsShiftOrControl(MapViewKeyEventArgs k)
	{
	  return IsShiftKey(k) || IsControlKey(k);
	}

	public bool IsShiftDown()
	{
	  return (Keyboard.Modifiers & ModifierKeys.Shift)
																   == ModifierKeys.Shift;
	}

	public bool IsControlDown()
	{
	  return (Keyboard.Modifiers & ModifierKeys.Control)
																   == ModifierKeys.Control;
	}

	public Tuple<bool, bool> IsShiftOrControlDown(MapViewKeyEventArgs k)
	{
	  return new Tuple<bool, bool>(IsShiftDown(), IsControlDown());
	}

	public Dictionary<FeatureLayer, List<long>> FilterSelection(
										Dictionary<BasicFeatureLayer, List<long>> selection)
	{
	  return FilterSelection(
					  selection.ToDictionary(k => k.Key as MapMember, k => k.Value));
	}

	public Dictionary<FeatureLayer, List<long>> FilterSelection(
										Dictionary<MapMember, List<long>> selection)
	{
	  return selection.Where(kvp =>
	  {
		if (kvp.Key is FeatureLayer fl)
		{
		  return fl.ShapeType == esriGeometryType.esriGeometryPolygon;
		}
		return false;
	  }).ToDictionary(k => (FeatureLayer)k.Key, k => k.Value);
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

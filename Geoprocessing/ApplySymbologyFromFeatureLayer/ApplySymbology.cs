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
using ArcGIS.Desktop.Core.Geoprocessing;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Extensions;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;

namespace ApplySymbologyFromFeatureLayer
{
  internal class ApplySymbology : Button
  {
	protected override async void OnClick()
	{
	  try
	  {
		IGPResult toolResult = await Execute_ApplySymbologyFromFeatureLayer();
		Geoprocessing.ShowMessageBox(toolResult.Messages, "GP Messages", toolResult.IsFailed ? GPMessageBoxStyle.Error : GPMessageBoxStyle.Default);
	  }
	  catch (Exception ex)
	  {
		MessageBox.Show($@"Exception thrown: {ex}");
	  }
	}

	public async Task<IGPResult> Execute_ApplySymbologyFromFeatureLayer()
	{
	  var environments = Geoprocessing.MakeEnvironmentArray(overwriteoutput: true);

	  var prj = Project.Current;
	  var map = MapView.Active;
	  if (map == null) throw new Exception("No active map found!");

	  var featLayers = map.Map.Layers.OfType<FeatureLayer>();

	  // the first layer in TOC is WITHOUT any symbology
	  FeatureLayer targetLayer = featLayers.ElementAt(0);

	  // the second layer HAS the symbology
	  // symbology from the 2nd layer will be applied to the first layer
	  FeatureLayer symbologyLayer = featLayers.ElementAt(1);

	  // Make sure you have fields with the POP2000 name.
	  var sourceField = "POP2000";
	  var targetField = "POP2000";
	  var fieldType = "VALUE_FIELD";
	  String fieldInfo = String.Format("{0} {1} {2}", fieldType, sourceField, targetField); // VALUE_FIELD NAME NAME")
	  MessageBox.Show(fieldInfo);
	  var parameters = Geoprocessing.MakeValueArray(targetLayer, symbologyLayer, fieldInfo); //, symbologyFields, updateSymbology);

	  // Does not Add outputs to Map as GPExecuteToolFlags.AddOutputsToMap is not used
	  GPExecuteToolFlags executeFlags = GPExecuteToolFlags.RefreshProjectItems | GPExecuteToolFlags.GPThread | GPExecuteToolFlags.AddToHistory;

	  IGPResult result = await Geoprocessing.ExecuteToolAsync("ApplySymbologyFromLayer_management", parameters, null, null, null, executeFlags);
	  return result;
	}
  }
}

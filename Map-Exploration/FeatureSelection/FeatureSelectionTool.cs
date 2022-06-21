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
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Framework.Dialogs;
using System.Windows.Input;
using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;

namespace FeatureSelection
{
  internal class FeatureSelectionTool : MapTool
  {
    private bool _ctrlPressed = false;
    private bool _shiftPressed = false;

    /// <summary>
    /// Specify the sketch geometry and sketch output type. Screen sketches allow you to perform interactive selection in 2D and 3D.
    /// </summary>
    public FeatureSelectionTool()
    {
      IsSketchTool = true;
      SketchType = SketchGeometryType.Rectangle;
      SketchOutputMode = SketchOutputMode.Screen;
    }

    /// <summary>
    /// Called when the sketch is finished.
    /// </summary>
    protected override async Task<bool> OnSketchCompleteAsync(ArcGIS.Core.Geometry.Geometry geometry)
    {
      //Get the instance of the ViewModel from the dock pane
      var featureSelVM = Module1.FeatureSelectionVM;
      if (featureSelVM == null || featureSelVM.SelectedLayer == null)
        return true;

      return await QueuedTask.Run(() =>
      {
        //Return all the features that intersect the sketch geometry
        var result = MapView.Active.GetFeatures(geometry);
        var layerSelection = result.ToDictionary().FirstOrDefault(kvp => kvp.Key == featureSelVM.SelectedLayer);

        //Clear the selection if no features where returned
        if (!result.ToDictionary().ContainsKey(featureSelVM.SelectedLayer))
        {
          featureSelVM.SelectedLayer.Select(null, SelectionCombinationMethod.Subtract);
          return true;
        }

        //Construct a query filter using the OIDs of the features that intersected the sketch geometry
        var oidList = result[featureSelVM.SelectedLayer];
        var oid = featureSelVM.SelectedLayer.GetTable().GetDefinition().GetObjectIDField();
        var qf = new ArcGIS.Core.Data.QueryFilter() { WhereClause = string.Format("({0} in ({1}))", oid, string.Join(",", oidList)) };

        //Add to the clause using the where clause specified in the dock pane.
        if (featureSelVM.WhereClause != "" && featureSelVM.WhereClause != null)
          qf.WhereClause += string.Format(" AND ({0})", featureSelVM.WhereClause);

        //Return if the expression is not valid.
        if (!featureSelVM.ValidateExpresion(false))
          return true;

        //Change the method depending on the hot keys that are pressed.
        var method = SelectionCombinationMethod.New;
        if (_ctrlPressed && _shiftPressed)
          method = SelectionCombinationMethod.And;
        else if (_ctrlPressed)
          method = SelectionCombinationMethod.Subtract;
        else if (_shiftPressed)
          method = SelectionCombinationMethod.Add;

        try
        {
          //Create the new selection
          featureSelVM.SelectedLayer.Select(qf, method);
        }
        catch (Exception){} //May occur if expression validates but is still invalid expression.
        return true;
      });
    }

    /// <summary>
    /// Set whether the ctrl or shift key is pressed.
    /// </summary>
    protected override void OnToolKeyDown(MapViewKeyEventArgs k)
    {
      if (k.Key == Key.LeftCtrl)
        _ctrlPressed = true;
      if (k.Key == Key.LeftShift)
        _shiftPressed = true;
    }

    /// <summary>
    /// Set whether the ctrl or shift key is pressed.
    /// </summary>
    protected override void OnToolKeyUp(MapViewKeyEventArgs k)
    {
      if (k.Key == Key.LeftCtrl)
        _ctrlPressed = false;
      if (k.Key == Key.LeftShift)
        _shiftPressed = false;
    }
  }
}

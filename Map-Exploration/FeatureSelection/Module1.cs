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
using System.Windows.Input;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using System.Threading.Tasks;
using ArcGIS.Core.Data;

namespace FeatureSelection
{
  /// <summary>
  /// This sample provides a map tool and a dock pane that allow you to create new selections and manipulate existing selections within the map.
  /// </summary>
  /// <remarks>
  /// 1. In Visual Studio click the Build menu. Then select Build Solution.
  /// 2. Launch the debugger to open ArcGIS Pro.
  /// 4. With a map view active go to the Add-In tab and click the Feature Selection button.
  /// 5. This will open the Feature Selection dock pane.
  /// 6. Select the layer of interest in the Combo box and in the list below it will show the object ids of the selected features.
  /// ![UI](Screenshots/screenshot1.png)
  /// 7. Select any of the object ids to see the attributes for that feature.
  /// 8. Click the select tool next to the layers combo box to interactively select features for that layer in the map.
  /// 9. Enter a Where clause in the "Select Where" edit box.
  /// 10. This where clause can be used to further narrow down the selection.
  /// ![UI](Screenshots/screenshot2.png)
  /// </remarks>
  internal class Module1 : Module
  {
    private static Module1 _this = null;
    private const string _dockPaneID = "FeatureSelection_FeatureSelectionDockPane";

    /// <summary>
    /// Retrieve the singleton instance to this module here
    /// </summary>
    public static Module1 Current
    {
      get
      {
        return _this ?? (_this = (Module1)FrameworkApplication.FindModule("FeatureSelection_Module"));
      }
    }

    #region Set As Expression

    /// <summary>
    /// Returns true if a new expression can be set
    /// </summary>
    internal bool CanSetAsExpression
    {
      get
      {
        var vm = FeatureSelectionVM;
        if (vm == null || vm.SelectedRow == null)
          return false;

        return GetFormattedExpression(vm.SelectedRow) != null;
      }
    }

    /// <summary>
    /// Sets the new expression using the selected field and corresponding attribute
    /// </summary>
    internal void SetAsExpression()
    {
      var vm = FeatureSelectionVM;
      vm.WhereClause = GetFormattedExpression(vm.SelectedRow);
    }

    #endregion

    #region Append To Expression
    
    /// <summary>
    /// Returns true if a the expression can be appended to
    /// </summary>
    internal bool CanAddToExpression
    {
      get
      {
        var vm = FeatureSelectionVM;
        if (vm == null || vm.SelectedRow == null)
          return false;

        return GetFormattedExpression(vm.SelectedRow) != null;
      }
    }

    /// <summary>
    /// Appends to the expression using the selected field and corresponding attribute
    /// </summary>
    internal void AddToExpression()
    {
      var vm = FeatureSelectionVM;
      if (vm.WhereClause == "")
        vm.WhereClause = GetFormattedExpression(vm.SelectedRow);
      else
        vm.WhereClause += string.Format(" AND {0}", GetFormattedExpression(vm.SelectedRow));
    }

    #endregion

    /// <summary>
    /// Gets a string representing a new clause using the information defined in the FieldAttributeInfo
    /// </summary>
    /// <param name="fieldAttribute"></param>
    private string GetFormattedExpression(FieldAttributeInfo fieldAttribute)
    {
      switch (fieldAttribute.FieldType)
	    {
        case FieldType.Double:
        case FieldType.Integer:
        case FieldType.Single:
        case FieldType.SmallInteger:
        case FieldType.OID:
          if (fieldAttribute.FieldValue == null)
            return string.Format("{0} is NULL", fieldAttribute.FieldName);
          else
            return string.Format("{0} = {1}", fieldAttribute.FieldName, fieldAttribute.FieldValue);
        case FieldType.String:
          if (fieldAttribute.FieldValue == null)
            return string.Format("{0} is NULL", fieldAttribute.FieldName);
          else
            return string.Format("{0} = {1}", fieldAttribute.FieldName, string.Format("'{0}'", fieldAttribute.FieldValue));
        default:
          return null;
	    }
    }

    /// <summary>
    /// Stores the instance of the Feature Selection dock pane viewmodel
    /// </summary>
    private static FeatureSelectionDockPaneViewModel _dockPane;
    internal static FeatureSelectionDockPaneViewModel FeatureSelectionVM
    {
      get
      {
        if (_dockPane == null)
        {
          _dockPane = FrameworkApplication.DockPaneManager.Find(_dockPaneID) as FeatureSelectionDockPaneViewModel;
        }
        return _dockPane;
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
  }
}

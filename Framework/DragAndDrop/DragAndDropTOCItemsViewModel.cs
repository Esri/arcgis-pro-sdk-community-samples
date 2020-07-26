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
using ArcGIS.Desktop.Framework.DragDrop;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using DragAndDrop.Helpers;

namespace DragAndDrop
{
  internal class DragAndDropTOCItemsViewModel : DockPane
  {
    private const string _dockPaneID = "DragAndDrop_DragAndDropTOCItems";
    private StringBuilder _sb = new StringBuilder();
    protected DragAndDropTOCItemsViewModel() { }

    /// <summary>
    /// Show the DockPane.
    /// </summary>
    internal static void Show()
    {
      DockPane pane = FrameworkApplication.DockPaneManager.Find(_dockPaneID);
      if (pane == null)
        return;

      pane.Activate();
    }
    #region Drag and Drop
    public override void OnDragOver(DropInfo dropInfo)
    {
      //default is to accept our data types
      dropInfo.Effects = DragDropEffects.All;
    }
    public override void OnDrop(DropInfo dropInfo)
    {
      if (dropInfo.HasTOCContent())//extension method
      {
        _sb.Clear();
        var mm = dropInfo.GetTOCContent();//extension method - returns all MapMembers being dropped
        _sb.AppendLine($"No of Map Members: {mm.Count}");
        //get just feature layers, for example
        var layers = dropInfo.GetTOCContentOfType<FeatureLayer>(); //extension method
        _sb.AppendLine($"No of FeatureLayers: {layers.Count}");

        //which, honestly, is the same as doing this...so take your pick...
        var layers2 = mm.OfType<FeatureLayer>()?.ToList() ?? new List<FeatureLayer>();
        //and then ditto for StandaloneTable, ElevationSurface, LabelClass, etc.

        //last, get the source map to which the map members belong
        var mapuri = dropInfo.GetMapUri();
        //99 times out of a 100 this will be the active view map....
        //but, if not, get the actual map (or scene) this way
        var map = FrameworkApplication.Panes.OfType<IMapPane>()?.Select(mp => mp.MapView.Map)
                       .Where(m => m.URI == mapuri).First();
        _sb.AppendLine($"Map Name: {map.Name}");
        _sb.AppendLine($"Map URI: {mapuri}");
        TOCContentInfo = _sb.ToString();
      }
      else //Data is not being dragged from the TOC
      {
        dropInfo.Handled = false;
        ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show($"Drag and drop only from the TOC here");
        return;
      }
    }


    #endregion

    /// <summary>
    /// Text shown near the top of the DockPane.
    /// </summary>
    private string _heading = "Drag and Drop from TOC";
    public string Heading
    {
      get { return _heading; }
      set
      {
        SetProperty(ref _heading, value, () => Heading);
      }
    }

    private string _tocContentInfo = "Drag and drop from TOC";
    public string TOCContentInfo
    {
      get { return _tocContentInfo; }
      set
      {
        SetProperty(ref _tocContentInfo, value, () => TOCContentInfo);
      }
    }
  }

  /// <summary>
  /// Button implementation to show the DockPane.
  /// </summary>
	internal class DragAndDropTOCItems_ShowButton : Button
  {
    protected override void OnClick()
    {
      DragAndDropTOCItemsViewModel.Show();
    }
  }
}

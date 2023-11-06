/*

   Copyright 2023 Esri

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       https://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

   See the License for the specific language governing permissions and
   limitations under the License.

*/
using ActiproSoftware.Windows.Extensions;
using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Core.Internal.CIM;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Extensions;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Events;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Internal.Mapping.CommonControls;
using ArcGIS.Desktop.Internal.Mapping.Controls.TextExpressionBuilder;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Desktop.Mapping.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace DockpaneWithLayerFieldSelection
{
  internal class DockpaneWithLayerFieldSelectionViewModel : DockPane
  {
    private const string _dockPaneID = "DockpaneWithLayerFieldSelection_DockpaneWithLayerFieldSelection";

    private ObservableCollection<FeatureLayer> _layers = new ();
    private ObservableCollection<string> _fieldNames = new ();
    private object _lock = new();

    protected DockpaneWithLayerFieldSelectionViewModel() 
    {
      BindingOperations.EnableCollectionSynchronization(_layers, _lock);
      BindingOperations.EnableCollectionSynchronization(_fieldNames, _lock);
      LayersAddedEvent.Subscribe(OnLayersAdded);
      LayersRemovedEvent.Subscribe(OnLayersRemoved);
      ActiveMapViewChangedEvent.Subscribe(OnActiveMapViewChanged);
      // just in case the map is already up
      if (MapView.Active != null)
      {
        ActiveMapViewChangedEventArgs args = new(MapView.Active, null);
        OnActiveMapViewChanged(args);
      }
    }

    #region Bindings

    public ObservableCollection<FeatureLayer> Layers
    {
      get { return _layers; }
    }

    private FeatureLayer _selectedLayer;
    public FeatureLayer SelectedLayer
    {
      get { return _selectedLayer; }
      set
      {
        SetProperty(ref _selectedLayer, value, () => SelectedLayer);
        FieldNames.Clear();
        if (SelectedLayer == null) return;
        QueuedTask.Run(() =>
        {
          foreach (FieldDescription fd in SelectedLayer?.GetFieldDescriptions())
          {
            string shapeField = SelectedLayer.GetFeatureClass().GetDefinition().GetShapeField();
            if (fd.Name == shapeField) continue;
            FieldNames.Add(fd.Name);
          }
        });
      }
    }

    public ObservableCollection<string> FieldNames
    {
      get { return _fieldNames; }
    }

    private string _selectedFieldName;
    public string SelectedFieldName
    {
      get { return _selectedFieldName; }
      set
      {
        SetProperty(ref _selectedFieldName, value, () => SelectedFieldName);
        Selection = string.Empty;
        if (SelectedFieldName == null) return;
        Selection = $@"{SelectedLayer.Name}.{SelectedFieldName}";
      }
    }

    private string _Selection;
    public string Selection
    {
      get => _Selection;
      set => SetProperty(ref _Selection, value);
    }

    /// <summary>
    /// Text shown near the top of the DockPane.
    /// </summary>
    private string _heading = "Show Layer and Field Selection";
    public string Heading
    {
      get => _heading;
      set => SetProperty(ref _heading, value);
    }

    #endregion

    #region Event Handlers

    /// <summary>
    /// The active map view changed, first clear layers and fields, then
    /// refresh with new layers if incomingView is not null
    /// </summary>
    /// <param name="args"></param>
    private void OnActiveMapViewChanged(ActiveMapViewChangedEventArgs args)
    {
      FieldNames.Clear();
      Layers.Clear();
      if (args.IncomingView != null)
      {
        var layers = args.IncomingView.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>();
        Layers.AddRange(layers);
      }
    }

    private void OnLayersRemoved(LayerEventsArgs args)
    {
      foreach (var layer in args.Layers.OfType<FeatureLayer>())
      {
        if (Layers.Contains(layer))
          Layers.Remove((FeatureLayer)layer);
      }
    }

    private void OnLayersAdded(LayerEventsArgs args)
    {
      foreach (var layer in args.Layers.OfType<FeatureLayer>())
      {
        Layers.Add((FeatureLayer)layer);
      }
    }

    #endregion

    /// <summary>
    /// Show the DockPane.
    /// </summary>
    internal static void Show()
    {
      DockPane pane = FrameworkApplication.DockPaneManager.Find(_dockPaneID);
      if (pane == null) return;
      pane.Activate();
    }
  }

  /// <summary>
  /// Button implementation to show the DockPane.
  /// </summary>
	internal class DockpaneWithLayerFieldSelection_ShowButton : Button
  {
    protected override void OnClick()
    {
      DockpaneWithLayerFieldSelectionViewModel.Show();
    }
  }
}

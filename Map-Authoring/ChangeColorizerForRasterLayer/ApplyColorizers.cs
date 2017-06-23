//   Copyright 2017 Esri
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at

//       http://www.apache.org/licenses/LICENSE-2.0

//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License. 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Mapping;
using System.Windows;
using MessageBox = ArcGIS.Desktop.Framework.Dialogs.MessageBox;
using ComboBox = ArcGIS.Desktop.Framework.Contracts.ComboBox;
using ArcGIS.Desktop.Framework.Threading.Tasks;

namespace ChangeColorizerForRasterLayer
{
  /// <summary>
  /// Represents the Apply Colorizers ComboBox.
  /// </summary>
  internal class ApplyColorizers : ComboBox
  {

    //Collection holding the applicable colorizers for the selected layer.
    IEnumerable<RasterColorizerType> _applicableColorizerList = null;

    BasicRasterLayer basicRasterLayer = null;

    /// <summary>
    /// Combo Box constructor. Make sure the combox box is enabled if raster layer is selected
    /// and subscribe to the layer selection changed event.
    /// </summary>
    public ApplyColorizers()
    {
      SelectedLayersChanged(new ArcGIS.Desktop.Mapping.Events.MapViewEventArgs(MapView.Active));
      ArcGIS.Desktop.Mapping.Events.TOCSelectionChangedEvent.Subscribe(SelectedLayersChanged);
    }

    /// <summary>
    /// Event handler for layer selection changes.
    /// </summary>
    private async void SelectedLayersChanged(ArcGIS.Desktop.Mapping.Events.MapViewEventArgs mapViewArgs)
    {
      // Clears the combo box items when layer selection changes. 
      Clear();

      // Checks the state of the active pane. 
      // Returns the active pane state if the active pane is not null, else returns null.
      State state = (FrameworkApplication.Panes.ActivePane != null) ? FrameworkApplication.Panes.ActivePane.State : null;

      if (state != null && mapViewArgs.MapView != null)
      {
        // Gets the selected layers from the current Map.
        IReadOnlyList<Layer> selectedLayers = mapViewArgs.MapView.GetSelectedLayers();

        // The combo box will update only if one layer is selected.
        if (selectedLayers.Count == 1)
        {
          // Gets the selected layer.
          Layer firstSelectedLayer = selectedLayers.First();

          // The combo box will update only if a raster layer is selected.
          if (firstSelectedLayer != null && (firstSelectedLayer is BasicRasterLayer || firstSelectedLayer is MosaicLayer))
          {
            // Gets the basic raster layer from the selected layer. 
            if (firstSelectedLayer is BasicRasterLayer)
              basicRasterLayer = (BasicRasterLayer)firstSelectedLayer;
            else if (firstSelectedLayer is MosaicLayer)
              basicRasterLayer = ((MosaicLayer)firstSelectedLayer).GetImageLayer() as BasicRasterLayer;

            // Initiates the combox box selected item.
            SelectedIndex = -1;

            // Updates the combo box with the corresponding colorizers for the selected layer.
            await UpdateCombo(basicRasterLayer);

            // Sets the combo box to display the first combo box item.
            SelectedIndex = 0;
          }
        }
      }
    }

    /// <summary>
    /// Destructor. Unsubscribe from the layer selection changed event.
    /// </summary>
    ~ApplyColorizers()
    {
      ArcGIS.Desktop.Mapping.Events.TOCSelectionChangedEvent.Unsubscribe(SelectedLayersChanged);
      Clear();
    }

    /// <summary>
    /// Updates the combo box with the colorizers that can be applied to the selected layer.
    /// </summary>
    /// <param name="activeMapView">Active MapView for the selected layer.</param>
    private async Task UpdateCombo(BasicRasterLayer basicRasterLayer)
    {
        try
        {
            await QueuedTask.Run(() =>
            {
              // Gets a list of raster colorizers that can be applied to the selected layer.
              _applicableColorizerList = basicRasterLayer.GetApplicableColorizers();
            });


            if (_applicableColorizerList == null)
            {
              Add(new ComboBoxItem("No colorizers found"));
              return;
            }

            // Adds new combox box item for how many colorizers found.
            Add(new ComboBoxItem($@"{_applicableColorizerList.Count()} Colorizer{(_applicableColorizerList.Count() > 1 ? "s" : "")} found"));

            //Iterates through the applicable colorizer collection to get the colorizer names, and add to the combo box.
            foreach (var rasterColorizerType in _applicableColorizerList)
            {
              // Gets the colorizer name from the RasterColorizerType enum.
              string ColorizerName = Enum.GetName(typeof(RasterColorizerType), rasterColorizerType);
              Add(new ComboBoxItem(ColorizerName));
            }
        }
        catch (Exception ex)
        {
          MessageBox.Show("Exception caught on Update combo box:" + ex.Message, "Exception", MessageBoxButton.OK, MessageBoxImage.Error);
        }

    }

    /// <summary>
    /// The on comboBox selection change event. 
    /// </summary>
    /// <param name="item">The newly selected combo box item</param>
    protected override async void OnSelectionChange(ComboBoxItem item)
    {
      if (_applicableColorizerList == null)
        MessageBox.Show("The applicable colorizer list is null.", "Colorizer Error:", MessageBoxButton.OK, MessageBoxImage.Error);

      if (item == null)
        MessageBox.Show("The combo box item is null.", "Combo box Error:", MessageBoxButton.OK, MessageBoxImage.Error);

      if (string.IsNullOrEmpty(item.Text))
        MessageBox.Show("The combo box item text is null or empty.", "Combo box Error:", MessageBoxButton.OK, MessageBoxImage.Error);

      try
      {
        // Gets the first selected layer in the active MapView.
        Layer firstSelectedLayer = MapView.Active.GetSelectedLayers().First();

        // Gets the BasicRasterLayer from the selected layer.
        if (firstSelectedLayer is BasicRasterLayer)
          basicRasterLayer = (BasicRasterLayer)firstSelectedLayer;
        else if (firstSelectedLayer is MosaicLayer)
          basicRasterLayer = ((MosaicLayer)firstSelectedLayer).GetImageLayer() as BasicRasterLayer;
        else
          MessageBox.Show("The selected layer is not a basic raster layer", "Select Layer Error:", MessageBoxButton.OK, MessageBoxImage.Error);

        // Gets the colorizer type from the selected combo box item.
        RasterColorizerType ColorizerType = _applicableColorizerList.FirstOrDefault(cn => Enum.GetName(typeof(RasterColorizerType), cn) == item.Text);

        // Applies the selected colorizer to the layer.
        switch (ColorizerType)
        {
          case RasterColorizerType.RGBColorizer:
            {
              // Sets the RGB colorizer to the selected layer.
              await ColorizerDefinitionVM.SetToRGBColorizer(basicRasterLayer);
              break;
            }
          case RasterColorizerType.StretchColorizer:
            {
              // Sets the Stretch colorizer to the selected layer.
              await ColorizerDefinitionVM.SetToStretchColorizer(basicRasterLayer);
              break;
            }
          case RasterColorizerType.DiscreteColorColorizer:
            {
              // Sets the Discrete Color colorizer to the selected layer.
              await ColorizerDefinitionVM.SetToDiscreteColorColorizer(basicRasterLayer);
              break;
            }
          case RasterColorizerType.ColormapColorizer:
            {
              // Sets the ColorMap colorizer to the selected layer.
              await ColorizerDefinitionVM.SetToColorMapColorizer(basicRasterLayer);
              break;
            }
          case RasterColorizerType.ClassifyColorizer:
            {
              // Sets the Classify colorizer to the selected layer.
              await ColorizerDefinitionVM.SetToClassifyColorizer(basicRasterLayer);
              break;
            }
          case RasterColorizerType.UniqueValueColorizer:
            {
              // Sets the Unique Value colorizer to the selected layer
              await ColorizerDefinitionVM.SetToUniqueValueColorizer(basicRasterLayer);
              break;
            }
          case RasterColorizerType.VectorFieldColorizer:
            {
              // Sets the Vector Field colorizer to the selected layer
              await ColorizerDefinitionVM.SetToVectorFieldColorizer(basicRasterLayer);
              break;
            }
        }
      }
      catch (Exception ex)
      {

        MessageBox.Show("Exception caught in OnSelectionChange:" + ex.Message, "Exception", MessageBoxButton.OK, MessageBoxImage.Error);
      }
    }

  }
}

//   Copyright 2018 Esri
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at

//       http://www.apache.org/licenses/LICENSE-2.0

//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License. 

using ArcGIS.Core.CIM;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using ComboBox = ArcGIS.Desktop.Framework.Contracts.ComboBox;


namespace ScientificDataStatisticalAnalysis
{

  internal class CellStatisticsComboBox : ComboBox
  {
    // The relative path of the raster function template. 
    // Note: Before using the add-in, please import the Scientific_data_calculation raster function template (RFT)
    // in the Project1 subcategory of the Project category on Raster Functions pane, save ArcGIS Pro project. 
    // The Scientific_data_calculation RFT can be located in the add-in's Visual Studio project folder.   
    public static string fileRelativePath = @"\RasterFunctionTemplates\Project1\Scientific_data_calculation.rft.xml";

    // Defines an instance that is used to save the default rendering rule of the selected layer. 
    CIMRenderingRule renderingRule_default = null;

    // Defines an instance for the selected combo box item. 
    public static ComboBoxItem selectedComboBoxItem = null;

    /// <summary>
    /// Enumeration of the operations provided in the combo box. 
    /// </summary>
    public enum CellStatistics_Operations
    {
      Majority = 38,
      Maximum = 39,
      Mean = 40,
      Median = 41,
      Minimum = 42,
      Minority = 43,
      Range = 47,
      StandardDeviation = 54,
      Sum = 55,
      Variety = 58,
      
      // Other operations that are also included in the cell statistics raster function.
      //MajorityIgnoreNoData = 66,
      //MaximumIgnoreNoData = 67,
      //MeanIgnoreNoData = 68,
      //MedianIgnoreNoData = 69,
      //MinimumIgnoreNoData = 70,
      //MinorityIgnoreNoData = 71,
      //RangeIgnoreNoData = 72,
      //StandardDeviationIgnoreNoData = 73,
      //SumIgnoreNoData = 74,
      //VarietyIgnoreNoData = 75
    }

    /// <summary>
    /// Constructor of the combo box. 
    /// Subscribes to the SelectedLayersChanges event.
    /// </summary>
    public CellStatisticsComboBox()
    {
      try
      {
        // Subscribes to the layer selection changed event.
        ArcGIS.Desktop.Mapping.Events.TOCSelectionChangedEvent.Subscribe(SelectedLayersChanged);
        SelectedLayersChanged(null);
      }
      catch (Exception ex)
      {
        MessageBox.Show("Exception caught on Updating combo box:" + ex.Message, "Exception", MessageBoxButton.OK, MessageBoxImage.Error);
      }

      // If DefQueryEditBox text changed, calls the onEditBoxChanged function. 
      DefQueryEditBox.TextChanged += onEditBoxChanged;

      // Initiates the combox box selected item.
      InitializeComboBox();

    }

    /// <summary>
    /// Initializes the combo box items. 
    /// </summary>
    private void InitializeComboBox()
    {
      // Resets the combo box.
      SelectedItem = -1;

      // Adds option to reset the layer to default status. 
      Add(new ComboBoxItem("None"));

      // Adds the values in the CellStatistics_Operations enumeration as the combo box items. 
      foreach (Enum _operation in Enum.GetValues(typeof(CellStatistics_Operations)))
      {
        // Adds the enum items by name. 
        string _operationName = Convert.ToString(_operation);
        Add(new ComboBoxItem(_operationName));
      }

      // Selects the first combo box item.
      //SelectedIndex = 0;
    }

    /// <summary>
    /// Event handler for layer selection change.
    /// </summary>
    /// <param name="mapViewArgs">An instance of the MapViewEventArgs.</param>
    private async void SelectedLayersChanged(ArcGIS.Desktop.Mapping.Events.MapViewEventArgs mapViewArgs)
    {
      // Check if there is an active map view.
      if (MapView.Active != null)
      {
        // Gets the selected layers from the current Map.
        IReadOnlyList<Layer> selectedLayers = MapView.Active.GetSelectedLayers();

        // The combo box will update only if one layer is selected.
        if (selectedLayers.Count == 1)
        {
          // Gets the selected layer.
          Layer firstSelectedLayer = selectedLayers.First();

          // Make sure the selected layer is an image service layer. 
          if (firstSelectedLayer != null && firstSelectedLayer is ImageServiceLayer)
          {
            // Initiates the combox box selected item.
            //InitializeComboBox();

            await QueuedTask.Run(() =>
            {
              // Get and store the rendering rule of the selected image service layer.
              renderingRule_default = (firstSelectedLayer as ImageServiceLayer).GetRenderingRule();
            });
          }
        }
      }
      else
        MessageBox.Show("There is no active map.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
    }

    /// <summary>
    /// Called when the combo box item selection changed.
    /// </summary>
    /// <param name="item"> The selected combo box item. </param>
    protected override async void OnSelectionChange(ComboBoxItem item)
    {
      // Sets logic if selected combo box item is null, then return.
      if (item == null)
        return;

      // Passes the current selected combo box item to the selectedComboBoxItem. 
      selectedComboBoxItem = item;

      // Adds validation here if the item text is emply then return.
      if (string.IsNullOrEmpty(item.Text))
        return;

      // Try and get the first selected layer.
      Layer firstSelectedLayer = null;
      try { firstSelectedLayer = MapView.Active.GetSelectedLayers().First(); } catch (Exception) { }
      // Check if there are any selected layers and if the first selected layer is a image service layer.
      if (!(firstSelectedLayer !=null && firstSelectedLayer is ImageServiceLayer))
      {  
        MessageBox.Show("Please select an image service layer.");
        return;
      }
      ImageServiceLayer selectedLayer = firstSelectedLayer as ImageServiceLayer;

      // Enters if the selected combo box item is not 'defult' or empty, else sets the original rendering rule to the selected layer. 
      if (item.Text != "None" && item.Text !=null)
      {
        // Gets the operation enum item from the its name. 
        CellStatistics_Operations operation = (CellStatistics_Operations)Enum.Parse(typeof(CellStatistics_Operations), item.Text);
        try
        {
          string rftFilePath = Project.Current.HomeFolderPath + fileRelativePath;
          // Customizes the raster function template XML file using the user defined definition query and operation. 
          string xmlFilePath = Process.CustomRFTXmlFile(rftFilePath, operation, DefQueryEditBox.passingText);
          // Applies the custom raster function template to the selected layer.  
          await Process.ApplyRFTXmlFile(selectedLayer, operation, xmlFilePath);
        }
        catch (Exception ex)
        {
          MessageBox.Show("Exception caught in OnSelectionChange:" + ex.Message, "Exception", MessageBoxButton.OK, MessageBoxImage.Error);
        }
      }
      else
      {
          await QueuedTask.Run(() =>
          {
            // Sets the defult rendering rule to the selected image service layer.
            selectedLayer.SetRenderingRule(renderingRule_default);
          });
      }
    }

    /// <summary>
    /// Called if the text is changed in Definition query edit box.  
    /// </summary>
    /// <param name="source"></param>
    /// <param name="e"></param>
    public void onEditBoxChanged(object source, TextEventArgs e)
    {
      // When the edit box text updated, applies the rendering rule to the selected layer again. 
      OnSelectionChange(selectedComboBoxItem);

    }
  }
}

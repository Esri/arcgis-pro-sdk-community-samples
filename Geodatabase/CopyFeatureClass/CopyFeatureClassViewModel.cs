/*

   Copyright 2025 Esri

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
using ArcGIS.Desktop.KnowledgeGraph;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using ArcGIS.Core.Data.DDL;
using ArcGIS.Desktop.Mapping.Events;

namespace CopyFeatureClass
{
  internal class CopyFeatureClassViewModel : DockPane
  {
    private const string _dockPaneID = "CopyFeatureClass_CopyFeatureClass";

    protected CopyFeatureClassViewModel() 
    {
      // Subscribe to the TOC selection changed event to update the status
      _ = TOCSelectionChangedEvent.Subscribe((args) =>
      {
        if (args.MapView == null) return;
        var sourceLayer = args.MapView.GetSelectedLayers().FirstOrDefault();
        if (sourceLayer == null)
        {
          Status = "Select a feature layer to be copied in the map's table of content";
          return;
        }
        Status = $@"Selected layer to be copied: {sourceLayer.Name}";
        Status += Environment.NewLine;
        if (string.IsNullOrEmpty(DestinationFeatureClassName))
          Status += "Specify the destination feature class name";
        else
          Status += $@"Destination feature class name: {DestinationFeatureClassName}";
        CopyButtonVisibility = sourceLayer != null
          && !string.IsNullOrEmpty(DestinationFeatureClassName) ? System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden;

      });
      Status = "Select a feature layer to be copied in the map's table of content";
      if (MapView.Active != null)
      {
        var sourceLayer = MapView.Active.GetSelectedLayers().FirstOrDefault();
        if (sourceLayer != null)
        {
          Status = $@"Selected layer to be copied: {sourceLayer.Name}";
        }
      }
      CopyButtonVisibility = System.Windows.Visibility.Hidden;
    }

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

    private string _DestinationFeatureClassName;

    public string DestinationFeatureClassName
    {
      get { return _DestinationFeatureClassName; }
      set
      {
        SetProperty(ref _DestinationFeatureClassName, value);
        var sourceLayer = MapView.Active.GetSelectedLayers().FirstOrDefault();
        if (sourceLayer == null)
        {
          Status = "Select a feature layer to be copied in the map's table of content";
        }
        else Status = $@"Selected layer to be copied: {sourceLayer.Name}";
        Status += Environment.NewLine;
        if (string.IsNullOrEmpty(value))
          Status += "Specify the destination feature class name";
        else
          Status += $@"Destination feature class name: {value}";
        CopyButtonVisibility = sourceLayer != null 
          && !string.IsNullOrEmpty(value) ? System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden;
      }
    }

    private string _Status;

    public string Status
    {
      get { return _Status; }
      set
      {
        SetProperty(ref _Status, value);
      }
    }

    private System.Windows.Visibility _CopyButtonVisibility = System.Windows.Visibility.Visible;

    public System.Windows.Visibility CopyButtonVisibility
    {
      get { return _CopyButtonVisibility; }
      set
      {
        SetProperty(ref _CopyButtonVisibility, value);
      }
    }

    public ICommand CmdCopy => new RelayCommand(async () =>
    {
      try
      {
        var sourceLayer = MapView.Active.GetSelectedLayers().FirstOrDefault();
        if (sourceLayer == null || !(sourceLayer is FeatureLayer featureLayer))
        {
          MessageBox.Show("Select the feature layer to be copied in the Map's TOC.");
          return;
        }
        await CreateCopyFeatureClass(featureLayer, DestinationFeatureClassName);
      }
      catch (Exception ex)
      {
        MessageBox.Show($@"Error: {ex.Message}");
      }
    }, () => true);

    private async Task CreateCopyFeatureClass(FeatureLayer featureLayer, string destinationFeatureClassName)
    {
      // implement the create F/C logic here ... use the selected layer as source and create the destination
      // feature class using the source F/C schema
      var isOk = await QueuedTask.Run<bool>(() =>
      {
        var LayerDef = featureLayer.GetFeatureClass().GetDefinition();
        using Geodatabase geodatabase = new(new FileGeodatabaseConnectionPath(new Uri(Project.Current.DefaultGeodatabasePath)));
        // Creating the attribute fields
        FeatureClassDefinition originalFeatureClassDefinition = featureLayer.GetFeatureClass().GetDefinition();
        FeatureClassDescription originalFeatureClassDescription = new(originalFeatureClassDefinition);
        FeatureClassDescription LayerDescription = new(destinationFeatureClassName, originalFeatureClassDescription.FieldDescriptions, originalFeatureClassDescription.ShapeDescription);
        SchemaBuilder schemaBuilder = new(geodatabase);
        schemaBuilder.Create(LayerDescription);
        bool success = schemaBuilder.Build();
        return success;
      });
      if (!isOk)
      {
        MessageBox.Show($@"Failed to create {destinationFeatureClassName}");
        return;
      }  
      else
      {
        Status += Environment.NewLine + $@"Created feature class: {System.IO.Path.Combine (Project.Current.DefaultGeodatabasePath, destinationFeatureClassName)}";
      }
      // add the new FeatureClass to the map
      var newLyr = await QueuedTask.Run(() =>
      {
        using Geodatabase geodatabase = new(new FileGeodatabaseConnectionPath(new Uri(Project.Current.DefaultGeodatabasePath)));
        var newFc = geodatabase.OpenDataset<FeatureClass>(destinationFeatureClassName);
        return LayerFactory.Instance.CreateLayer<FeatureLayer>(new FeatureLayerCreationParams(newFc) { Name = $@"Copied: {destinationFeatureClassName}" }, MapView.Active.Map);
      });
      // copy all data
      var copyResult = await QueuedTask.Run<(bool hasEdits, uint rowCount)>(() =>
      {
        // create an edit operation
        EditOperation copyOperation = new EditOperation()
        {
          Name = "Copy Data",
          ProgressMessage = "Working...",
          CancelMessage = "Operation canceled.",
          ErrorMessage = $@"Error copying data to {featureLayer.Name}",
          SelectModifiedFeatures = false,
          SelectNewFeatures = false
        };
        using var rowCursor = featureLayer.Search();
        var fieldMap = new FieldMap(featureLayer);
        var rowCount = 0u;
        while (rowCursor.MoveNext())
        {
          using (var row = rowCursor.Current as Feature)
          {
            var geom = row.GetShape().Clone();
            if (geom == null)
              continue;
            var newAttributes = fieldMap.GetAttributeDict(row);
            copyOperation.Create(newLyr, geom, newAttributes);
            rowCount++;
          }
        }
        // execute the operation only if changes where made
        if (!copyOperation.IsEmpty
            && !copyOperation.Execute())
        {
          var msg = $@"Copy operation to {featureLayer.Name} failed {copyOperation.ErrorMessage}";
          MessageBox.Show(msg);
          Status += Environment.NewLine + msg;
          return (false, 0);
        }
        return (true, rowCount);
      });
      if (copyResult.hasEdits)
      {
        // save all edits
        await Project.Current.SaveEditsAsync();
        Status += Environment.NewLine + $@"Copied {copyResult.rowCount} features to {destinationFeatureClassName}";
      }
    }

    /// <summary>
    /// Text shown near the top of the DockPane.
    /// </summary>
    private string _heading = "Using the Selected F/C Layer as Input\nSpecify the Destination F/C name";
    public string Heading
    {
      get => _heading;
      set => SetProperty(ref _heading, value);
    }

    #region Image Sources

    /// <summary>
    /// Copy Snippet button imagesource
    /// </summary>
    public ImageSource CopyImageSrc
    {
      get
      {
        var imageSource = System.Windows.Application.Current.Resources["SelectionCopySelectedFeatures16"] as ImageSource;
        return imageSource;
      }
    }

    #endregion Image Sources
  }


  /// <summary>
  /// Button implementation to show the DockPane.
  /// </summary>
  internal class CopyFeatureClass_ShowButton : Button
  {
    protected override void OnClick()
    {
      CopyFeatureClassViewModel.Show();
    }
  }
}

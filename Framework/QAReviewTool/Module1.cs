/*

   Copyright 2019 Esri

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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using System.Threading.Tasks;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Desktop.Mapping.Events;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Core.Data;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Editing.Attributes;
using ArcGIS.Desktop.Core.Geoprocessing;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Core.Data.DDL;

namespace QAReviewTool
{
  /// <summary>
  /// This sample is a simplified data quality assurance(QA) review workflow.  It provides a set of controls guiding an operator through a data quality assurance review workflow to visually inspecting each feature and optionally add a QA notation to the record.
  /// </summary>
  /// <remarks>
  /// 1. Download the Community Sample data(see under the 'Resources' section for downloading sample data). Make sure that the Sample dataset is unzipped under c:\data and that the sample data contains the folder C:\Data\QAReviewTool containing a map package named "QA_Review_Tool.ppkx" which is required for this sample.
  /// 1. Open this solution in Visual Studio.
  /// 1. Click the build menu and select Build Solution.
  /// 1. Click the Start button to run the solution.  ArcGIS Pro will open.
  /// 1. Open the map package "QA_Review_Tool.ppkx" located in the "C:\Data\QAReviewTool" folder.  This project contains all required data.  Be sure the Topographic basemap layer is displayed.
  /// 1. Click on the "Review" tab provided by the add-in.  In the "Review Layer Selection" group, click the dropdown button for the "Layer" combobox and select "La_Jolla_Roads".  This is the feature class to be reviewed.  Press the "Start Feature Review" button, which enables the comboboxes in the controls in the "Feature Review Field Settings" group.  You can also click the "Open Attribute Table" button to view the selected table's attribute table.
  /// ![UI](Screenshot/Screenshot1.png)
  /// 1. In the "Feature Review Field Settings" group, click on the dropdown button for the "Value Field" and scroll to the bottom of the list and choose the field "review_code".  This will populate the "Value" field combobox with the unique data values from the review_code field.  It will then zoom to the feature containing the value "1" in the "Value" field.  The forward and backward navigation buttons are now enabled, and you can click on these buttons to zoom to the different records containing other review_code values.  Click on "Show Selected Records" in the attribute table to just show the selected record(s).
  /// ![UI](Screenshot/Screenshot2.png)
  /// 1. In the "Note Field" combobox, choose the "QA_NOTE" from the list of available field names to store your QA notes.  If the entered field name doesn't exist, the add-in prompts before adding the field to your table.  If you chose to add the QA_NOTE field to your schema check the attribute table to verify the addition.  The field , and is also listed in the Note Field combobox.  This step will enable the rest of the controls on the Review tab.
  /// ![UI](Screenshot/Screenshot3.png)
  /// 1. You are now able to use the enabled Note Shortcuts to apply a brief description to any selected records.Use the "Forward" button to navigate to review_code value 3 and you will see three road features selected.  As the Topographic basemap layer does not show a street for these features, change the basemap to Imagery to see verify the data.  You will see the features represent the entrance driveway to a building and its parking lot.  Press the "Correct" note shortcut button to set the QA review value.
  /// ![UI](Screenshot/Screenshot4.png)
  /// 1. Next, you can try applying custom notes and managing your edits using the controls in the "Custom Notes" group.You can load a set of custom notes from a textfile and use these in place of the note shortcuts.Press the "Load Note File" button and navigate to the C:\Data\QAReviewTool folder.Click on the file "Custom Notes.txt" and press OK.
  /// ![UI](Screenshot/Screenshot5.png)
  /// 1. The values will be loaded into the "Note Text" combobox.You can then select from these values and apply them to your current selected records using the "Save Notation" button.You can also add new values to the Note Text combobox and save the values to a new note file.Finally, you can use the Undo, Save and Discard buttons to manage your edits.
  /// ![UI](Screenshot/Screenshot6.png)
  /// </remarks>
  internal class Module1 : Module
  {

    private static Module1 _this = null;

    /// <summary>
    /// Retrieve the singleton instance to this module here
    /// </summary>
    public static Module1 Current
    {
      get
      {
        return _this ?? (_this = (Module1)FrameworkApplication.FindModule("QAReviewTool_Module"));
      }
    }

    public Module1()
    {
      // this constructor is getting called at startup of Pro
      // in order to load the layer list combobox 
      // we subscribe to the active mapview changed event which occurs 
      // when a map view has changed
      ActiveMapViewChangedEvent.Subscribe(OnActiveMapViewChanged);
    }

    #region Event Listening

    private MapView _currentActiveMapView = null;

    private void OnActiveMapViewChanged(ActiveMapViewChangedEventArgs activeMapViewChangedEventArgs)
    {
      try
      {
        // We are only interested in the current mapview, when focus changes,
        // we do not refresh if we are still on the current active mapview
        if (activeMapViewChangedEventArgs.IncomingView == null) return;
        if (activeMapViewChangedEventArgs.IncomingView == _currentActiveMapView) return;
        _currentActiveMapView = activeMapViewChangedEventArgs.IncomingView;

        // we now have a new mapview, refresh the layer list combobox
        InitializeLayerComboBox();
      }
      catch (Exception ex)
      {
        MessageBox.Show("Error in OnActiveMapViewChanged:  " + ex.ToString(), "Error");
      }
    }

    #endregion Event Listening

    #region Business Logic: Layer Handling

    // These properties are used by the ribbon controls
    private LayerListComboBox _layerListComboBox;
    public LayerListComboBox LayerComboBox
    {
      get { return _layerListComboBox; }
      set
      {
        _layerListComboBox = value;
        InitializeLayerComboBox();
      }
    }

    // if true the layer list was initialized
    private bool _LayerHasBeenDefined = false;

    /// <summary>
    /// The layer combo box is initialized with the layers in the current map
    /// </summary>
    private void InitializeLayerComboBox()
    {
      if (_LayerHasBeenDefined)
        ClearSession();
      // the UI is not running yet
      if (LayerComboBox == null)
        return;
      LayerComboBox.ClearItems();
      Map map = MapView.Active?.Map;
      if (map == null)
        return;

      var layers = map.GetLayersAsFlattenedList().OfType<FeatureLayer>();
      bool hasItems = false;
      foreach (var lyr in layers)
      {
        string fc = lyr.Name;
        LayerComboBox.AddItem(new ComboBoxItem(fc));
        hasItems = true;
      }
      if (hasItems)
      {
        LayerComboBox.Text = "Choose...";
      }
      FrameworkApplication.State.Deactivate("CanStartReview");
      _LayerHasBeenDefined = true;
    }

    public bool LayerHasBeenInitialized
    {
      get { return _LayerHasBeenDefined; }
      set { _LayerHasBeenDefined = value; }
    }

    private ComboBoxItem _LayerComboboxItemSelected;
    public ComboBoxItem LayerComboboxItemSelected
    {
      get
      {
        return _LayerComboboxItemSelected;
      }
      internal set
      {
        _LayerComboboxItemSelected = value;
        ClearSession();

        FrameworkApplication.State.Activate("CanStartReview");
      }
    }

    /// <summary>
    /// If a session was started, clear all the comboboxes and deactivate the states
    /// </summary>
    public void ClearSession()
    {
      if (LayerHasBeenInitialized)
      {
        if (Project.Current.HasEdits)
        {
          MessageBox.Show("You have ended your current review session. You have unsaved edits.");
        }
        // clear the comboboxes
        NotationNoteComboBox.ClearItems();
        LayerFieldComboBox.ClearItems();
        FieldValueComboBox.ClearItems();

        // reset states to deactivated
        FrameworkApplication.State.Deactivate("LayerSelectedState");
        FrameworkApplication.State.Deactivate("CanStartReview");
        FrameworkApplication.State.Deactivate("NavigationQueryDefined");
        FrameworkApplication.State.Deactivate("QANotationDefined");

        LayerHasBeenInitialized = true;
      }
    }

    /// <summary>
    /// This refresh method is called when the active map view changes and therefore the list of layers needs to be refreshed
    /// </summary>
    public void RefreshLayerComboBox()
    {
      try
      {
        if (LayerComboBox == null)
        {
          _LayerHasBeenDefined = true;
          return;
        }
      }
      catch (Exception ex)
      {
        MessageBox.Show("Error in RefreshLayerComboBox:  " + ex.ToString(), "Error");
      }
    }

    /// <summary>
    /// This method is called when the Layer refresh button is clicked
    /// </summary>
    internal void LayerRefreshClicked()
    {
      try
      {
        if (string.IsNullOrEmpty(LayerComboBox.Text)
          || LayerComboBox.Text == "Choose...")
        {
          MessageBox.Show("First select a layer from the list.", "Select a layer");
          return;
        }
        // Populate the Field combobox with the list of fields from the selected layer
        RefreshLayerFieldComboBox();

        ClearValueComboBox();
      }
      catch (Exception ex)
      {
        MessageBox.Show("Error in LayerRefreshClicked:  " + ex.ToString(), "Error");
      }
    }

    public void OpenAttributeTable()
    {
      try
      {
        if (LayerComboBox.SelectedItem == null || LayerComboBox.SelectedItem.ToString() == "")
        {
          MessageBox.Show("First select a layer from the list.", "Select a layer");
          return;
        }
        // Get the layer and create a list from all the values
        var QALayer = MapView.Active.Map.FindLayers((LayerComboBox.SelectedItem).ToString()).FirstOrDefault() as FeatureLayer;
        if (QALayer == null) return;

        FrameworkExtender.OpenTablePane(FrameworkApplication.Panes, QALayer, TableViewMode.eAllRecords);

      }
      catch (Exception ex)
      {
        MessageBox.Show("Error in OpenAttributeTable:  " + ex.ToString(), "Error");
      }
    }

    #endregion Business Logic: Layer Handling

    #region Business Logic: QA Field Handling

    private QAFieldListComboBox _qaFieldComboBox;
    public QAFieldListComboBox QAFieldComboBox
    {
      get
      {
        return _qaFieldComboBox;
      }
      set
      {
        var oldValue = _qaFieldComboBox;
        _qaFieldComboBox = value;
        if (oldValue == null)
          InitializeQAFieldComboBox();
      }
    }

    public NavigateFieldNameListCombo LayerFieldComboBox { get; set; }
    public NavigateValueListCombo FieldValueComboBox { get; set; }

    public void InitializeQAFieldComboBox()
    {
      try
      {
        if (QAFieldComboBox == null || QAFieldComboBox.ItemCollection.Count > 0)
          return;
        QAFieldComboBox.AddItem(new ComboBoxItem("QA_Notes"));
        QAFieldComboBox.AddItem(new ComboBoxItem("ReviewerNotes"));
        QAFieldComboBox.AddItem(new ComboBoxItem("QA_NOTE"));
        QAFieldComboBox.AddItem(new ComboBoxItem("REV_NOTE"));
      }
      catch (Exception ex)
      {
        MessageBox.Show("Error in RefreshQAFieldComboBox:  " + ex.ToString(), "Error");
      }
    }

    private Selection QALayerSelection;
    public void RefreshLayerFieldComboBox()
    {
      try
      {
        LayerFieldComboBox.ClearItems();
        QueuedTask.Run(() =>
        {
          var LayerName = LayerComboBox.SelectedItem.ToString();
          // Get all the fields from the selected item in LayerComboBox
          // Get the layer in the ComboBox selection
          var QALayer = MapView.Active.Map.FindLayers(LayerName).FirstOrDefault() as FeatureLayer;
          if (QALayer == null) return;
          var FieldsList = QALayer.GetTable().GetDefinition().GetFields();  // as string
                                                                            // set default to the ObjectID field
          ComboBoxItem oidCbItem = null;
          foreach (var fld in FieldsList)
          {
            if (fld.FieldType == FieldType.Geometry) continue; // skip the shape field (geometry)
            if (fld.FieldType == FieldType.OID) oidCbItem = new ComboBoxItem(fld.Name);
            LayerFieldComboBox.AddItem(new ComboBoxItem(fld.Name));
          }
          LayerFieldComboBox.Text = oidCbItem.Text;

          // set the selected set which is then used for navigation
          // Zoom to either the selected features, or the extent of all layers in the map
          var featureLayers = MapView.Active.Map.Layers.OfType<FeatureLayer>().Where((featurelayer) => featurelayer.Name == LayerComboBox.SelectedItem.ToString());
          QALayerSelection = QALayer.Select(); // select all features
          var selectionSet = QALayerSelection.GetObjectIDs();
          if (QALayerSelection.GetCount() > 0)
          {
            MapView.Active.ZoomTo(QALayer, selectionSet, TimeSpan.FromSeconds(0));
            MapView.Active.ZoomOutFixed(TimeSpan.FromSeconds(0));
          }
          else
          {
            MapView.Active.ZoomTo(QALayer);
          }

          if (oidCbItem != null)
            LayerFieldComboBox.SelectedItem = oidCbItem;

        });
      }
      catch (Exception ex)
      {
        MessageBox.Show("Error in RefreshLayerFieldComboBox:  " + ex.ToString(), "Error");
      }
    }

    public void ClearValueComboBox()
    {
      FieldValueComboBox.ClearItems();
    }


    /// <summary>
    /// Populates the FieldValueComboBox with the unique values from the selected field
    /// </summary>
    /// <param name="fieldName"></param>
    /// <returns></returns>
    public async Task<string> RefreshFieldValueComboBoxAsync(string fieldName)
    {
      try
      {
        FieldValueComboBox.ClearItems();
        var result = await QueuedTask.Run<string>(() =>
        {
          // Get the layer and create a list from all the values
          var QALayer = MapView.Active.Map.FindLayers((LayerComboBox.SelectedItem).ToString()).FirstOrDefault() as FeatureLayer;
          if (QALayer == null) return string.Empty;

          // a QA layer was selected -> get the field values too
          List<object> QALayerCodeList = new() { };
          RowCursor QARowCursor = QALayerSelection.Search();
          using (QARowCursor)
          {
            while (QARowCursor.MoveNext())
            {
              using Row currentRow = QARowCursor.Current;
              QALayerCodeList.Add(currentRow[fieldName]);
            }
          }
          QALayerCodeList.Sort();
          var isEmpty = true;
          // Get unique values in the list
          foreach (object item in QALayerCodeList.Distinct())
          {
            if (item == null) continue;
            var str = item.ToString();
            // add to combobox
            FieldValueComboBox.AddItem(new ComboBoxItem(str));
            isEmpty = false;
          }
          if (isEmpty) return string.Empty;
          FieldValueComboBox.SelectedItem = FieldValueComboBox.ItemCollection.FirstOrDefault();
          FieldValueComboBox.Text = FieldValueComboBox.ItemCollection.FirstOrDefault().ToString();

          // Select the recordset by this first value in the list 
          object selectionvalue = FieldValueComboBox.SelectedItem;
          SelectByValue(selectionvalue);
          return string.Empty;
        });
        return result;
      }
      catch (Exception ex)
      {
        return $@"Error in RefreshFieldValueComboBox: {ex.ToString()}";
      }
    }

    #endregion Business Logic: QA Field Handling

    #region Business Logic: Navigation Handling


    public void SelectNextValue(bool forwardDirection)
    {
      try
      {
        int index = FieldValueComboBox.SelectedIndex;
        int lastindex = FieldValueComboBox.ItemCollection.Count - 1;
        if (forwardDirection == false)
        {
          // if current item is the minimum value in itemcollection, then go to the last
          if (index == 0)
          {
            FieldValueComboBox.SelectedIndex = lastindex;
          }
          else
          {
            FieldValueComboBox.SelectedIndex = (index - 1);
          }
        }
        if (forwardDirection == true)
        {
          // if current item is the minimum value in itemcollection, then go to the last
          if (index == lastindex)
          {
            FieldValueComboBox.SelectedIndex = (lastindex - index);
          }
          else
          {
            FieldValueComboBox.SelectedIndex = (index + 1);
          }
        }
      }
      catch (Exception ex)
      {
        MessageBox.Show("Error in SelectNextValue:  " + ex.ToString(), "Error");
      }
    }

    #endregion Business Logic: Navigation Handling

    #region Business Logic: Edit QA Field Handling
    public NotationNoteComboBox NotationNoteComboBox { get; set; }

    public void OnEnterSearchValue()
    {
      try
      {
        object selectionvalue = FieldValueComboBox.Text;
        SelectByValue(selectionvalue);
      }
      catch (Exception ex)
      {
        MessageBox.Show("Error in OnEnterSearchValue:  " + ex.ToString(), "Error");
      }
    }

    public void SelectByValue(object selectionvalue)
    {
      try
      {
        QueuedTask.Run(() =>
        {
          // Get the layer and create a list from all the values
          var QALayer = MapView.Active.Map.FindLayers((LayerComboBox.SelectedItem).ToString()).FirstOrDefault() as FeatureLayer;
          if (QALayer == null) return;

          string fieldname = LayerFieldComboBox.SelectedItem.ToString();
          var FieldsList = QALayer.GetTable().GetDefinition().GetFields();
          var field = FieldsList.Where((fld) => fld.Name.Equals(fieldname, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();
          if (field == null) return;

          var fieldtype = field.FieldType;

          string clause;
          if (fieldtype.ToString() == "String")
          {
            clause = fieldname + " = '" + selectionvalue + "'";
          }
          else
          {
            clause = fieldname + " = " + selectionvalue;
          }

          var qf = new QueryFilter
          {
            WhereClause = clause
          };

          if (QALayerSelection == null || QALayerSelection.GetCount() == 0) { return; }
          Selection subselection = QALayerSelection.Select(qf, SelectionOption.Normal);
          var subselectionIDs = subselection.GetObjectIDs();
          QALayer.SetSelection(subselection);
          if (subselection.GetCount() > 0)
          {
            MapView.Active.ZoomTo(QALayer, subselectionIDs, TimeSpan.FromSeconds(0));
            MapView.Active.ZoomOutFixed(TimeSpan.FromSeconds(0));
          }

          FrameworkApplication.State.Activate("NavigationQueryDefined");

        });
      }
      catch (Exception ex)
      {
        MessageBox.Show("Error in SelectByValue:  " + ex.ToString(), "Error");
      }
    }

    public void AddQAFieldToLayer()
    {
      try
      {
        QueuedTask.Run(async () =>
        {
          if (string.IsNullOrEmpty (QAFieldComboBox.Text)) return;
          string QAfieldname = QAFieldComboBox.Text.ToString();
          // search the layer for the field, if exists, then confirm use, if not, confirm addition
          var LayerName = LayerComboBox.Text.ToString();
          var QALayer = MapView.Active.Map.FindLayers(LayerName).FirstOrDefault() as FeatureLayer;
          if (QALayer == null) return;

          var FieldsList = QALayer.GetTable().GetDefinition().GetFields();
          var field = FieldsList.Where((fld) => fld.Name.Equals(QAfieldname, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();
          if (field == null) // field not found
          {
            // Prompt for confirmation, and if answer is no, return.
            var result = ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("Add new QA notation field: '" + QAfieldname + "'?", "Create QA Notation Field", System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Question);
            // Return if cancel value is chosen
            if (result == System.Windows.MessageBoxResult.Cancel
              || result == System.Windows.MessageBoxResult.Yes)
            {
              FrameworkApplication.State.Deactivate("QANotationDefined");
              return;
            }
            else
            {
              // Check for edits, and if they exist prompt for saving - essential for attribute creation
              if (Project.Current.HasEdits)
              {
                // Prompt for confirmation, and if answer is no, return.
                result = MessageBox.Show("Edits must be saved before proceeding. Save edits?", "Save All Edits", System.Windows.MessageBoxButton.OKCancel, System.Windows.MessageBoxImage.Asterisk);
                // Return if cancel value is chosen
                if (Convert.ToString(result) == "OK")
                {
                  await Project.Current.SaveEditsAsync();
                }
                else // operation Canceled
                {
                  MessageBox.Show("Add field Canceled.");
                  return;
                }
              }

              // Add the field
              // AddField_management(in_table, field_name, field_type, { field_precision}, { field_scale}, { field_length}, { field_alias}, { field_is_nullable}, { field_is_required}, { field_domain})
              var parameters = Geoprocessing.MakeValueArray(QALayer.GetTable(), QAfieldname, "TEXT");
              var gpResult = await Geoprocessing.ExecuteToolAsync("Management.AddField", parameters);
              //Geoprocessing.ShowMessageBox(gpResult.Messages, "GP Messages",
              //gpResult.IsFailed ? GPMessageBoxStyle.Error : GPMessageBoxStyle.Default);
              if (gpResult.IsFailed == true)
              {
                MessageBox.Show("New attribute operation failed.", "ERROR");
              }

              // Activate controls
              FrameworkApplication.State.Activate("QANotationDefined");
              if (NotationNoteComboBox.ItemCollection.FirstOrDefault() == null || NotationNoteComboBox.ItemCollection.FirstOrDefault().ToString() == "")
              {
                NotationNoteComboBox.Text = "Load or add...";
              }
            }
          }
          else  // field is found
          {
            // Prompt for confirmation, and if answer is no, return.
            var result = MessageBox.Show($@"The QA Field, '{QAfieldname}' exists.  Use it?",
              "Use existing QA Field",
              System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Question);
            // Return if cancel value is chosen
            if (result == System.Windows.MessageBoxResult.Cancel
              || result == System.Windows.MessageBoxResult.No)
            {
              return;
            }
            else
            {
              // Leave the field name in the box, and activate controls
              FrameworkApplication.State.Activate("QANotationDefined");
              NotationNoteComboBox.Text = "Load or add...";
            }
          }
        });
      }
      catch (Exception ex)
      {
        MessageBox.Show("Error in AddQAFieldToLayer:  " + ex.ToString(), "Error");
      }
    }

    private async Task<string> AddFieldToTable (FeatureLayer featureLayer, string newStringFieldName)
    {
      var result = await QueuedTask.Run(() =>
      {
        var selectedLayerTable = featureLayer.GetTable();
        var stringFieldDescription = new ArcGIS.Core.Data.DDL.FieldDescription(newStringFieldName, FieldType.String);
        using var geoDb = selectedLayerTable.GetDatastore() as Geodatabase;
        var fcName = selectedLayerTable.GetName();
        try
        {
          FeatureClassDefinition originalFeatureClassDefinition = geoDb.GetDefinition<FeatureClassDefinition>(fcName);
          FeatureClassDescription originalFeatureClassDescription = new FeatureClassDescription(originalFeatureClassDefinition);

          // Assemble a list of all of new field descriptions
          var fieldDescriptions = new List<ArcGIS.Core.Data.DDL.FieldDescription>() {
                    stringFieldDescription
                };
          // Create a FeatureClassDescription object to describe the feature class to create
          var fcDescription =
            new FeatureClassDescription(fcName, fieldDescriptions, originalFeatureClassDescription.ShapeDescription);

          // Create a SchemaBuilder object
          SchemaBuilder schemaBuilder = new SchemaBuilder(geoDb);

          // Add the modification to the feature class to our list of DDL tasks
          schemaBuilder.Modify(fcDescription);

          // Execute the DDL
          bool success = schemaBuilder.Build();
        }
        catch (Exception ex)
        {
          return $@"Exception: {ex}";
        }
        return string.Empty;
      });
      return result;
    }

    public void EditQANoteFieldValue(string NoteValue)
    {
      try
      {
        // Update value of NotationNoteComboBox
        var qaFieldValue = QAFieldComboBox.Text;
        if (qaFieldValue == null || qaFieldValue == "")
        {
          MessageBox.Show("Choose a note field to store the update.", "Note Field Required");
          return;
        }

        QueuedTask.Run(() =>
        {
          if (NoteValue == "QANoteCombo")
          {
            // Update value of NotationNoteComboBox
            var qaNoteValue = NotationNoteComboBox.Text;
            if (qaNoteValue == "Load or add...") { return; }
            if (qaNoteValue == null) { NoteValue = ""; }
            else { NoteValue = qaNoteValue.ToString(); }
          }

          // Get the chosen QA field and set the selected records to the NoteValue
          // if selection is empty, present error dialog box
          var LayerName = LayerComboBox.Text;
          var QANoteFieldName = QAFieldComboBox.Text;
          // Get all the fields from the selected item in LayerComboBox
          // Get the layer in the ComboBox selection
          var QALayer = MapView.Active.Map.FindLayers(LayerName).FirstOrDefault() as FeatureLayer;
          if (QALayer == null) { return; }

          // check to make sure that the QA note field exists
          string QAfieldname = QAFieldComboBox.Text.ToString();
          if (string.IsNullOrEmpty (QAfieldname))
          {
            MessageBox.Show("Choose a valid note field.", "Choose Note Field");
            return;
          }
          // check if field exists

          var FieldsList = QALayer.GetTable().GetDefinition().GetFields();
          var field = FieldsList.Where((fld) => fld.Name.Equals(QAfieldname, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();
          if (field == null) // field not found
          {
            MessageBox.Show("The field: " + QAfieldname + ", does not exist. Update your note field choice.", "Error");
            return;
          }

          // Get the selection
          Selection QASelection = QALayer.GetSelection();
          var selectionSet = QASelection.GetObjectIDs();
          if (QASelection.GetCount() == 0)
          {
            MessageBox.Show("No selection available for operation.", "Edit Note");
            return;
          }
          // create an instance of the inspector class
          var inspector = new Inspector();
          // load the selected features into the inspector using a list of object IDs
          inspector.Load(QALayer, selectionSet);
          inspector[QANoteFieldName] = NoteValue;
          // apply the changes as an edit operation
          var editOp = new EditOperation();
          editOp.Name = "Edit: " + NoteValue;
          editOp.Modify(inspector);
          bool result = editOp.Execute();
          if (!result)
          {
            MessageBox.Show("Operation was not added to undo stack.");
          }
        });
      }
      catch (Exception ex)
      {
        MessageBox.Show("Error in EditQANoteFieldValue:  " + ex.ToString(), "Error");
      }
    }

    /// <summary>
    /// The 'Note Text' text is added to 
    /// </summary>
    public void AddNewEditNoteValue(string qaNoteValue)
    {
      // Routine to load and maintain QA Note Texts within the combobox
      try
      {
        if (string.IsNullOrEmpty(qaNoteValue)) return;
        // Don't add duplicates to the list
        if (NotationNoteComboBox.ItemCollection.Contains(qaNoteValue)) return;
        var newComboItem = new ComboBoxItem(qaNoteValue);
        NotationNoteComboBox.AddItem(newComboItem);
        NotationNoteComboBox.SelectedItem = newComboItem;
      }
      catch (Exception ex)
      {
        MessageBox.Show($@"Error in AddNewEditNoteValue: {ex.ToString()}", "Error");
      }
    }

    private bool _dialogPath = false;
    public void LoadNoteFile()
    {
      try
      {
        // Load the setting with the default path, if there is a setting.
        // Read a file and load the contents into the Note Texts combobox
        string startLocation;
        if (_dialogPath == false)
        {
          startLocation = System.IO.Path.GetDirectoryName(Project.Current.URI);
        }
        else
        {
          startLocation = AppSettings.Default.NoteFilePath;
        }
        OpenItemDialog openDialog = new OpenItemDialog()
        {
          Title = "Select a Note Texts file to load",
          MultiSelect = false,
          Filter = ItemFilters.TextFiles,
          InitialLocation = startLocation
        };
        string savePath;
        if (openDialog.ShowDialog() == true)
        {
          IEnumerable<Item> selectedItem = openDialog.Items;
          foreach (Item i in selectedItem)
          {
            System.IO.StreamReader file = new System.IO.StreamReader(i.Path);
            string line;
            while ((line = file.ReadLine()) != null)
            {
              NotationNoteComboBox.AddItem(new ComboBoxItem(line));
            }
            savePath = System.IO.Path.GetDirectoryName(i.Path);
            AppSettings.Default.NoteFilePath = savePath;
          }
          NotationNoteComboBox.SelectedItem = NotationNoteComboBox.ItemCollection.FirstOrDefault();

          AppSettings.Default.Save();
          _dialogPath = true; // is now set
        }
      }
      catch (Exception ex)
      {
        MessageBox.Show("Error in LoadNoteFile:  " + ex.ToString(), "Error");
      }
    }

    public void SaveNoteFile()
    {
      try
      {
        string startLocation;
        if (_dialogPath == false)
        {
          startLocation = System.IO.Path.GetDirectoryName(Project.Current.URI);
        }
        else
        {
          startLocation = AppSettings.Default.NoteFilePath;
        }

        string currentNoteValue = NotationNoteComboBox.Text;
        // Save the values found in the Note Texts combobox to a file
        var bpf = new BrowseProjectFilter("esri_browseDialogFilters_textFiles_txt")
        {
          Name = ".txt file to export 'Note Texts' to", 
          InitialPath = startLocation
        };
        var saveItemDialog = new SaveItemDialog { BrowseFilter = bpf };
        var result = saveItemDialog.ShowDialog();
        if (result.Value == false) return;
        var txtFilePath = $@"{saveItemDialog.FilePath}";
        if (!txtFilePath.ToLower().EndsWith(".txt")) txtFilePath += ".txt";
        var folder = System.IO.Path.GetDirectoryName(txtFilePath);
        if (!System.IO.Directory.Exists(folder))
          System.IO.Directory.CreateDirectory(folder);
        var exists = System.IO.File.Exists(txtFilePath);
        if (exists)
        {
          var isYes = MessageBox.Show($@"The export will write over the existing file {txtFilePath}", "Override File", System.Windows.MessageBoxButton.YesNo);
          if (isYes != System.Windows.MessageBoxResult.Yes) return;
          System.IO.File.Delete(txtFilePath);
        }
        // If the file is there, open it, or else it will be created
        System.IO.StreamWriter writer;
        writer = new System.IO.StreamWriter(txtFilePath);
        foreach (var note in NotationNoteComboBox.ItemCollection)
        {
          writer.WriteLine(note);
        }
        writer.Close();
        NotationNoteComboBox.Text = currentNoteValue;

        string savePath = System.IO.Path.GetDirectoryName(txtFilePath);
        AppSettings.Default.NoteFilePath = savePath;

        AppSettings.Default.Save();
        _dialogPath = true; // is now set
      }
      catch (Exception ex)
      {
        MessageBox.Show("Error in SaveNoteFile:  " + ex.ToString(), "Error");
      }

    }

    public void UndoOperation()
    {
      try
      {
        // Undo the last operation
        var operationMgr = MapView.Active.Map.OperationManager;
        if (operationMgr.CanUndo) { operationMgr.UndoAsync(); }

      }
      catch (Exception ex)
      {
        MessageBox.Show("Error in UndoOperation:  " + ex.ToString(), "Error");
      }
    }

    public void SaveEdits()
    {
      try
      {
        var cmdSaveEdits = FrameworkApplication.GetPlugInWrapper("esri_editing_SaveEditsBtn") as ICommand;
        if (cmdSaveEdits != null)
        {
          if (cmdSaveEdits.CanExecute(null))
          {
            cmdSaveEdits.Execute(null);
          }

        }
      }
      catch (Exception ex)
      {
        MessageBox.Show("Error in SaveEdits:  " + ex.ToString(), "Error");
      }

    }

    public void DiscardEdits()
    {
      {
        try
        {
          var cmdSaveEdits = FrameworkApplication.GetPlugInWrapper("esri_editing_DiscardEditsBtn") as ICommand;
          if (cmdSaveEdits != null)
          {
            if (cmdSaveEdits.CanExecute(null))
            {
              cmdSaveEdits.Execute(null);
            }

          }
        }
        catch (Exception ex)
        {
          MessageBox.Show("Error in SaveEdits:  " + ex.ToString(), "Error");
        }

      }

    }


    #endregion

    #region Overrides
    /// <summary>
    /// Called by Framework when ArcGIS Pro is closing
    /// </summary>
    /// <returns>False to prevent Pro from closing, otherwise True</returns>
    protected override bool CanUnload()
    {

      return true;
    }



    protected override Func<Task> ExecuteCommand(string id)
    {

      var command = FrameworkApplication.GetPlugInWrapper(id) as ICommand;
      if (command == null)
        return () => Task.FromResult(0);
      if (!command.CanExecute(null))
        return () => Task.FromResult(0);

      return () =>
      {
        command.Execute(null); // if it is a tool, execute will set current tool
        return Task.FromResult(0);
      };
    }

    #endregion Overrides

  }
}

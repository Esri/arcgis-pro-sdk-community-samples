/*

   Copyright 2018 Esri

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



namespace QAReviewTool
{
	/// <summary>
	/// This sample provides a set of controls which guide the user through a data quality assurance(QA) review workflow, with tools for visually reviewing and notating datasets based on their accuracy.
	/// </summary>
	/// <remarks>
	/// 1. Download the Community Sample data(see under the 'Resources' section for downloading sample data). Make sure that the Sample dataset is unzipped under c:\data and that the sample data contains the folder C:\Data\QAReviewTool containing a map package named “QA_Review_Tool.ppkx” which is required for this sample.
	/// 1. Open this solution in Visual Studio 2017.
	/// 1. Click the build menu and select Build Solution.
	/// 1. Click the Start button to run the solution.  ArcGIS Pro will open.
	/// 1. Open the map package "QA_Review_Tool.ppkx" located in the "C:\Data\QAReviewTool" folder.This project contains all required data.  Be sure the Topographic basemap layer is displayed.
	/// 1. Click on the new “Review” tab provided by the add-in.  In the “Layer and Selection” group, click the dropdown button for the “Layer” combobox and select “La_Jolla_Roads”.  Press the “Refresh Selection” button, which will enable the comboboxes in the “Field Setting” group.Then press the “Open Attribute Table” button and adjust the size of the table to where you can see around 3 or 4 records.
	/// ![UI](Screenshot/Screenshot1.png)
	/// 1. In the Field Setting group, click on the dropdown button for the “Value Field” and scroll to the bottom of the list and choose the field “review_code”.  This will populate the “Value” field combobox with the unique data values from the review_code field.It will then zoom to the feature containing the value “1”.  The forward and backward navigation buttons are now enabled, and you can click on these to zoom to the different records containing other review_code values.Click on “Show Selected Records” in the attribute table to just show the selected record(s).
	/// ![UI](Screenshot/Screenshot2.png)
	/// 1. In the “Note Field” combobox, choose the “QA_NOTE” from the list of available field names to store your QA notes.  It will ask if you would like to add the field to your data, and press “OK” to add it.Check to see that QA_NOTE is added at the end of your attribute table, and is also listed in the Note Field combobox.  This step will enable the rest of the controls on the Review tab.
	/// ![UI](Screenshot/Screenshot3.png)
	/// 1. You are now able to use the enabled Note Shortcuts to apply a brief description to any selected records.Use the “Forward” button to navigate to review_code value 3 and you will see three road features selected.  As the Topographic basemap layer does not show a street for these features, change the basemap to Imagery to see verify the data.  You will see the features represent the entrance driveway to a building and its parking lot.  Press the “Correct” note shortcut button to set the QA review value.
	/// ![UI](Screenshot/Screenshot4.png)
	/// 1. Next, you can try applying custom notes and managing your edits using the controls in the “Custom Notes” group.You can load a set of custom notes from a textfile and use these in place of the note shortcuts.Press the “Load Note File” button and navigate to the C:\Data\QAReviewTool folder.Click on the file “Custom Notes.txt” and press OK.
	/// ![UI](Screenshot/Screenshot5.png)
	/// 1. The values will be loaded into the “Note Value” combobox.You can then select from these values and apply them to your current selected records using the “Commit” button.You can also add new values to the note value combobox and save the values to a new note file.Finally, you can use the Undo, Save and Discard buttons to manage your edits.
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
			ActiveMapViewChangedEvent.Subscribe(OnActiveMapViewChanged);

		}

		#region Event Listening

		private MapView _currentActiveMapView = null;

		private void OnActiveMapViewChanged(ActiveMapViewChangedEventArgs activeMapViewChangedEventArgs)
		{
			try
			{
				// We are only interested in the current mapview, when focus changes, we do not refresh if we are still on the current active mapview
				if (activeMapViewChangedEventArgs.IncomingView == null) return;
				if (activeMapViewChangedEventArgs.IncomingView == _currentActiveMapView) return;
				_currentActiveMapView = activeMapViewChangedEventArgs.IncomingView;

				RefreshLayerComboBox();
				ResetTab();
			}
			catch (Exception ex)
			{
				MessageBox.Show("Error in OnActiveMapViewChanged:  " + ex.ToString(), "Error");
			}


		}

		#endregion Event Listening


		#region Business Logic

		// if true we need to refresh the layer list first
		private bool _deferredRefreshOfLayerList = false;

		// These are the participating UI classes 
		private LayerListComboBox _layerListComboBox;
		public LayerListComboBox LayerComboBox
		{
			get { return _layerListComboBox; }
			set
			{
				_layerListComboBox = value;
				if (_deferredRefreshOfLayerList)
				{
					_deferredRefreshOfLayerList = false;
					RefreshLayerComboBox();
				}
			}
		}
		public QAFieldListComboBox QAFieldComboBox { get; set; }
		public LayerFieldListComboBox LayerFieldComboBox { get; set; }
		public ValueListComboBox FieldValueComboBox { get; set; }
		public EditNoteComboBox EditNoteComboBox { get; set; }

		public bool DeferredRefreshOfLayerList { get => _deferredRefreshOfLayerList; set => _deferredRefreshOfLayerList = value; }

		public void ResetTab()
		{

			if (!_deferredRefreshOfLayerList)
			{

				if (Project.Current.HasEdits)
				{
					MessageBox.Show("You have ended your current review session. You have unsaved edits.");
				}

				// clear the comboboxes
				EditNoteComboBox.ClearItems();
				LayerFieldComboBox.ClearItems();
				QAFieldComboBox.ClearItems();
				FieldValueComboBox.ClearItems();

				// reset states to deactivated
				FrameworkApplication.State.Deactivate("active_state_1");
				FrameworkApplication.State.Deactivate("active_state_2");
				FrameworkApplication.State.Deactivate("active_state_3");


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
					_deferredRefreshOfLayerList = true;
					return;
				}
				LayerComboBox.ClearItems();
				QueuedTask.Run(() =>
				{
					Map map = MapView.Active.Map;
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
										// LayerComboBox.Text = LayerComboBox.ItemCollection.FirstOrDefault().ToString();
									}
				});

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
				if (LayerComboBox.Text == null || LayerComboBox.Text == "" || LayerComboBox.Text == "Choose...")
				{
					MessageBox.Show("First select a layer from the list.", "Select a layer");
					return;
				}
				// Populate the QA Field combobox with 2 default values
				// Either call 
				RefreshQAFieldComboBox();
				// Populate the Field combobox with the list of fields from the selected layer
				RefreshLayerFieldComboBox();

				ClearValueComboBox();
			}
			catch (Exception ex)
			{
				MessageBox.Show("Error in LayerRefreshClicked:  " + ex.ToString(), "Error");
			}
		}


		public void RefreshQAFieldComboBox()
		{
			try
			{
				QAFieldComboBox.ClearItems();
				QAFieldComboBox.AddItem(new ComboBoxItem("QA_NOTE"));
				QAFieldComboBox.AddItem(new ComboBoxItem("REV_NOTE"));
				QAFieldComboBox.Text = "Choose...";
			}
			catch (Exception ex)
			{
				MessageBox.Show("Error in RefreshQAFieldComboBox:  " + ex.ToString(), "Error");
			}
		}

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

									// bool hasItems = false;  // Keep empty, as selection of an item populates the values combobox
									foreach (var fld in FieldsList)
					{
						var fldNameString = fld.Name;
						if (fldNameString != "Shape")
						{
							LayerFieldComboBox.AddItem(new ComboBoxItem(fldNameString));
						}
					}

					LayerFieldComboBox.Text = "Choose...";

									// Zoom to either the selected features, or the extent of all layers in the map
									var featureLayers = MapView.Active.Map.Layers.OfType<FeatureLayer>().Where((featurelayer) => featurelayer.Name == LayerComboBox.SelectedItem.ToString());
									//MapView.Active.SelectLayers(featureLayers.ToList());

									Selection QASelection = QALayer.GetSelection();
					var selectionSet = QASelection.GetObjectIDs();
					if (QASelection.GetCount() > 0)
					{
						MapView.Active.ZoomTo(QALayer, selectionSet, TimeSpan.FromSeconds(0));
						MapView.Active.ZoomOutFixed(TimeSpan.FromSeconds(0));
					}
					else
					{
						MapView.Active.ZoomTo(QALayer);
					}
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


		public void RefreshFieldValueComboBox(string fieldName)
		{
			try
			{
				FieldValueComboBox.ClearItems();
				QueuedTask.Run(() =>
				{
									// Get the layer and create a list from all the values
									var QALayer = MapView.Active.Map.FindLayers((LayerComboBox.SelectedItem).ToString()).FirstOrDefault() as FeatureLayer;

					if (QALayer == null) return;

					QALayer.ClearSelection();
					Selection QALayerSelection = QALayer.Select();

									// Determine field type for adding values and sorting

									List<object> QALayerCodeList = new List<object> { };

					using (RowCursor QARowCursor = QALayerSelection.Search(null, false))
					{
						while (QARowCursor.MoveNext())
						{
							using (Row currentRow = QARowCursor.Current)
							{
												// QALayerCodeList.Add(Convert.ToString(currentRow[fieldName]));
												QALayerCodeList.Add(currentRow[fieldName]);
							}
						}
					}
					QALayerCodeList.Sort();
									// Get unique values and counts in the list
									foreach (object item in QALayerCodeList.Distinct())
					{
						if ((item != null) && (item.ToString() != ""))
						{
											// add to combobox
											FieldValueComboBox.AddItem(new ComboBoxItem((item.ToString())));
						}
					}

									// Dispose of data classes
									QALayer.ClearSelection();
					QALayerSelection.Dispose();

									// FieldValueComboBox.ItemCollection.FirstOrDefault();
									FieldValueComboBox.Enabled = true;
					FieldValueComboBox.SelectedItem = FieldValueComboBox.ItemCollection.FirstOrDefault();

									// Select the recordset by this first value in the list 
									object selectionvalue = FieldValueComboBox.SelectedItem;
					SelectByValue(selectionvalue);
				});
			}
			catch (Exception ex)
			{
				MessageBox.Show("Error in RefreshFieldValueComboBox:  " + ex.ToString(), "Error");
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
				//var QALayer = MapView.Active.Map.FindLayers((LayerComboBox.SelectedItem).ToString()).FirstOrDefault() as FeatureLayer;
				//if (QALayer == null) return;

				// MapView.Active.SelectLayers();
				//Zoom to the selected layers in the TOC
				var featureLayers = MapView.Active.Map.Layers.OfType<FeatureLayer>().Where((featurelayer) => featurelayer.Name == LayerComboBox.SelectedItem.ToString());
				MapView.Active.SelectLayers(featureLayers.ToList());

				var cmdOpenAttributeTable = FrameworkApplication.GetPlugInWrapper("esri_editing_table_openTablePaneButton") as ICommand;
				if (cmdOpenAttributeTable != null)
				{
					if (cmdOpenAttributeTable.CanExecute(null))
					{
						cmdOpenAttributeTable.Execute(null);
					}
				}

			}
			catch (Exception ex)
			{
				MessageBox.Show("Error in OpenAttributeTable:  " + ex.ToString(), "Error");
			}
		}

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

					QALayer.ClearSelection();
									// Select by the selectionvalue
									string fieldname = LayerFieldComboBox.SelectedItem.ToString();
					string clause = fieldname + " = " + selectionvalue;
					var qf = new QueryFilter
					{
						WhereClause = clause
					};
					QALayer.Select(qf);

					Selection QASelection = QALayer.GetSelection();
					var selectionSet = QASelection.GetObjectIDs();
					if (QASelection.GetCount() > 0)
					{
						MapView.Active.ZoomTo(QALayer, selectionSet, TimeSpan.FromSeconds(0));
						MapView.Active.ZoomOutFixed(TimeSpan.FromSeconds(0));
					}

					FrameworkApplication.State.Activate("active_state_2");

				});
			}
			catch (Exception ex)
			{
				MessageBox.Show("Error in SelectByValue:  " + ex.ToString(), "Error");
			}
		}

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

		public void AddQAFieldToLayer()
		{
			try
			{
				QueuedTask.Run(async () =>
				{

					if (QAFieldComboBox.Text == null) { return; }
					string QAfieldname = QAFieldComboBox.Text.ToString();
									// search the layer for the field, if exists, then confirm use, if not, confirm addition
									var LayerName = LayerComboBox.Text.ToString();
					var QALayer = MapView.Active.Map.FindLayers(LayerName).FirstOrDefault() as FeatureLayer;
					if (QALayer == null) return;
					int qaFieldIndex = QALayer.GetTable().GetDefinition().FindField(QAfieldname);

					if (qaFieldIndex == -1) // field not found
									{

										// Prompt for confirmation, and if answer is no, return.
										var result = ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("Create new field: " + QAfieldname + "?", "CREATE QA FIELD", System.Windows.MessageBoxButton.OKCancel, System.Windows.MessageBoxImage.Asterisk);
										// Return if cancel value is chosen
										if (Convert.ToString(result) == "Cancel")
						{
							QAFieldComboBox.Text = "Choose...";
							FrameworkApplication.State.Deactivate("active_state_3");
							return;
						}
						else
						{

											// Check for edits, and if they exist prompt for saving - essential for attribute creation
											if (Project.Current.HasEdits)
							{
												// Prompt for confirmation, and if answer is no, return.
												result = ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("Edits must be saved before proceeding. Save edits?", "Save All Edits", System.Windows.MessageBoxButton.OKCancel, System.Windows.MessageBoxImage.Asterisk);
												// Return if cancel value is chosen
												if (Convert.ToString(result) == "OK")
								{
									await Project.Current.SaveEditsAsync();
								}
								else // operation cancelled
												{
									MessageBox.Show("Add field cancelled.");
									QAFieldComboBox.Text = "Choose...";
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
											FrameworkApplication.State.Activate("active_state_3");
							if (EditNoteComboBox.ItemCollection.FirstOrDefault() == null || EditNoteComboBox.ItemCollection.FirstOrDefault().ToString() == "")
							{
								EditNoteComboBox.Text = "Load or add...";
							}


						}
					}
					else  // field is found
									{
										// Prompt for confirmation, and if answer is no, return.
										var result = ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("The QA Field, " + QAfieldname + " exists.  Use it?", "USE QA FIELD", System.Windows.MessageBoxButton.OKCancel, System.Windows.MessageBoxImage.Asterisk);
										// Return if cancel value is chosen
										if (Convert.ToString(result) == "Cancel")
						{
							QAFieldComboBox.Text = "Choose...";
							return;
						}
						else
						{
											// Leave the field name in the box, and activate controls
											FrameworkApplication.State.Activate("active_state_3");
							EditNoteComboBox.Text = "Load or add...";
						}
					}

				});
			}

			catch (Exception ex)
			{
				ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("Error in AddQAFieldToLayer:  " + ex.ToString(), "Error");
			}

		}


		public void EditQANoteFieldValue(string NoteValue)
		{
			try
			{
				// Update value of EditNoteComboBox
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
										// Update value of EditNoteComboBox
										var qaNoteValue = EditNoteComboBox.Text;
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
					if (QAfieldname == "Choose...")
					{
						MessageBox.Show("Choose a valid note field.", "Choose Note Field");
						return;
					}
									// check if field exists
									int qaFieldIndex = QALayer.GetTable().GetDefinition().FindField(QAfieldname);
					if (qaFieldIndex == -1) // field not found
									{
						MessageBox.Show("The field: " + QAfieldname + ", does not exist. Update your note field choice.", "Error");
						QAFieldComboBox.Text = "Choose...";
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

		public void SaveCustomNoteValues()
		{
			// Routine to load and maintain QA Note values within the combobox
			try
			{
				QueuedTask.Run(() =>
				{
									// Get current custom Note value and save to new settings file
									// Update value of EditNoteComboBox
									var qaNoteValue = EditNoteComboBox.Text;
					if (qaNoteValue == null) { return; }

									//// Build the path to the settings file under My Documents
									//string MyDocs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
									//string PathCombination = System.IO.Path.Combine(MyDocs, "ArcGIS", "QAToolSettings"); // "QANoteValues.txt");
									//if (!System.IO.Directory.Exists(PathCombination))
									//{
									//    System.IO.Directory.CreateDirectory(PathCombination);
									//}

									// Don't add duplicates to the list
									string selectedNotevalue;
					bool duplicateFound = false;

					for (int i = 0; i < EditNoteComboBox.ItemCollection.Count; i++)
					{
						EditNoteComboBox.SelectedIndex = i;
						selectedNotevalue = EditNoteComboBox.SelectedItem.ToString();
						if (selectedNotevalue == qaNoteValue) { duplicateFound = true; }
					}

					if (duplicateFound == false)
					{
						EditNoteComboBox.AddItem(new ComboBoxItem(qaNoteValue));
					}


					EditNoteComboBox.Text = qaNoteValue;
					EditNoteComboBox.SelectedItem = qaNoteValue;

				});

			}

			catch (Exception ex)
			{
				MessageBox.Show("Error in SaveCustomNoteValues:  " + ex.ToString(), "Error");
			}

		}

		private bool _dialogPath = false;
		public void LoadNoteFile()
		{
			try
			{
				// Load the setting with the default path, if there is a setting.


				// Read a file and load the contents into the Note values combobox
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
					Title = "Select a Note values file to load",
					MultiSelect = false,
					Filter = ItemFilters.textFiles,
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
							EditNoteComboBox.AddItem(new ComboBoxItem(line));
						}
						savePath = System.IO.Path.GetDirectoryName(i.Path);
						AppSettings.Default.NoteFilePath = savePath;
					}
					EditNoteComboBox.SelectedItem = EditNoteComboBox.ItemCollection.FirstOrDefault();

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

				string currentNoteValue = EditNoteComboBox.Text;
				// Save the values found in the Note values combobox to a file
				SaveItemDialog saveDialog = new SaveItemDialog()
				{
					Title = "Save the Note values to a file",
					Filter = ItemFilters.textFiles,
					DefaultExt = "txt",
					InitialLocation = startLocation
				};

				if (saveDialog.ShowDialog() == true)
				{
					// IEnumerable<Item> selectedItem = openDialog.Items;

					// If the file is there, open it, or else it will be created
					System.IO.StreamWriter writer;
					writer = new System.IO.StreamWriter(saveDialog.FilePath, true);
					string selectedNotevalue;
					for (int i = 0; i < EditNoteComboBox.ItemCollection.Count; i++)
					{
						EditNoteComboBox.SelectedIndex = i;
						selectedNotevalue = EditNoteComboBox.SelectedItem.ToString();
						writer.WriteLine(selectedNotevalue);
					}
					writer.Close();
					EditNoteComboBox.Text = currentNoteValue;

					string savePath = System.IO.Path.GetDirectoryName(saveDialog.FilePath);
					AppSettings.Default.NoteFilePath = savePath;

					AppSettings.Default.Save();
					_dialogPath = true; // is now set

				}
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


		#endregion Overrides

	}
}

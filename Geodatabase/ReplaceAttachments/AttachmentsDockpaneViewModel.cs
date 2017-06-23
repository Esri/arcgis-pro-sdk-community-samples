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
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using ArcGIS.Core.Data;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using System.Windows.Input;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using System.Windows.Data;
using ReplaceAttachments.Util;
using System.Threading.Tasks;

namespace ReplaceAttachments
{
    internal class AttachmentsDockpaneViewModel : DockPane
    {
        private const string _dockPaneID = "ReplaceAttachments_AttachmentsDockpane";

        private ObservableCollection<string> _layers = new ObservableCollection<string>();
        private ObservableCollection<string> _relationshipClasses = new ObservableCollection<string>();
        private ObservableCollection<string> _attachmentNames = new ObservableCollection<string>();
        private object _lockCollections = new object();
        private bool _isGoEnabled = false;

        protected AttachmentsDockpaneViewModel() {
            // when a new mapview is coming in we need to update the list of layers
            ArcGIS.Desktop.Mapping.Events.ActiveMapViewChangedEvent.Subscribe(OnMapViewChanged);
            
            BindingOperations.EnableCollectionSynchronization(_layers, _lockCollections);
            BindingOperations.EnableCollectionSynchronization(_relationshipClasses, _lockCollections);
            BindingOperations.EnableCollectionSynchronization(_attachmentNames, _lockCollections);
        }

        protected override Task InitializeAsync()
        {
            if (MapView.Active != null) UpdateLayers(MapView.Active.Map);
            return Task.FromResult(0);
        }


        private void OnMapViewChanged (ArcGIS.Desktop.Mapping.Events.ActiveMapViewChangedEventArgs args)
        {
            if (args.IncomingView == null) return;
            UpdateLayers(args.IncomingView.Map);
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

        /// <summary>
        /// Text shown near the top of the DockPane.
        /// </summary>
        private string _heading = "Replace Attachments for Related Feature Classes";
        public string Heading
        {
            get { return _heading; }
            set
            {
                SetProperty(ref _heading, value, () => Heading);
            }
        }

        private string _selectedLayer;
        private string _selectedRelationship;
        private string _relatedFeatureClass;
        private string oldAttachmentName;
        private string newAttachment;
        private Definition relatedFeatureClassDefinition;

        public ObservableCollection<string> Layers
        {
            get { return _layers; }
        }

        public ObservableCollection<string> RelationshipClasses
        {
            get { return _relationshipClasses; }
        }

        public ObservableCollection<string> AttachmentNames
        {
            get { return _attachmentNames; }
        }

        public string SelectedRelationship
        {
            get { return _selectedRelationship; }
            set
            {
                SetProperty(ref _selectedRelationship, value, () => SelectedRelationship);
                UpdateRelatedFeatureClass();
                UpdateIsGoEnabled();
            }
        }

        public string SelectedLayer
        {
            get { return _selectedLayer; }
            set
            {
                SetProperty(ref _selectedLayer, value, () => SelectedLayer);
                UpdateRelationshipClasses();
                UpdateIsGoEnabled();
            }
        }

        public bool IsGoEnabled
        {
            get { return _isGoEnabled; }
            set
            {
                SetProperty(ref _isGoEnabled, value, () => IsGoEnabled);
            }
        }
        public string RelatedFeatureClass
        {
            get { return _relatedFeatureClass; }
            set
            {
                SetProperty(ref _relatedFeatureClass, value, () => RelatedFeatureClass);
                QueuedTask.Run(() =>
                {
                    lock (_lockCollections) _attachmentNames.Clear();
                    using (Table table = (MapView.Active.Map.Layers.First(layer => layer.Name.Equals(SelectedLayer)) as FeatureLayer).GetTable())
                    {
                        if (!table.IsAttachmentEnabled()) return;
                        Geodatabase geodatabase = null;
                        if (table != null && table.GetDatastore() is Geodatabase)
                            geodatabase = table.GetDatastore() as Geodatabase;
                        if (geodatabase == null) return;

                        if (_relatedFeatureClass == null) return;
                        Table relatedTable = geodatabase.OpenDataset<Table>(_relatedFeatureClass);
                        if (relatedTable == null) return;

                        using (RowCursor rowCursor = table.Search(null, false))
                        {
                            while (rowCursor.MoveNext())
                            {
                                using (Row row = rowCursor.Current)
                                {
                                    IEnumerable<Attachment> attachments = row.GetAttachments(null, true);
                                    foreach (Attachment attachment in attachments)
                                    {
                                        lock (_lockCollections)
                                        {
                                            _attachmentNames.Add(attachment.GetName());
                                        }
                                    }
                                }
                            }
                        }
                    }
                });
            }
        }

        public string OldAttachmentName
        {
            get { return oldAttachmentName; }
            set
            {
                SetProperty(ref oldAttachmentName, value, () => OldAttachmentName);
                UpdateIsGoEnabled();
            }
        }
        public string NewAttachment
        {
            get { return newAttachment; }
            set
            {
                SetProperty(ref newAttachment, value, () => NewAttachment);
                UpdateIsGoEnabled();
            }
        }

        private ICommand _goReplaceAttachment = null;
        public ICommand GoReplaceAttachment
        {
            get
            {
                if (_goReplaceAttachment == null) _goReplaceAttachment = new RelayCommand(Work, () => true);
                return _goReplaceAttachment;
            }
        }

        private ICommand _selectPath = null;
        public ICommand SelectPath
        {
            get
            {
                if (_selectPath == null) _selectPath = new RelayCommand(GetPath, () => true);
                return _selectPath;
            }
        }

        private void GetPath(object param)
        {
            OpenItemDialog pathDialog = new OpenItemDialog();
            pathDialog.Title = "Select Path to attachment";
            pathDialog.InitialLocation = @"C:\Data\";
            pathDialog.MultiSelect = false;
            pathDialog.Filter = ItemFilters.rasters;

            bool? ok = pathDialog.ShowDialog();

            if (ok == true)
            {
                IEnumerable<Item> selectedItems = pathDialog.Items;
                foreach (Item selectedItem in selectedItems)
                    NewAttachment = selectedItem.Path;
            }
        }

        /// <summary>
        /// Given that 
        /// 1. A layer has been selected
        /// 2. A RelationshipClass has been selected
        /// 3. The Table/FeatureClass related to the Table/FeatureClass corresponding to the layer selected has Attachments enabled
        /// 
        /// Then, this method will 
        /// 1. Get all the rows of the Related Table/FeatureClass
        /// 2. Find if any row has Attachments which have the same name as the value of "OldAttachmentName"
        /// 3. Replace the Attachment Data for the matching Attachments with the Data corresponding to "NewAttachment"
        /// </summary>
        private void Work(object param)
        {
             QueuedTask.Run(() =>
             {
                 using (Table table = (MapView.Active.Map.Layers.First(layer => layer.Name.Equals(SelectedLayer)) as FeatureLayer).GetTable())
                 {
                     if (!table.IsAttachmentEnabled()) return;

                     Geodatabase geodatabase = null;
                     if (table != null && table.GetDatastore() is Geodatabase)
                         geodatabase = table.GetDatastore() as Geodatabase;
                     if (geodatabase == null) return;

                     if (_relatedFeatureClass == null) return;
                     Table relatedTable = geodatabase.OpenDataset<Table>(_relatedFeatureClass);
                     if (relatedTable == null) return;

                     if (String.IsNullOrEmpty(OldAttachmentName)) return;
                     if (String.IsNullOrEmpty(NewAttachment) || !File.Exists(NewAttachment)) return;

                     using (MemoryStream newAttachmentMemoryStream = CreateMemoryStreamFromContentsOf(NewAttachment))
                     {
                         using (RowCursor rowCursor = table.Search(null, false))
                         {
                             while (rowCursor.MoveNext())
                             {
                                 using (Row row = rowCursor.Current)
                                 {   
                                     IEnumerable<Attachment> attachments = row.GetAttachments(null, true).Where(attachment => attachment.GetName().Equals(oldAttachmentName));
                                     foreach (Attachment attachment in attachments)
                                     {
                                         attachment.SetData(newAttachmentMemoryStream);
                                         row.UpdateAttachment(attachment);
                                     }
                                 }
                             }
                         }
                     }
                 }
             });
        }

        private async void UpdateIsGoEnabled()
        {
            IsGoEnabled = await QueuedTask.Run(() =>
            {
                bool isGoEnabled = false;
                using (Table table = (MapView.Active.Map.Layers.First(layer => layer.Name.Equals(SelectedLayer)) as FeatureLayer).GetTable())
                {
                    if (!table.IsAttachmentEnabled()) return isGoEnabled;

                    Geodatabase geodatabase = null;
                    if (table != null && table.GetDatastore() is Geodatabase)
                        geodatabase = table.GetDatastore() as Geodatabase;
                    if (geodatabase == null) return isGoEnabled;

                    if (_relatedFeatureClass == null) return isGoEnabled;
                    Table relatedTable = geodatabase.OpenDataset<Table>(_relatedFeatureClass);
                    if (relatedTable == null) return isGoEnabled;

                    if (String.IsNullOrEmpty(OldAttachmentName)) return isGoEnabled;
                    if (String.IsNullOrEmpty(NewAttachment) || !File.Exists(NewAttachment)) return isGoEnabled;
                    isGoEnabled = true;
                }
                return isGoEnabled;
            });
        }

        /// <summary>
        /// This method will populate the Layers (bound to the LayersComboBox) with all the Feature Layers present in the Active Map View
        /// </summary>
        public void UpdateLayers(Map map)
        {
            lock (_lockCollections)
            {
                _layers.Clear();
                _layers.AddRange(new ObservableCollection<string>(map.Layers.Where(layer => layer is FeatureLayer).Select(layer => layer.Name)));
            }
        }

        /// <summary>
        /// Given that a Layer has been selected, this method will populate the RelationshipClasses (bound to RelationshipClasses Combobox) with the names of the RelationshipClasses 
        /// in which the FeatureClass/Table corresponding to the selected layer participates (as Origin or Destination)
        /// </summary>
        public void UpdateRelationshipClasses()
        {
            QueuedTask.Run(() =>
            {
                lock (_lockCollections) _relationshipClasses.Clear();
                using (Table table = (MapView.Active.Map.Layers.First(layer => layer.Name.Equals(SelectedLayer)) as FeatureLayer).GetTable())
                {
                    Geodatabase geodatabase = null;
                    if (table != null && table.GetDatastore() is Geodatabase)
                        geodatabase = table.GetDatastore() as Geodatabase;
                    if (geodatabase == null) return;
                    IReadOnlyList<RelationshipClassDefinition> relationshipClassDefinitions = geodatabase.GetDefinitions<RelationshipClassDefinition>();
                    IEnumerable<RelationshipClassDefinition> relavantRelationshipClassDefns = relationshipClassDefinitions.Where(definition =>
                                                                                                        definition.GetOriginClass().Contains(table.GetName()) ||
                                                                                                        definition.GetDestinationClass().Contains(table.GetName()));
                    IEnumerable<string> relationshipClassNames = relavantRelationshipClassDefns.Select(definition => definition.GetName());
                    lock (_lockCollections) _relationshipClasses.AddRange(new ObservableCollection<string>(relationshipClassNames));
                }
            });
        }

        /// <summary>
        /// Given that the RelationshipClass is selected, this method will
        /// 1. Get the Definitions of Datasets related by the selected relationship class
        /// 2. Set the RelatedFeatureClass property to the Table/FeatureClass which is related to the FeatureClass corresponding to the selected layer
        /// </summary>
        public void UpdateRelatedFeatureClass()
        {
            QueuedTask.Run(() =>
            {
                using (Table table = (MapView.Active.Map.Layers.First(layer => layer.Name.Equals(SelectedLayer)) as FeatureLayer).GetTable())
                {
                    Geodatabase geodatabase = null;
                    if (table != null && table.GetDatastore() is Geodatabase)
                        geodatabase = table.GetDatastore() as Geodatabase;
                    if (geodatabase == null) return;
                    if(SelectedRelationship == null) return;
                    IReadOnlyList<Definition> relatedDefinitions = geodatabase.GetRelatedDefinitions(geodatabase.GetDefinition<RelationshipClassDefinition>(SelectedRelationship), 
                                                                                                     DefinitionRelationshipType.DatasetsRelatedThrough);
                    relatedFeatureClassDefinition = relatedDefinitions.First(definition => definition.GetName() != table.GetDefinition().GetName());
                    RelatedFeatureClass = relatedFeatureClassDefinition.GetName();
                }
            });
        }

        /// <summary>
        /// This is a helper method to generate a memory stream from a a file
        /// </summary>
        /// <param name="fileNameWithPath"></param>
        /// <returns>Memory Stream representing the File</returns>
        private static MemoryStream CreateMemoryStreamFromContentsOf(String fileNameWithPath)
        {
            MemoryStream memoryStream;
            using (memoryStream = new MemoryStream())
            using (FileStream file = new FileStream(fileNameWithPath, FileMode.Open, FileAccess.Read))
            {
                byte[] bytes = new byte[file.Length];
                file.Read(bytes, 0, (int)file.Length);
                memoryStream.Write(bytes, 0, (int)file.Length);
            }
            return memoryStream;
        } 
    }

    /// <summary>
    /// Button implementation to show the DockPane.
    /// </summary>
    internal class AttachmentsDockpane_ShowButton : Button
    {
        protected override void OnClick()
        {
            AttachmentsDockpaneViewModel.Show();
        }
    }
}

//   Copyright 2015 Esri
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
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArcGIS.Core.Data;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;

namespace ReplaceAttachments
{
    internal class AttachmentsDockpaneViewModel : DockPane
    {
        private const string _dockPaneID = "ReplaceAttachments_AttachmentsDockpane";

        protected AttachmentsDockpaneViewModel() { }

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

        private ObservableCollection<String> layers;
        private ObservableCollection<String> relationshipClasses;
        private string selectedLayer;
        private string selectedRelationship;
        private string relatedFeatureClass;
        private string oldAttachmentName;
        private string newAttachment;
        private Definition relatedFeatureClassDefinition;

        public ObservableCollection<string> Layers
        {
            get { return layers; }
            set
            {
                layers = value;
                NotifyPropertyChanged(new PropertyChangedEventArgs("Layers"));
            }
        }

        public ObservableCollection<string> RelationshipClasses
        {
            get { return relationshipClasses; }
            set
            {
                relationshipClasses = value;
                NotifyPropertyChanged(new PropertyChangedEventArgs("RelationshipClasses"));
            }
        }

        public string SelectedRelationship
        {
            get { return selectedRelationship; }
            set
            {
                SetProperty(ref selectedRelationship, value, () => SelectedRelationship);
            }
        }

        public string SelectedLayer
        {
            get { return selectedLayer; }
            set
            {
                SetProperty(ref selectedLayer, value, () => SelectedLayer);
            }
        }

        public string RelatedFeatureClass
        {
            get { return relatedFeatureClass; }
            set
            {
                SetProperty(ref relatedFeatureClass, value, () => RelatedFeatureClass);
            }
        }

        public string OldAttachmentName
        {
            get { return oldAttachmentName; }
            set
            {
                SetProperty(ref oldAttachmentName, value, () => OldAttachmentName);
            }
        }
        public string NewAttachment
        {
            get { return newAttachment; }
            set
            {
                SetProperty(ref newAttachment, value, () => NewAttachment);
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
        public void Work()
        {
            QueuedTask.Run(async () =>
            {
                using (Table table = (MapView.Active.Map.Layers.First(layer => layer.Name.Equals(SelectedLayer)) as FeatureLayer).GetTable())
                {
                    Geodatabase geodatabase = null;
                    if (table != null && table.GetDatastore() is Geodatabase)
                        geodatabase = table.GetDatastore() as Geodatabase;
                    if (geodatabase == null) return;
                    
                    Table relatedTable = geodatabase.OpenDataset<Table>(relatedFeatureClass);
                    if (!relatedTable.IsAttachmentEnabled()) return;

                    if (String.IsNullOrEmpty(OldAttachmentName)) return;
                    if (String.IsNullOrEmpty(NewAttachment) || !File.Exists(NewAttachment)) return;

                    using (MemoryStream newAttachmentMemoryStream = CreateMemoryStreamFromContentsOf(NewAttachment))
                    {
                        using (RowCursor rowCursor = relatedTable.Search(null, false))
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

        /// <summary>
        /// This method will populate the Layers (bound to the LayersComboBox) with all the Feature Layers present in the Active Map View
        /// </summary>
        public void UpdateLayers()
        {
            Layers = new ObservableCollection<string>(MapView.Active.Map.Layers.Where(layer => layer is FeatureLayer).Select(layer => layer.Name));
        }

        /// <summary>
        /// Given that a Layer has been selected, this method will populate the RelationshipClasses (bound to RelationshipClasses Combobox) with the names of the RelationshipClasses 
        /// in which the FeatureClass/Table corresponding to the selected layer participates (as Origin or Destination)
        /// </summary>
        public void UpdateRelationshipClasses()
        {
            QueuedTask.Run(() =>
            {
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
                    RelationshipClasses = new ObservableCollection<string>(relationshipClassNames);
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

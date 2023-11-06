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
using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Data.NetworkDiagrams;
using ArcGIS.Core.Data.UtilityNetwork;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ShowContainment
{
    internal static class Utility
    {
        public static NetworkDiagram m_diagram;
        /// <summary>
        /// List of selected junction ID 
        /// </summary>
        public static List<long> GlobalSelectedJunctionIDs { get; set; }

        ///// <summary>
        ///// List of selected edge ID 
        ///// </summary>
        public static List<long> GlobalSelectedEdgeIDs { get; set; }

        ///// <summary>
        ///// List of selected container ID 
        ///// </summary>
        public static List<long> GlobalSelectedContainerIDs { get; set; }

        public static SelectionSet GlobalMapSelection { get; set; }

        public static List<DiagramContainerElement> GlobalDiagramContainerElements;
        public static List<DiagramJunctionElement> GlobalDiagramJunctionElements;
        public static List<DiagramEdgeElement> GlobalDiagramEdgeElements;
        public static int SelectionCount = 0;
        public enum Selected
        {
            NOTHING,
            ONE,
            MORE,
            INVALID
        }

        public static string messError { get; set; }

        internal static string messMoreFeatures = "There are more than one selected diagram features in the active diagram. This command expects a single selected diagram feature";

        internal static string messNoFeatures = "There are no selected diagram features in the active diagram. This command expects a single selected diagram feature";

        internal static string messNoContents = "There is no content to show, the selected diagram feature doesn’t represent any container for the ";

        public static string messNoTemplateExpand = "This add-in command doesn't find any ExpandContainers template for this utility network";

        public static string messNoTemplateBasic = "This add-in command doesn't find any Basic template for this utility network";

        internal static string messSystemJunctionSelected = "This add-in command doesn't working with a system juntion selected";

        public static string messCannotExtend = "This command only works from active diagrams based on diagram templates that can be extended. It looks like the Extend Diagram property is disable for the diagram template on which the active diagram is based. Please run this command from a diagram based on an extendable diagram template";

        public static string templateExpandContainer = "ExpandContainers";
        
        public static string templateBasic = "Basic";

        public static DiagramLayer GetDiagramLayerFromMap(Map map)
        {
            if (map == null)
                return null;

            IReadOnlyList<Layer> myLayers = map.Layers;
            if (myLayers == null)
                return null;

            foreach (Layer l in myLayers)
            {
                if (l.GetType() == typeof(DiagramLayer))
                    return l as DiagramLayer;
            }

            return null;
        }
         
        public static NetworkDiagram GetNetworkDiagram(Map map)
        {
            DiagramLayer diagLayer = GetDiagramLayerFromMap(map);
            return diagLayer.GetNetworkDiagram();
        }
 
        public static async void ShowDiagram(NetworkDiagram Diagram, string diagramName)
        {
            // Create a diagram layer from a NetworkDiagram (myDiagram)
            DiagramLayer diagramLayer = await QueuedTask.Run<DiagramLayer>(() =>
            {
                try
                {
                    // Create the diagram map
                    var newMap = MapFactory.Instance.CreateMap(diagramName, MapType.NetworkDiagram, MapViewingMode.Map);
                    if (newMap == null)
                    {
                        messError = "Error create a map.";
                        return null;
                    }

                    // Open the diagram map
                    var mapPane = ArcGIS.Desktop.Core.ProApp.Panes.CreateMapPaneAsync(newMap, MapViewingMode.Map);
                    if (mapPane == null)
                    {
                        messError = "Error create a map pane.";
                        return null;
                    }

                    //Add the diagram to the map
                    return newMap.AddDiagramLayer(Diagram);
                }
                catch (Exception ex)
                {
                        messError = string.Format("Show container : \n{0}", ex.Message);
                }

                return null;
            });
        }
       
        internal static void CleanModule()
        {
            GlobalDiagramJunctionElements = null;
            GlobalDiagramEdgeElements = null;
            GlobalDiagramContainerElements = null;
            GlobalMapSelection = null;
            GlobalSelectedJunctionIDs = null;
            GlobalSelectedEdgeIDs = null;
            GlobalSelectedContainerIDs = null;
            SelectionCount = 0;
            messError = string.Empty;
        }

        public static Selected GetSelectionInMap(Map ReferenceMap)
        {
            CleanModule();

            GlobalSelectedJunctionIDs = new List<long>();
            GlobalSelectedContainerIDs = new List<long>();
            GlobalSelectedEdgeIDs = new List<long>();

            GlobalMapSelection = ReferenceMap.GetSelection();

            foreach (var v in GlobalMapSelection.ToDictionary())
            {
                FeatureLayer layer = v.Key as FeatureLayer;
                if (layer.ShapeType == esriGeometryType.esriGeometryPoint)
                {
                    foreach (var id in v.Value)
                    {
                        GlobalSelectedJunctionIDs.Add(id);
                        SelectionCount++;
                    }
                }
                if (layer.ShapeType == esriGeometryType.esriGeometryPolygon)
                {
                    foreach (var id in v.Value)
                    {
                        GlobalSelectedContainerIDs.Add(id);
                        SelectionCount++;
                    }
                }
                else if (layer.ShapeType == esriGeometryType.esriGeometryPolyline)
                {
                    foreach (var id in v.Value)
                    {
                        GlobalSelectedEdgeIDs.Add(id);
                        SelectionCount++;
                    }
                }
                else if (layer.ShapeType == esriGeometryType.esriGeometryLine)
                {
                    return Selected.INVALID;
                }
            }

            if (SelectionCount == 0)
            {
                return Selected.NOTHING;
            }
            else if (SelectionCount > 1)
            {
                return Selected.MORE;
            }

            DiagramElementQueryResult deqr;

            DiagramElementQueryByObjectIDs query1 = new()
            {
                AddConnected = false,
                AddContents = false,
                ContainerObjectIDs = GlobalSelectedContainerIDs,
                JunctionObjectIDs = GlobalSelectedJunctionIDs,
                EdgeObjectIDs = GlobalSelectedEdgeIDs
            };

            deqr = m_diagram.QueryDiagramElements(query1);

            GlobalDiagramJunctionElements = deqr.DiagramJunctionElements.ToList();
            GlobalDiagramEdgeElements = deqr.DiagramEdgeElements.ToList();
            GlobalDiagramContainerElements = deqr.DiagramContainerElements.ToList();
            return Selected.ONE;
        }

        internal static bool IsContainer(Geodatabase geodatabase, string className, Guid guid)
        {
            Row contRow = null;
            FeatureClass contFeatureClass = OpenDataset<FeatureClass>(geodatabase, className);

            if (contFeatureClass == null)
                return false;

            QueryFilter subQueryFilter = new QueryFilter();
            subQueryFilter.WhereClause = GetGlobalIDWhereClause(contFeatureClass, guid);
            RowCursor featureCursor = contFeatureClass.Search(subQueryFilter);
            if (featureCursor.MoveNext())
                contRow = featureCursor.Current;

            string assFieldName = "AssociationStatus";
            object statusValue = contRow[assFieldName];
            int statusIntValue = Convert.ToInt32(statusValue);
        
            if (statusIntValue % 2 ==  1)
                return true;

            return false;
        }

        private static T OpenDataset<T>(Geodatabase geodatabase, String name) where T : Dataset
        {
            IReadOnlyList<FeatureClassDefinition> allFeatureClasses = geodatabase.GetDefinitions<FeatureClassDefinition>();
            foreach (FeatureClassDefinition fcDef in allFeatureClasses)
            {
                string aliasName = fcDef.GetAliasName();
                if (!String.IsNullOrEmpty(aliasName))
                {
                    if (aliasName.Equals(name, StringComparison.OrdinalIgnoreCase) ||
                      aliasName.Replace(" ", "").Equals(name, StringComparison.OrdinalIgnoreCase))
                    {
                        return geodatabase.OpenDataset<T>(fcDef.GetName());
                    }
                }
                else
                {
                    string fullName = fcDef.GetName();
                    if (fullName.Replace("_", "").Contains(name))
                    {
                        return geodatabase.OpenDataset<T>(fullName);
                    }
                }
            }

            return null;
        }
    
        private static string GetGlobalIDWhereClause(Table table, Guid globalID)
        {
            using (TableDefinition tableDefinition = table.GetDefinition())
            {
                return tableDefinition.GetGlobalIDField() + " = '{" + globalID.ToString().ToUpper() + "}'";
            }
        }
        
        public static UtilityNetwork GetUtilityNetworkFromMap(Map MapToSearch = null)
        {
            if (MapToSearch == null)
                MapToSearch = MapView.Active.Map;
            if (MapToSearch == null)
                return null;

            IReadOnlyList<Layer> myLayers = MapToSearch.GetLayersAsFlattenedList();

            foreach (Layer l in myLayers)
            {
                if (l.GetType() == typeof(UtilityNetworkLayer))
                    return ((UtilityNetworkLayer)l).GetUtilityNetwork();
                else if (l.GetType() == typeof(DiagramLayer))
                {
                    DiagramLayer dl = l as DiagramLayer;
                    NetworkDiagram nd = dl.GetNetworkDiagram();
                    return nd.DiagramManager.GetNetwork<UtilityNetwork>();
                }
            }

            return null;
        }

        public static bool CheckValidation(MapView mapView)
        {
            if (mapView == null)
            {
                messError = "Please select a feature layer.";
                return false;
            }

            DiagramLayer diagramLayer = Utility.GetDiagramLayerFromMap(mapView.Map);
            if (diagramLayer == null)
            {
                messError = "it should have a diagram layer.";
                return false;
            };

            m_diagram = GetNetworkDiagram(mapView.Map);

            Selected select = GetSelectionInMap(mapView.Map);

            if (select == Selected.NOTHING)
            {
                messError = messNoFeatures;
                return false;
            }
            else if (select == Selected.MORE)
            {
                messError = messMoreFeatures;
                return false;

            }
            else if (select == Selected.INVALID)
            {
                messError = messNoContents;
                return false;
            }

            return true;
        }

        public static void AddGUIToList(MapView view, UtilityNetwork un, ref IList<Guid> listElements, ref IList<Guid> listGlobalIDs)
        {
            try
            {               
                un = GetUtilityNetworkFromMap(view.Map);
                Geodatabase geodatabase = un.GetDatastore() as Geodatabase;

                if (GlobalDiagramContainerElements.Count == 1)
                {
                    DiagramContainerElement cont = GlobalDiagramContainerElements[0];
                    listElements.Add(cont.AssociatedGlobalID);
                    listGlobalIDs.Add(cont.GlobalID);
                }
                else if (GlobalDiagramJunctionElements.Count == 1)
                {
                    DiagramJunctionElement junc = GlobalDiagramJunctionElements[0];

                    if (junc.AssociatedSourceID == 2) // do not treat a system juntion
                    {
                        messError = messSystemJunctionSelected;
                        return;
                    }

                    string fclassName1 = un.GetDefinition().GetNetworkSources().First(x => x.ID == junc.AssociatedSourceID).Name;

                    int position = fclassName1.IndexOf(".");

                    if (position != -1)
                        fclassName1 = fclassName1.Substring(position + 1);

                    if (IsContainer(geodatabase, fclassName1, junc.AssociatedGlobalID))
                    {
                        listElements.Add(junc.AssociatedGlobalID);
                        listGlobalIDs.Add(junc.GlobalID);
                    }
                    else
                    {
                        messError = messNoContents + un.GetName() + " utility network";
                        return;
                    }
                }
                else if (GlobalDiagramEdgeElements.Count == 1)
                {
                    DiagramEdgeElement edge = GlobalDiagramEdgeElements[0];

                    string fclassName1 = un.GetDefinition().GetNetworkSources().First(x => x.ID == edge.AssociatedSourceID).Name;
                    int position = fclassName1.IndexOf(".");

                    if (position != -1)
                        fclassName1 = fclassName1.Substring(position + 1);

                    if (IsContainer(geodatabase, fclassName1, edge.AssociatedGlobalID))
                    {
                        listElements.Add(edge.AssociatedGlobalID);
                        listGlobalIDs.Add(edge.GlobalID);
                    }
                    else
                    {
                        messError = messNoContents + un.GetName() + " utility network";
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                messError = ex.Message;
                return;
            }
        }
    }
}

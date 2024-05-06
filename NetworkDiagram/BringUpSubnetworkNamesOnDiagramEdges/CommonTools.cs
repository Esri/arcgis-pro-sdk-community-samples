/*

   Copyright 2024 Esri

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
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Dialogs = ArcGIS.Desktop.Framework.Dialogs;


namespace BringUpSubnetworkNamesOnDiagramEdges
{
  internal static class CommonTools
  {
    #region internal static fields
    /// <summary>
    /// Maximum value in where clause IN
    /// </summary>
    private const int ModuleMaxSqlValueInWhereClause = 999;

    /// <summary>
    /// Active Map Name
    /// </summary>
    internal static string GlobalActiveMapName;

    /// <summary>
    /// Active DiagramLayer
    /// </summary>
    internal static DiagramLayer GlobalDiagramLayer { get; set; }

    /// <summary>
    /// Active NetworkDiagram
    /// </summary>
    internal static NetworkDiagram GlobalDiagram { get; set; }

    /// <summary>
    /// Active DiagramManager
    /// </summary>
    internal static DiagramManager GlobalDiagramManager { get; set; }

    /// <summary>
    /// Active UtilityNetwork
    /// </summary>
    internal static UtilityNetwork GlobalUtilityNetwork { get; set; }

    /// <summary>
    /// Active Geodatabase
    /// </summary>
    internal static Geodatabase GlobalGeodatabase { get; set; }

    /// <summary>
    /// Indicate whether it is a subnetwork system diagram 
    /// </summary>
    internal static bool GlobalIsSystem { get; set; }

    /// <summary>
    /// Indicate whether it is a stored diagram 
    /// </summary>
    internal static bool GlobalIsStored { get; set; }

    /// <summary>
    /// Indicate whether it is a versionned diagram
    /// </summary>
    internal static bool GlobalIsVersioned { get; set; }

    /// <summary>
    /// Indicate whether it is a utility network version 7
    /// </summary>
    internal static bool IsUNV7Flag { get; set; }

    /// <summary>
    /// List of the selected diagram junction IDs 
    /// </summary>
    internal static List<long> GlobalSelectedJunctionIDs { get; set; }

    /// <summary>
    /// List of the selected diagram edge IDs
    /// </summary>
    internal static List<long> GlobalSelectedEdgeIDs { get; set; }

    /// <summary>
    /// List of the selected diagram container IDs 
    /// </summary>
    internal static List<long> GlobalSelectedContainerIDs { get; set; }

    /// <summary>
    /// Container margin 
    /// </summary>
    internal static double GlobalContainerMargin { get; set; }

    /// <summary>
    /// Map selection, used to restore the initial selection once the process completes 
    /// </summary>
    internal static SelectionSet GlobalMapSelection { get; set; }

    /// <summary>
    /// Active GeodatabaseType
    /// </summary>
    internal static GeodatabaseType GlobalTypeGeo { get; set; }

    /// <summary>
    /// List of the diagram junction elements in a diagram
    /// </summary>
    internal static List<DiagramJunctionElement> GlobalDiagramJunctionElements;

    /// <summary>
    /// List of the diagram edge elements in a diagram
    /// </summary>
    internal static List<DiagramEdgeElement> GlobalDiagramEdgeElements;

    /// <summary>
    /// List of the diagram container elements in a diagram
    /// </summary>
    internal static List<DiagramContainerElement> GlobalDiagramContainerElements;

    /// <summary>
    /// List of the diagram junctions to save
    /// </summary>
    internal static List<DiagramJunctionElement> GlobalJunctionsToSave;

    /// <summary>
    /// List of the diagram edges to save
    /// </summary>
    internal static List<DiagramEdgeElement> GlobalEdgesToSave;

    /// <summary>
    /// List of of the diagram containers to save
    /// </summary>
    internal static List<DiagramContainerElement> GlobalContainersToSave;

    /// <summary>
    /// Diagram Envelope
    /// </summary>
    internal static Envelope GlobalEnvelope;

    /// <summary>
    /// Active Map
    /// </summary>
    internal static Map GlobalActiveMap;

    /// <summary>
    /// Flag Set to True when a function is running
    /// Stop Cleanning module
    /// </summary>
    internal static bool GlobalIsRunning = false;
    #endregion

    /// <summary>
    /// Clean all global fields
    /// </summary>
    internal static void CleanModule()
    {
      GlobalActiveMapName = "";
      GlobalDiagramLayer = null;
      GlobalDiagram = null;
      GlobalDiagramManager = null;
      GlobalUtilityNetwork = null;
      GlobalGeodatabase = null;
      GlobalSelectedJunctionIDs = null;
      GlobalSelectedEdgeIDs = null;
      GlobalSelectedContainerIDs = null;
      GlobalDiagramJunctionElements = null;
      GlobalDiagramEdgeElements = null;
      GlobalDiagramContainerElements = null;
      GlobalJunctionsToSave = null;
      GlobalEdgesToSave = null;
      GlobalContainersToSave = null;
      GlobalEnvelope = null;
      GlobalActiveMap = null;
    }

    /// <summary>
    /// Empty any list of selected objects
    /// </summary>
    internal static void CleanSelections()
    {
      if (GlobalSelectedContainerIDs == null)
        GlobalSelectedContainerIDs = [];
      else
        GlobalSelectedContainerIDs.Clear();

      if (GlobalSelectedEdgeIDs == null)
        GlobalSelectedEdgeIDs = [];
      else
        GlobalSelectedEdgeIDs.Clear();

      if (GlobalSelectedJunctionIDs == null)
        GlobalSelectedJunctionIDs = [];
      else
        GlobalSelectedJunctionIDs.Clear();
    }

    /// <summary>
    /// Format exception and InnerException
    /// </summary>
    /// <param name="ex">Exception</param>
    /// <returns>Return exception and InnerException as string</returns>
    internal static string ExceptionFormat(Exception ex)
    {
      if (ex.InnerException != null)
      {
        string s = ExceptionFormat(ex.InnerException);

        if (s.Length > 0)
          return $"{ex.Message}\n{s}";
      }

      return ex.Message;
    }

    /// <summary>
    /// Format the GUID list to the where clause GUID IN (List of GUIDs>
    /// </summary>
    /// <param name="selectEID">List of GUIDs to format</param>
    /// <returns>List of GUIDs string</returns>
    /// <remarks>Due to database limitations the number of GUIDs in a list is limited
    /// This max number is defined by the constant ModuleMaxSqlValueInWhereClause</remarks>
    internal static List<string> FormatGuidToString(List<Guid> selectEID)
    {
      string s = "";
      int i = 0;
      List<string> lEid = [];
      foreach (Guid il in selectEID)
      {
        i++;
        s += "'{" + il.ToString().ToUpper() + "}',";
        if (i > ModuleMaxSqlValueInWhereClause)
        {
          s = s[0..^1];
          lEid.Add(s);
          s = "";
          i = 0;
        }
      }

      if (!string.IsNullOrEmpty(s))
      {
        s = s[0..^1];
        lEid.Add(s);
      }

      return lEid;
    }

    /// <summary>
    /// Get Diagram Layer from Map
    /// </summary>
    /// <param name="map">Map</param>
    /// <returns>Diagram Layer</returns>
    internal static DiagramLayer GetDiagramLayerFromMap(Map map)
    {
      if (map == null || map.MapType != MapType.NetworkDiagram)
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

    /// <summary>
    /// Get the active geodatabase fom the active map
    /// </summary>
    /// <returns>Geodatabase</returns>
    private static Geodatabase GetGeodatabaseFromActiveMap()
    {
      UtilityNetwork un = GetUtilityNetworkFromActiveMap();
      if (un != null)
        return un.GetDatastore() as Geodatabase;

      return null;
    }

    /// <summary>
    /// Set Global fields
    /// </summary>
    /// <param name="map">Diagram Layer</param>
    internal static void GetParameters(Map map)
    {
      if (map == null)
      {
        CleanModule();
        return;
      }

      if (GlobalActiveMapName != map.Name || GlobalDiagram == null || GlobalDiagramLayer == null)
      {
        GlobalActiveMap = map;
        GlobalActiveMapName = GlobalActiveMap.Name;
        GlobalDiagramLayer = GetDiagramLayerFromMap(map);
        if (GlobalDiagramLayer == null)
          CleanModule();
        else
        {
          QueuedTask.Run(() =>
          {
            try
            {
              GlobalDiagram = GlobalDiagramLayer.GetNetworkDiagram();
            }
            catch
            {
              GlobalDiagram = null;
            }

            try
            {
              if (GlobalDiagram != null)
              {
                GlobalDiagramManager = GlobalDiagram.DiagramManager;

                GlobalUtilityNetwork = GlobalDiagramManager.GetNetwork<UtilityNetwork>();

                GlobalGeodatabase = GlobalUtilityNetwork.GetDatastore() as Geodatabase;

                try
                {
                  UtilityNetwork unTest = GlobalGeodatabase.OpenDataset<UtilityNetwork>(GlobalUtilityNetwork.GetName());
                  if (unTest != null)
                  {
                    int unVersion = Convert.ToInt32(CommonTools.GlobalUtilityNetwork.GetDefinition().GetSchemaVersion());
                    IsUNV7Flag = unVersion >= 7;
                  }
                  else
                    IsUNV7Flag = false;
                }
                catch
                {
                  // This is a Trace Network
                  IsUNV7Flag = false;
                }

                NetworkDiagramInfo networkDiagramInfo = GlobalDiagram.GetDiagramInfo();
                if (networkDiagramInfo == null)
                {
                  GlobalIsSystem = false;
                  GlobalEnvelope = null;
                  GlobalIsStored = false;
                  GlobalContainerMargin = 0.5;
                }
                else
                {
                  GlobalIsSystem = networkDiagramInfo.IsSystem;
                  GlobalEnvelope = networkDiagramInfo.DiagramExtent;
                  GlobalIsStored = networkDiagramInfo.IsStored;
                  GlobalContainerMargin = networkDiagramInfo.ContainerMargin;
                }

                GlobalTypeGeo = GlobalGeodatabase.GetGeodatabaseType();

                if (GlobalTypeGeo == GeodatabaseType.Service)
                {
                  VersionManager vm = GlobalGeodatabase.GetVersionManager();
                  if (vm != null)
                    GlobalIsVersioned = (vm.GetCurrentVersion() != null);
                  else
                    GlobalIsVersioned = false;
                }
                else
                  GlobalIsVersioned = false;

              }
              else
                GlobalDiagramLayer = null;

              CleanSelections();
            }
            catch (Exception ex)
            {
              ShowException(exception: ex);

              CleanModule();
            }
          });
        }
      }
    }

    /// <summary>
    /// Get a RowCursor of the source
    /// </summary>
    /// <param name="Source">Feature class source</param>
    /// <param name="SearchGuid">Guid list</param>
    /// <param name="ListSearchFields">Searched field list</param>
    /// <param name="WhereField">Guid field name in where clause</param>
    /// <param name="FieldsName">Qualified field name list</param>
    /// <returns>RowCursor</returns>
    internal static RowCursor GetRowCursorFromFeatureClassAndGuidList(FeatureClass Source, List<Guid> SearchGuid, List<string> ListSearchFields, string WhereField, out List<Tuple<string, string>> FieldsName)
    {
      InitializeFields(TableFields: Source.GetDefinition().GetFields(), ListSearchFields: ListSearchFields, WhereField: WhereField, FieldsName: out FieldsName, ListFieldName: out string ListFieldName, SearchField: out string SearchField);

      List<string> stringGuids = FormatGuidToString(SearchGuid);

      StringBuilder sb = new();

      foreach (string se in stringGuids)
        sb.AppendFormat("{0} IN ({1}) OR ", SearchField, se);

      string s = sb.ToString();
      QueryFilter query = new()
      {
        SubFields = ListFieldName,
        WhereClause = s[0..^4]
      };

      return Source.Search(query);
    }

    /// <summary>
    /// Get a RowCursor of the source
    /// </summary>
    /// <param name="SourceName">Source name</param>
    /// <param name="SearchGuid">Guid list</param>
    /// <param name="ListSearchFields">Searched field list</param>
    /// <param name="WhereField">Guid field name in where clause</param>
    /// <param name="FieldsName">Qualified field name list</param>
    /// <returns>RowCursor</returns>
    internal static RowCursor GetRowCursorFromSourceNameAndGuidList(string SourceName, List<Guid> SearchGuid, List<string> ListSearchFields, string WhereField, out List<Tuple<string, string>> FieldsName)
    {
      if (string.IsNullOrEmpty(SourceName))
      {
        FieldsName = null;
        return null;
      }

      using FeatureClass tSource = OpenDataset<FeatureClass>(SourceName.Replace(" ", ""));
      return GetRowCursorFromFeatureClassAndGuidList(Source: tSource, SearchGuid: SearchGuid, WhereField: WhereField, ListSearchFields: ListSearchFields, FieldsName: out FieldsName);
    }

    /// <summary>
    /// Get Utility Network from a map
    /// </summary>
    /// <param name="map">Map, use the active map if missing</param>
    /// <returns>Utility Network</returns>
    internal static UtilityNetwork GetUtilityNetworkFromActiveMap(Map map = null)
    {
      map ??= MapView.Active?.Map;
      if (map == null)
        return null;

      IReadOnlyList<Layer> myLayers = map.GetLayersAsFlattenedList();

      foreach (Layer l in myLayers)
      {
        try
        {
          if (l is UtilityNetworkLayer unLayer)
            return unLayer.GetUtilityNetwork();
          else if (l is DiagramLayer dl)
          {
            NetworkDiagram nd = dl.GetNetworkDiagram();
            return nd.DiagramManager.GetNetwork<UtilityNetwork>();
          }
        }
        catch { }
      }

      return null;
    }

    /// <summary>
    /// Flag for UN V7 and higher
    /// </summary>
    /// <param name="Utility">Utility Network</param>
    /// <returns>True if UN is a 7 verion or higher</returns>
    internal static bool IsUNV7(UtilityNetwork Utility)
    {
      if (Utility == null)
        return false;

      int version = Convert.ToInt32(Utility.GetDefinition().GetSchemaVersion());
      if (version > 6)
      {
        FrameworkApplication.State.Activate("BringUpSubnetworkNamesOnDiagramEdges_IsUNV7");
        return true;
      }
      else
      {
        FrameworkApplication.State.Deactivate("BringUpSubnetworkNamesOnDiagramEdges_IsUNV7");
        return false;
      }
    }

    /// <summary>
    /// Get the qualified field name list
    /// </summary>
    /// <param name="TableFields">Fields list</param>
    /// <param name="ListSearchFields">Searched field list</param>
    /// <param name="WhereField">Guid field name in where clause</param>
    /// <param name="FieldsName">Qualified field name list</param>
    /// <param name="ListFieldName"></param>
    /// <param name="SearchField">qualified Where field name</param>
    internal static void InitializeFields(IReadOnlyList<Field> TableFields, List<string> ListSearchFields, string WhereField, out List<Tuple<string, string>> FieldsName, out string ListFieldName, out string SearchField)
    {
      FieldsName = [];
      ListFieldName = "";
      SearchField = "";

      foreach (Field field in TableFields)
      {
        if (IsNameInList(ListSearchFields, field.Name, out string OriginalName))
        {
          ListFieldName += field.Name + ",";
          FieldsName.Add(Tuple.Create(OriginalName, field.Name));
        }

        if (IsNameInList([WhereField], field.Name, out string Original))
          SearchField = field.Name;
      }

      ListFieldName = ListFieldName[0..^1];
    }

    /// <summary>
    /// If NetworkDiagram exists, fill the lists of DiagramElements, with the selection
    /// If there is no selection and GetSelection is true, get all DiagramElements in diagram
    /// </summary>
    /// <param name="GetAllElements">Get all DiagramElements in diagram</param>
    /// <param name="GetSelection">Get only DiagramElements in selection, if exist, if not get all</param>
    /// <param name="GetJunctionInfo">Get info for junction</param>
    /// <param name="GetEdgeInfo">Get info for Edge</param>
    /// <param name="GetContainerInfo">Get info for container</param>
    /// <returns>NetworkDiagram exists</returns>
    internal static bool IsDiagramUsable(bool GetAllElements = false, bool GetSelection = false, bool IsUN7TN4 = true)
    {
      if (GlobalDiagram == null || GlobalGeodatabase == null)
        GetParameters(MapView.Active.Map);

      if (GlobalDiagram == null)
      {
        CleanModule();
        return false;
      }

      if (IsUN7TN4 && !IsUNV7Flag)
      {
        Dialogs.MessageBox.Show("This tool only works with Utility Network version 7 or higher");
        CleanModule();
        return false;
      }

      if (GlobalGeodatabase == null || GlobalGeodatabase.HasEdits())
      {
        CleanModule();
        return false;
      }

      if (GetAllElements == false && GetSelection == false)
        return true;

      GlobalDiagramJunctionElements = [];
      GlobalDiagramEdgeElements = [];
      GlobalDiagramContainerElements = [];

      if (GetSelection)
      {
        GlobalSelectedJunctionIDs = [];
        GlobalSelectedContainerIDs = [];
        GlobalSelectedEdgeIDs = [];

        GlobalMapSelection = MapView.Active.Map.GetSelection();
        foreach (var v in GlobalMapSelection.ToDictionary())
        {
          FeatureLayer layer = v.Key as FeatureLayer;
          if (layer.ShapeType == esriGeometryType.esriGeometryPoint)
          {
            foreach (var id in v.Value)
              GlobalSelectedJunctionIDs.Add(id);
          }
          else if (layer.ShapeType == esriGeometryType.esriGeometryPolygon)
          {
            foreach (var id in v.Value)
              GlobalSelectedContainerIDs.Add(id);
          }
          else
          {
            foreach (var id in v.Value)
              GlobalSelectedEdgeIDs.Add(id);
          }
        }
      }
      else
        GlobalMapSelection = null;

      DiagramElementQueryResult deqr;
      if (GetAllElements || (GlobalSelectedContainerIDs.Count + GlobalSelectedEdgeIDs.Count + GlobalSelectedJunctionIDs.Count == 0))
      {
        DiagramElementQueryByElementTypes query = new()
        {
          QueryDiagramContainerElement = true,
          QueryDiagramEdgeElement = true,
          QueryDiagramJunctionElement = true
        };
        deqr = GlobalDiagram.QueryDiagramElements(query);
      }
      else
      {
        DiagramElementQueryByObjectIDs query1 = new()
        {
          AddConnected = true,
          AddContents = true,
          ContainerObjectIDs = GlobalSelectedContainerIDs,
          JunctionObjectIDs = GlobalSelectedJunctionIDs,
          EdgeObjectIDs = GlobalSelectedEdgeIDs
        };

        deqr = GlobalDiagram.QueryDiagramElements(query1);
      }

      foreach (var v in deqr.DiagramJunctionElements)
        GlobalDiagramJunctionElements.Add(v);

      foreach (var v in deqr.DiagramEdgeElements)
        GlobalDiagramEdgeElements.Add(v);

      foreach (var v in deqr.DiagramContainerElements)
        GlobalDiagramContainerElements.Add(v);

      return true;
    }

    /// <summary>
    /// Search a name in a qualified name list
    /// </summary>
    /// <param name="NameList">Name list</param>
    /// <param name="SearchName">Search Name</param>
    /// <param name="OriginalName">Original name</param>
    /// <returns>true if found</returns>
    private static bool IsNameInList(List<string> NameList, string SearchName, out string OriginalName)
    {
      int index = SearchName.LastIndexOf('.');
      string search;

      if (index == -1)
        search = SearchName.ToLower();
      else
        search = SearchName[(index + 1)..].ToLower();

      foreach (string s in NameList)
      {
        string sLower = s.ToLower();

        if (sLower == search)
        {
          OriginalName = s;
          return true;
        }
      }

      OriginalName = "";
      return false;
    }

    /// <summary>
    /// Open a dataset
    /// </summary>
    /// <typeparam name="T">Dataset type</typeparam>
    /// <param name="name">Dataset name</param>
    /// <returns>Dataset</returns>
    internal static T OpenDataset<T>(string name) where T : Dataset
    {
      GlobalGeodatabase ??= GetGeodatabaseFromActiveMap();

      if (GlobalGeodatabase.GetGeodatabaseType() == GeodatabaseType.Service)
      {
        Type t = typeof(T);

        if (t == typeof(Table))
        {
          IReadOnlyList<TableDefinition> allTables = GlobalGeodatabase.GetDefinitions<TableDefinition>();
          foreach (TableDefinition tableDef in allTables)
          {
            string aliasName = tableDef.GetAliasName();
            if (aliasName.Replace(" ", "").Equals(name, StringComparison.OrdinalIgnoreCase))
            {
              return GlobalGeodatabase.OpenDataset<T>(tableDef.GetName());
            }
          }
        }
        else if (t == typeof(FeatureClass))
        {
          IReadOnlyList<FeatureClassDefinition> allFeatureClasses = GlobalGeodatabase.GetDefinitions<FeatureClassDefinition>();
          foreach (FeatureClassDefinition fcDef in allFeatureClasses)
          {
            string aliasName = fcDef.GetAliasName();
            if (!string.IsNullOrEmpty(aliasName))
            {
              if (aliasName.Replace(" ", "").Equals(name, StringComparison.OrdinalIgnoreCase))
              {
                return GlobalGeodatabase.OpenDataset<T>(fcDef.GetName());
              }
            }
            else
            {
              //Weird case, where the alias name is empty (Error Tables, Dirty Areas)
              string fullName = fcDef.GetName();
              if (fullName.Replace("_", "").Contains(name)) //the point errors, line errors, polygon errors and dirty areas come back as  Dirty_Areas (want to compare to DirtyAreas). 
              {
                return GlobalGeodatabase.OpenDataset<T>(fullName);
              }
            }
          }
        }
        else if (t == typeof(RelationshipClass))
        {
          IReadOnlyList<RelationshipClassDefinition> allRelationshipClasses = GlobalGeodatabase.GetDefinitions<RelationshipClassDefinition>();
          foreach (RelationshipClassDefinition rcDef in allRelationshipClasses)
          {
            string aliasName = rcDef.GetAliasName();
            if (aliasName.Replace(" ", "").Equals(name, StringComparison.OrdinalIgnoreCase))
            {
              return GlobalGeodatabase.OpenDataset<T>(rcDef.GetName());
            }
          }
        }
        else if (t == typeof(UtilityNetwork))
        {
          IReadOnlyList<UtilityNetworkDefinition> unDefinition = GlobalGeodatabase.GetDefinitions<UtilityNetworkDefinition>();
          foreach (UtilityNetworkDefinition unDef in unDefinition)
          {
            return GlobalGeodatabase.OpenDataset<T>(unDef.GetName());
          }
        }
        else
        {
          //There is no type supported in the Feature Service DB, have to return null
          return null;
        }
      }
      else
      {
        return GlobalGeodatabase.OpenDataset<T>(name);
      }
      return null;
    }

    /// <summary>
    /// Show the exception message
    /// </summary>
    /// <param name = "exception" >Exception</param >
    internal static void ShowException(Exception exception)
    {
      string s = ExceptionFormat(exception);

#if (DEBUG)
      s += Environment.NewLine + Environment.NewLine + exception.StackTrace + Environment.NewLine + Environment.NewLine;
      Debug.WriteLine(s);
      Dialogs.MessageBox.Show(s);
#else
      Dialogs.MessageBox.Show(s);
#endif
    }


    /// <summary>
    /// Get a editable fields list by Source Name
    /// </summary>
    /// <param name="SourceName">Network Source Name</param>
    /// <param name="dataset">Table</param>
    /// <returns>Fields list</returns>
    internal static List<Field> GetFieldListBySourceName(string SourceName, ref Table dataset)
    {
      List<Field> fields = [];

      try
      {
        dataset = OpenDataset<Table>(SourceName);
      }
      catch (Exception ex)
      {
        ShowException(ex);
        return fields;
      }

      foreach (Field f in dataset.GetDefinition().GetFields())
      {
        if (f.IsEditable)
        {
          fields.Add(f);
        }
        else if (f.FieldType == FieldType.GlobalID)
        {
          fields.Add(f);
        }
        else if (f.Name.Contains("AssetGroup", StringComparison.OrdinalIgnoreCase) && !f.Name.Contains('.'))
        {
          fields.Add(f);
        }
        else if (f.Name.Contains("AssetType", StringComparison.OrdinalIgnoreCase) && !f.Name.Contains('.'))
        {
          fields.Add(f);
        }
      }

      return fields;
    }
  }
}

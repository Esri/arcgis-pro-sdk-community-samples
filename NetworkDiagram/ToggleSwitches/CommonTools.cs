using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Data.UtilityNetwork;
using ArcGIS.Core.Data.UtilityNetwork.NetworkDiagrams;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;


namespace ToggleSwitches
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
    /// Indicate if it is a system diagram 
    /// </summary>
    internal static bool GlobalIsSystem { get; set; }

    /// <summary>
    /// Indicate if it is a stored diagram 
    /// </summary>
    internal static bool GlobalIsStored { get; set; }

    /// <summary>
    /// Indicate if it is a versionned diagram
    /// </summary>
    internal static bool GlobalIsVersioned { get; set; }

    /// <summary>
    /// List of selected junction ID 
    /// </summary>
    internal static List<long> GlobalSelectedJunctionIDs { get; set; }

    /// <summary>
    /// List of selected edge ID 
    /// </summary>
    internal static List<long> GlobalSelectedEdgeIDs { get; set; }

    /// <summary>
    /// List of selected container ID 
    /// </summary>
    internal static List<long> GlobalSelectedContainerIDs { get; set; }

    /// <summary>
    /// Container margin 
    /// </summary>
    internal static double GlobalContainerMargin { get; set; }

    /// <summary>
    /// Map selection, used to reselect the same objects after running tool 
    /// </summary>
    internal static Dictionary<MapMember, List<long>> GlobalMapSelection { get; set; }

    /// <summary>
    /// Active GeodatabaseType
    /// </summary>
    internal static GeodatabaseType GlobalTypeGeo { get; set; }

    /// <summary>
    /// List of Diagram Junction Element contained in diagram
    /// </summary>
    internal static List<DiagramJunctionElement> GlobalDiagramJunctionElements;

    /// <summary>
    /// List of Diagram Edge Element contained in diagram
    /// </summary>
    internal static List<DiagramEdgeElement> GlobalDiagramEdgeElements;

    /// <summary>
    /// List of Diagram Container Element contained in diagram
    /// </summary>
    internal static List<DiagramContainerElement> GlobalDiagramContainerElements;

    /// <summary>
    /// List of Diagram Junction to save
    /// </summary>
    internal static List<DiagramJunctionElement> GlobalJunctionsToSave;

    /// <summary>
    /// List of Diagram Edge to save
    /// </summary>
    internal static List<DiagramEdgeElement> GlobalEdgesToSave;

    /// <summary>
    /// List of Diagram Container to save
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
    /// Empty all list of selected objects
    /// </summary>
    internal static void CleanSelections()
    {
      if (GlobalSelectedContainerIDs == null)
        GlobalSelectedContainerIDs = new List<long>();
      else
        GlobalSelectedContainerIDs.Clear();

      if (GlobalSelectedEdgeIDs == null)
        GlobalSelectedEdgeIDs = new List<long>();
      else
        GlobalSelectedEdgeIDs.Clear();

      if (GlobalSelectedJunctionIDs == null)
        GlobalSelectedJunctionIDs = new List<long>();
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
          return String.Format("{0}\n{1}", ex.Message, s);
      }

      return ex.Message;
    }

    /// <summary>
    /// Foramt the Guid list to the where clause GUID IN (List of Guid>
    /// </summary>
    /// <param name="selectEID">List of guid to format</param>
    /// <returns>List of Guid string</returns>
    /// <remarks>Due to some database limitation the number of guid in a list is limited
    /// This max number is defined by the constant ModuleMaxSqlValueInWhereClause</remarks>
    private static List<string> FormatGuidToString(List<Guid> selectEID)
    {
      string s = "";
      int i = 0;
      List<string> lEid = new List<string>();
      foreach (Guid il in selectEID)
      {
        i++;
        s += "'{" + il.ToString().ToUpper() + "}',";
        if (i > ModuleMaxSqlValueInWhereClause)
        {
          s = s.Substring(0, s.Length - 1);
          lEid.Add(s);
          s = "";
          i = 0;
        }
      }

      if (!String.IsNullOrEmpty(s))
      {
        s = s.Substring(0, s.Length - 1);
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

      StringBuilder sb = new StringBuilder();

      foreach (string se in stringGuids)
        sb.AppendFormat("{0} IN ({1}) OR ", SearchField, se);

      string s = sb.ToString();
      QueryFilter query = new QueryFilter()
      {
        SubFields = ListFieldName,
        WhereClause = s.Substring(0, s.Length - 4)
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
      if (String.IsNullOrEmpty(SourceName))
      {
        FieldsName = null;
        return null;
      }

      using (FeatureClass tSource = OpenDataset<FeatureClass>(SourceName.Replace(" ", "")))
      {
        return GetRowCursorFromFeatureClassAndGuidList(Source: tSource, SearchGuid: SearchGuid, WhereField: WhereField, ListSearchFields: ListSearchFields, FieldsName: out FieldsName);
      }
    }

    /// <summary>
    /// Get Utility Network from a map
    /// </summary>
    /// <param name="map">Map, use the active map if missing</param>
    /// <returns>Utility Network</returns>
    internal static UtilityNetwork GetUtilityNetworkFromActiveMap(Map map = null)
    {
      if (map == null)
        map = MapView.Active.Map;
      if (map == null)
        return null;

      IReadOnlyList<Layer> myLayers = map.GetLayersAsFlattenedList();

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

    /// <summary>
    /// Retrieve the Utility Network from a map
    /// </summary>
    /// <param name="SearchMap">Map</param>
    /// <param name="SearchConnection">Connection to the database</param>
    /// <param name="UN">The utility Network found</param>
    /// <param name="SubnetLayers">Subnetwork layer list</param>
    /// <param name="NameFieldSubNet">Subnetwork field name</param>
    /// <returns>true if found</returns>
    internal static bool GetUNFromMap(Map SearchMap, string SearchConnection, ref UtilityNetwork UN, ref List<FeatureLayer> SubnetLayers, ref string NameFieldSubNet)
    {
      UtilityNetwork unSearch = GetUtilityNetworkFromActiveMap(SearchMap);
      if (unSearch == null)
        return false;

      bool bFound = false;

      string unConnect = ((Geodatabase)unSearch.GetDatastore()).GetConnectionString();
      bFound = (unConnect == SearchConnection);

      if (bFound)
      {
        if (UN == null)
          UN = unSearch;

        Table subnetTable = UN.GetSystemTable(SystemTableType.Subnetworks);

        IReadOnlyList<Field> listField;

        if (String.IsNullOrEmpty(NameFieldSubNet))
        {
          listField = subnetTable.GetDefinition().GetFields();

          Field FieldSubNetName = listField.FirstOrDefault(a => a.Name.ToLower() == "subnetworkname");
          NameFieldSubNet = FieldSubNetName?.Name;
        }

        IReadOnlyList<Layer> listLayers = SearchMap.GetLayersAsFlattenedList();
        foreach (Layer l in listLayers)
        {
          if (l is FeatureLayer fl && fl.ShapeType == esriGeometryType.esriGeometryPolyline)
          {
            listField = fl.GetTable().GetDefinition().GetFields();

            Field field = listField.FirstOrDefault(a => a.Name.ToLower() == "subnetworkcontrollernames"); // only subnetwork table contains tiername
            if (field != null)
            {
              SubnetLayers.Add(fl);
            }
          }
        }
      }

      return bFound && SubnetLayers.Count > 0;
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
    internal static void InitializeFields(IReadOnlyList<Field> TableFields, List<string> ListSearchFields, string WhereField, out List<Tuple<string, string>> FieldsName,
        out string ListFieldName, out string SearchField)
    {
      FieldsName = new List<Tuple<string, string>>();
      ListFieldName = "";
      SearchField = "";

      foreach (Field field in TableFields)
      {
        if (IsNameInList(ListSearchFields, field.Name, out string OriginalName))
        {
          ListFieldName += field.Name + ",";
          FieldsName.Add(Tuple.Create(OriginalName, field.Name));
        }

        if (IsNameInList(new List<string>() { WhereField }, field.Name, out string Original))
          SearchField = field.Name;
      }

      ListFieldName = ListFieldName.Substring(0, ListFieldName.Length - 1);
    }

    /// <summary>
    /// If NetworkDiagram exists, fill the lists of DiagramElements, with the selection
    /// If there is no selection and GetSelection is true, get all DiagramElements in diagram
    /// </summary>
    /// <param name="GetAllElements">Get all DiagramElements in diagram</param>
    /// <param name="GetSelection">Get only DiagramElements in selection, if exist, if not get all</param>
    /// <returns>NetworkDiagram exists</returns>
    internal static bool IsDiagramUsable(bool GetAllElements = false, bool GetSelection = false)
    {
      if (GlobalDiagram == null)
        GetParameters(MapView.Active.Map);

      if (GlobalDiagram == null) return false;

      if (GlobalGeodatabase.HasEdits())
        return false;

      if (GetAllElements == false && GetSelection == false)
        return true;

      if (GetSelection)
      {
        GlobalSelectedJunctionIDs = new List<long>();
        GlobalSelectedContainerIDs = new List<long>();
        GlobalSelectedEdgeIDs = new List<long>();

        GlobalMapSelection = MapView.Active.Map.GetSelection();
        foreach (var v in GlobalMapSelection)
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
      try
      {
        if (GetAllElements || (GlobalSelectedContainerIDs.Count + GlobalSelectedEdgeIDs.Count + GlobalSelectedJunctionIDs.Count == 0))
        {
          DiagramElementQueryByElementTypes query = new DiagramElementQueryByElementTypes
          {
            QueryDiagramContainerElement = true,
            QueryDiagramEdgeElement = true,
            QueryDiagramJunctionElement = true
          };
          deqr = GlobalDiagram.QueryDiagramElements(query);
        }
        else
        {
          DiagramElementQueryByObjectIDs query1 = new DiagramElementQueryByObjectIDs
          {
            AddConnected = true,
            AddContents = true,
            ContainerObjectIDs = GlobalSelectedContainerIDs,
            JunctionObjectIDs = GlobalSelectedJunctionIDs,
            EdgeObjectIDs = GlobalSelectedEdgeIDs
          };

          deqr = GlobalDiagram.QueryDiagramElements(query1);
        }
      }


      catch (Exception ex)
      {
        ShowException(exception: ex);
        return false;
      }

      GlobalDiagramJunctionElements = deqr.DiagramJunctionElements.ToList();
      GlobalDiagramEdgeElements = deqr.DiagramEdgeElements.ToList();
      GlobalDiagramContainerElements = deqr.DiagramContainerElements.ToList();

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
      int index = SearchName.LastIndexOf(".");
      string search;

      if (index == -1)
        search = SearchName.ToLower();
      else
        search = SearchName.Substring(index + 1).ToLower();

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
    private static T OpenDataset<T>(string name) where T : Dataset
    {
      if (GlobalGeodatabase == null)
        GlobalGeodatabase = GetGeodatabaseFromActiveMap();

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
            if (!String.IsNullOrEmpty(aliasName))
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
              if (fullName.Replace("_", "").Contains(name)) //the Point errors, line errors, polygon errors and dirty areas come back as  Dirty_Areas (want to compare to DirtyAreas). 
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
#endif
      ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(s);
    }

    /// <summary>
    /// Get the Un Schema version
    /// </summary>
    /// <param name="diagram">NetworkDiagram</param>
    /// <returns>string</returns>
    /// <remarks>UN Version 3 and earlier use only Subnetwork name
    /// UN Version 4 and later use Supported subnetwork name for container
    /// UN Version 5 and later use Supporting subnetwork name for structure</remarks>
    internal static int GetSchemaVersion(NetworkDiagram diagram)
    {
      DiagramManager diagramManager = diagram.DiagramManager;
      UtilityNetwork utilityNetwork = diagramManager.GetNetwork<UtilityNetwork>();

      UtilityNetworkDefinition unDefinition = utilityNetwork.GetDefinition();

      return Convert.ToInt32(unDefinition.GetSchemaVersion());
    }
  }
}

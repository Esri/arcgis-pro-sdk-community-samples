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
using ArcGIS.Core.Data.UtilityNetwork.Trace;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using static ArcGIS.Desktop.Internal.Core.PortalTrafficDataService.PortalDescriptionResponse;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Net;
using System.Runtime.InteropServices;
using Dialogs = ArcGIS.Desktop.Framework.Dialogs;
using System.Windows.Media;
using ArcGIS.Desktop.Internal.GeoProcessing;
using System.Diagnostics;
using System.Runtime.Intrinsics.X86;
using ActiproSoftware.Windows.Shapes;
using Microsoft.VisualBasic;

namespace CountAggregatedNetworkElements
{
  /// <summary>
  /// This add-in demonstrates how to fill the new Info field added with utility network version 7 using the attributes retrieved from the network elements associated with diagram features.
  /// In this sample, the developed add-in command opens a custom pane where the user can set the specific type of network elements he wants to retrieve and count among all the aggregated network elements related to any diagram feature.
  /// The add ins code sample then stores two different information separated using a single vertical line in the Info field for each diagram feature.The 1st information is the total count of network elements that the diagram feature aggregates.
  /// The 2nd information is the count of the specific aggregated network elements that the user wishes.
  /// **Note:** This code sample is generic and can be used for any utility network dataset once upgraded to version 7.
  /// </summary>
  /// <remarks>   
  /// 1. In Visual Studio click the Build menu. Then select Build Solution.
  /// 2. Click Start button to open ArcGIS Pro. 
  /// ArcGIS Pro will open.
  /// 3. Open C:\Data\NetworkDiagrams\FillsUpDiagramFeatureInfoField_UNv7\FillsUpDiagramFeatureInfoField_UNv7.aprx
  /// 4. Click on the Utility Network tab on the ribbon. Then, in the Diagram group, click Find Diagrams.
  /// ![UI](Screenshots/Screenshot1.png)
  /// The Find Diagrams pane opens. It shows a diagram that has been already stored; its name is TwelveMainDistributionSubnetworks.
  /// ![UI](Screenshots/Screenshot2.png)
  /// 5. Double-click TwelveMainDistributionSubnetworks in the diagram list.
  /// The stored diagram opens in a new diagram map.
  /// ![UI](Screenshots/Screenshot3.png)
  /// It represents a simplification of 12 distribution subnetworks. This diagram mainly focuses on critical devices, such as switches, the other portions of the subnetworks being aggregated under these remaining switches or under reduction edges that connect these switches
  /// 6. In the Contents pane, expand the TwelveMainDistributionSubnetworks diagram layer and scroll down until you see the Medium Voltage Switch sublayer.
  /// 7. Right click the Medium Voltage Switch sublayer and click Attribute Table.
  /// ![UI](Screenshots/Screenshot4.png)
  /// Info, the fourth field in the open table, is the new field added for any diagram feature within utility network version 7. For the time being and for this sample diagram, it shows values that have been filled using the BringUpSubnetworkNameOnDiagramEdges sample add-in command.
  /// You are going to retrieve and compute some other information using the newly installed CountAggregatedNetworkElements
  /// 8. Click on the Add-in tab on the ribbon and in the Count Objects group, click Count Aggregated Network Elements.
  /// ![UI](Screenshots/Screenshot5.png)
  /// The Count Aggregated Network Elements pane window opens.
  /// ![UI](Screenshots/Screenshot6.png)
  /// Suppose that for each diagram feature in the active diagram, you want to retrieve the count of the low voltage single phase residentials it serves. To get this information, you must set up the three lists in the pane as follows:
  /// 9. In the By Network Source dropdown list, pick up Electric Device.
  /// 10. Then, expand the By Asset Group list and select Low Voltage Service.
  /// 11. At last, expand the By Asset Type list and click Single Phase Residential LV
  /// ![UI](Screenshots/Screenshot7.png)
  /// 12. At the bottom of the pane, click Apply
  /// The process starts and retrieves the particular network elements you ask for that are aggregated for each diagram feature in the active diagram.It builds a string that concatenates the total count of network elements that each diagram feature represents and the count of the low voltage single phase residentials it aggregates.
  /// This new built string is saved in the Info field for each diagram feature.
  /// The open Medium Voltage Switch attribute table refreshes to reflect these new Info field values.
  /// ![UI](Screenshots/Screenshot8.png)
  /// 13. Use the Explore tool and click some of the Medium Voltage Switch diagram features. The Pop-up dialog opens and shows the Info field value for the clicked diagram feature.
  /// ![UI](Screenshots/Screenshot9.png)
  /// 14. In the Count Aggregated Network Elements pane window, at the left bottom corner, click the Refresh button to re-initialize the list
  /// 15. Then, pick up the network source, the network source/asset group couple or the network source/asset group/asset type triplet you want.For example, select Electric Device in the 1st list, then select Low Voltage Lighting.
  /// ![UI](Screenshots/Screenshot10.png)
  /// 16. At the bottom of the pane, click Apply.
  /// 17. Click the Explore tool and click any diagram feature you want to know about the count of the low voltage lighting it deserves.
  /// ![UI](Screenshots/Screenshot11.png)
  /// </remarks>
  internal class CountAggregatedNetworkElementsModule : Module
  {
    private static CountAggregatedNetworkElementsModule _this = null;

    /// <summary>
    /// Retrieve the singleton instance to this module here
    /// </summary>
    public static CountAggregatedNetworkElementsModule Current => _this ??= (CountAggregatedNetworkElementsModule)FrameworkApplication.FindModule("CountAggregatedNetworkElements_Module");

    internal const int GlobalMinUnVersion = 7;  // Set -1 to indicate that the tool does not work with UN
    internal const int GlobalMinTnVersion = -1; // Set -1 to indicate that the tool does not work with TN

    internal static List<long> GlobalSelectedJunctionIDs { get; set; }
    internal static List<long> GlobalSelectedContainerIDs { get; set; }
    internal static SelectionSet GlobalMapSelection { get; set; }
    internal static List<DiagramJunctionElement> GlobalDiagramJunctionElements;
    internal static List<DiagramContainerElement> GlobalDiagramContainerElements;
    internal static List<DiagramEdgeElement> GlobalDiagramEdgeElements;
    internal static int SelectionCount = 0;
    internal static UtilityNetwork GlobalUtilityNetwork { get; set; }
    internal static DiagramLayer GlobalDiagramLayer { get; set; }
    internal static NetworkDiagram GlobalDiagram { get; set; }
    internal static Geodatabase GlobalGeodatabase = null;
    internal static IReadOnlyList<NetworkSource> GlobalNetworkSources = null;

    internal static bool GlobalIsUn { get; set; }

    internal static List<Tuple<string, string>> _internalExternalSourceName = [];
    private static ManageCustomElement customGraph = new();

    internal const string cAssociation = "Association";
    public const string cIsUN = "CountAggregatedNetworkElements_ISUN";

    #region Overrides
    /// <summary>
    /// Called by Framework when ArcGIS Pro is closing
    /// </summary>
    /// <returns>False to prevent Pro from closing, otherwise True</returns>
    protected override bool CanUnload()
    {
      //TODO - add your business logic
      //return false to ~cancel~ Application close
      return true;
    }

    protected override bool Initialize()
    {
      FrameworkApplication.State.Activate(cIsUN);
      return base.Initialize();
    }

    protected override void Uninitialize()
    {
      base.Uninitialize();
    }
    #endregion Overrides

    internal static void CleanModule()
    {
      GlobalSelectedJunctionIDs = null;
      GlobalSelectedContainerIDs = null;
      GlobalMapSelection = null;
      GlobalDiagramJunctionElements = null;
      GlobalDiagramContainerElements = null;
      GlobalUtilityNetwork = null;
      GlobalDiagramLayer = null;
      GlobalDiagram = null;
      GlobalGeodatabase = null;
      SelectionCount = 0;
    }

    /// <summary>
    /// If NetworkDiagram exists, fill the lists of DiagramElements, with the selection
    /// If there is no selection and GetSelection is true, get all DiagramElements in diagram
    /// </summary>
    /// <param name="GetAllElements">Get all DiagramElements in diagram</param>
    /// <param name="GetSelection">Get only DiagramElements in selection, if exist, if not get all</param>
    /// <param name="ReferenceMap">Map where the Diagram should be found, if missing use the ActiveMap</param>
    /// <param name="AddConnected">Add connected DiagramElements</param>
    /// <param name="AddContent">Add all DiagramElements in DiagramElements container</param>
    /// <returns>NetworkDiagram exists</returns>
    internal static bool IsDiagramUsable(int MinUnVersion, int MinTnVersion, bool GetAllElements = false, bool GetSelection = false, Map ReferenceMap = null, bool AddConnected = false, bool AddContent = false)
    {
      if (ReferenceMap == null && (MapView.Active == null || MapView.Active.Map == null))
      {
        CleanModule();
        return false;
      }

      Map map = ReferenceMap ?? MapView.Active.Map;

      GlobalDiagramLayer = GetDiagramLayerFromMap(map);
      if (GlobalDiagramLayer == null)
      {
        CleanModule();
      }
      else
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
            DiagramManager GlobalDiagramManager = GlobalDiagram.DiagramManager;

            GlobalUtilityNetwork = GlobalDiagramManager.GetNetwork<UtilityNetwork>();
            if (GlobalUtilityNetwork == null)
            {
              CleanModule();
              return false;
            }

            try
            {
              GlobalGeodatabase = GlobalUtilityNetwork.GetDatastore() as Geodatabase;
            }
            catch
            {
              CleanModule();
              return false;
            }

            try
            {
              UtilityNetwork unTest = GlobalGeodatabase.OpenDataset<UtilityNetwork>(GlobalUtilityNetwork.GetName());
              if (unTest == null) // this is a Trace Network, there is no rules and no way to known the list of disposable edges
              {
                GlobalIsUn = false;
                if (MinTnVersion == -1)
                {
                  Dialogs.MessageBox.Show("This tool does not work on Trace Network.");
                  FrameworkApplication.State.Deactivate(cIsUN);
                  return false;
                }
                else if (!ValidateVersion(MinTnVersion, GlobalUtilityNetwork))
                {
                  Dialogs.MessageBox.Show($"This tool works only with Trace Network version {MinTnVersion} or higher");
                  FrameworkApplication.State.Deactivate(cIsUN);
                  return false;
                }
              }
              else if (MinUnVersion == -1)
              {
                Dialogs.MessageBox.Show("This tool does not work on Utility Network.");
                FrameworkApplication.State.Deactivate(cIsUN);
                return false;
              }
              else if (!ValidateVersion(MinUnVersion, GlobalUtilityNetwork))
              {
                Dialogs.MessageBox.Show($"This tool works only with Utility Network version {MinUnVersion} or higher");
                FrameworkApplication.State.Deactivate(cIsUN);
                return false;
              }
              FrameworkApplication.State.Activate(cIsUN);
              GlobalIsUn = true;
            }
            catch
            {
              // this is a Trace Network, there is no rules and no way to known the list of disposable edges
              if (MinTnVersion == -1)
              {
                Dialogs.MessageBox.Show("This tool does not work on Trace Network.");
                FrameworkApplication.State.Deactivate(cIsUN);
                return false;
              }
              if (!ValidateVersion(MinTnVersion, GlobalUtilityNetwork))
              {
                Dialogs.MessageBox.Show($"This tool works only with Trace Network version {MinTnVersion} or higher");
                FrameworkApplication.State.Deactivate(cIsUN);
                return false;
              }
              GlobalIsUn = false;
            }

            if (GlobalDiagram == null)
            {
              CleanModule();
              return false;
            }
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
      }

      if (GlobalDiagram == null || GlobalGeodatabase.HasEdits())
        return false;

      if (GetSelection)
        GetSelectionInMap(map);
      else
        GlobalMapSelection = null;

      DiagramElementQueryResult deqr;
      try
      {
        if (GetAllElements || (GlobalSelectedContainerIDs.Count + GlobalSelectedJunctionIDs.Count == 0))
        {
          deqr = GlobalDiagram.QueryDiagramElements(new DiagramElementQueryByElementTypes()
          {
            QueryDiagramContainerElement = true,
            QueryDiagramEdgeElement = true,
            QueryDiagramJunctionElement = true
          });
        }
        else
        {
          deqr = GlobalDiagram.QueryDiagramElements(new DiagramElementQueryByObjectIDs()
          {
            AddConnected = AddConnected,
            AddContents = AddContent,
            ContainerObjectIDs = GlobalSelectedContainerIDs,
            JunctionObjectIDs = GlobalSelectedJunctionIDs
          });
        }
      }
      catch (Exception ex)
      {
        ShowException(exception: ex);
        return false;
      }

      GlobalDiagramJunctionElements = [.. deqr.DiagramJunctionElements];
      GlobalDiagramContainerElements = [.. deqr.DiagramContainerElements];
      GlobalDiagramEdgeElements = [.. deqr.DiagramEdgeElements];

      //ValidateSelection(map);
      return true;
    }

    /// <summary>
    /// Validate the version of the Utility Network
    /// </summary>
    /// <param name="MinVersion">Verion minimum</param>
    /// <param name="Utility">Utility Network</param>
    /// <returns>True if the Utility Network version is greater than or equal to the MinVersion </returns>
    internal static bool ValidateVersion(int MinVersion, UtilityNetwork Utility)
    {
      if (Utility == null)
        return false;

      int version = Convert.ToInt32(Utility.GetDefinition().GetSchemaVersion());
      return (version >= MinVersion);
    }

    /// <summary>
    /// Empty all list of selected objects
    /// </summary>
    internal static void CleanSelections()
    {
      if (GlobalSelectedContainerIDs == null)
        GlobalSelectedContainerIDs = [];
      else
        GlobalSelectedContainerIDs.Clear();

      if (GlobalSelectedJunctionIDs == null)
        GlobalSelectedJunctionIDs = [];
      else
        GlobalSelectedJunctionIDs.Clear();
    }

    /// <summary>
    /// Get the selection in the map
    /// </summary>
    /// <param name="ReferenceMap">Map</param>
    /// <remarks>if the ReferenceMap is null, use the active Map</remarks>
    internal static void GetSelectionInMap(Map ReferenceMap = null)
    {
      if (ReferenceMap == null && (MapView.Active == null || MapView.Active.Map == null))
        return;

      Map map = ReferenceMap ?? MapView.Active.Map;

      GlobalSelectedJunctionIDs = [];
      GlobalSelectedContainerIDs = [];

      GlobalMapSelection = map.GetSelection();
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
      }
    }

    /// <summary>
    /// Show the exception in a Framework MessageBox
    /// </summary>
    /// <param name="exception">Exception</param>
    internal static void ShowException(Exception exception)
    {
      string s = ExceptionFormat(exception);

#if (DEBUG)
      s += Environment.NewLine + Environment.NewLine + exception.StackTrace + Environment.NewLine + Environment.NewLine;
#endif

      System.Diagnostics.Debug.WriteLine(s);
      Dialogs.MessageBox.Show(s);
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
    /// Get Diagram Layer From Map
    /// </summary>
    /// <param name="map">Map</param>
    /// <returns>DiagramLayer</returns>
    internal static DiagramLayer GetDiagramLayerFromMap(Map map)
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

    /// <summary>
    /// Get Network Diagram from Map
    /// </summary>
    /// <param name="map">Map</param>
    /// <returns>NetworkDiagram</returns>
    internal static NetworkDiagram GetNetworkDiagram(Map map)
    {
      DiagramLayer diagLayer = GetDiagramLayerFromMap(map);

      try
      {
        return diagLayer.GetNetworkDiagram();
      }
      catch
      {
        return null;
      }
    }

    /// <summary>
    /// Get Internal Source Name From Real Name
    /// </summary>
    /// <param name="RealSourceName">Real Source Name</param>
    /// <returns>Internal Source Name</returns>
    internal static string GetInternalSourceNameFromRealName(string RealSourceName)
    {
      string sourceName = _internalExternalSourceName.FirstOrDefault(a => a.Item2 == RealSourceName)?.Item1;
      if (string.IsNullOrEmpty(sourceName))
      {
        sourceName = RealSourceName;

        int pos = sourceName.LastIndexOf('.');
        if (pos > 0)
          sourceName = sourceName[(pos + 1)..];

        _internalExternalSourceName.Add(Tuple.Create(sourceName, RealSourceName));
      }

      return sourceName;
    }

    /// <summary>
    /// Get the utility network from a map
    /// </summary>
    /// <param name="MapToSearch">Map where the utility network can be found</param>
    /// <returns>UtilityNetwork</returns>
    internal static UtilityNetwork GetUtilityNetworkFromMap(Map MapToSearch = null)
    {
      MapToSearch ??= MapView.Active.Map;
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

    /// <summary>
    /// Retrieve the database from the activez Map
    /// </summary>
    /// <returns>Active database</returns>
    private static Geodatabase GetGeodatabaseFromActiveMap()
    {
      UtilityNetwork un = GlobalUtilityNetwork ?? GetUtilityNetworkFromMap();
      if (un != null)
        return un.GetDatastore() as Geodatabase;

      return null;
    }

    /// <summary>
    /// Open a dataset
    /// </summary>
    /// <typeparam name="T">Type of dataset</typeparam>
    /// <param name="Name">Dataset name</param>
    /// <returns>A dataset of T type</returns>
    internal static T OpenDataset<T>(string Name) where T : Dataset
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
            if (aliasName.Replace(" ", "").Equals(Name, StringComparison.OrdinalIgnoreCase))
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
              if (aliasName.Replace(" ", "").Equals(Name, StringComparison.OrdinalIgnoreCase))
              {
                return GlobalGeodatabase.OpenDataset<T>(fcDef.GetName());
              }
            }
            else
            {
              //Weird case, where the alias name is empty (Error Tables, Dirty Areas)
              string fullName = fcDef.GetName();
              if (fullName.Replace("_", "").Contains(Name)) //the Point errors, line errors, polygon errors and dirty areas come back as Dirty_Areas (want to compare to DirtyAreas). 
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
            if (aliasName.Replace(" ", "").Equals(Name, StringComparison.OrdinalIgnoreCase))
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
        return GlobalGeodatabase.OpenDataset<T>(Name);
      }
      return null;
    }

    /// <summary>
    /// Create a custom list of Diagram Element with the Info and another fields
    /// </summary>
    internal static void CreateCustomGraph()
    {
      customGraph = new();
      if (GlobalDiagram == null)
      {
        IsDiagramUsable(7, -1, true);
      }

      IList<DiagramElementInfo> diagramElementInfos = GlobalDiagram.GetDiagramElementInfo(new DiagramElementFilter() { BrowseJunctions = true, BrowseEdges = true, BrowseContainers = true });

      foreach (var item in GlobalDiagramJunctionElements)
      {
        customGraph.Add(new(Element: item, Info: diagramElementInfos.FirstOrDefault(a => a.ElementID == item.ID)?.Info));
      }

      foreach (var item in GlobalDiagramEdgeElements)
      {
        customGraph.Add(new(Element: item, Info: diagramElementInfos.FirstOrDefault(a => a.ElementID == item.ID)?.Info));
      }

      foreach (var item in GlobalDiagramContainerElements)
      {
        customGraph.Add(new(Element: item, Info: diagramElementInfos.FirstOrDefault(a => a.ElementID == item.ID)?.Info));
      }

      GlobalNetworkSources ??= GlobalUtilityNetwork.GetDefinition().GetNetworkSources();

      List<int> listOfLineId = [];

      foreach (var networkSource in GlobalNetworkSources.Where(a => a.UsageType == SourceUsageType.Line || a.UsageType == SourceUsageType.StructureLine || a.UsageType == SourceUsageType.Association))
        listOfLineId.Add(networkSource.ID);


      List<string> searchFieldNames =
      [
        "GlobalId",
        "DiagramElement::SourceID",
        "AssociationStatus",
        "AssetGroup",
        "AssetType"
      ];

      DiagramElementsAttributes attributes = GlobalDiagram.GetFeatureAttributes(filter: new DiagramElementFilter() { BrowseContainers = true, BrowseEdges = true, BrowseJunctions = true }, attributeNames: [.. searchFieldNames], useCodedValueNames: true, addAggregatedElementValues: true);

      int associatedObjectGuidIndex = 0;
      int sourceIDIndex = 1;
      int associationStatusIndex = 2;
      int assetGroupIndex = 3;
      int assetTypeIndex = 4;

      List<CustomAggregation> aggregations = [];
      foreach (var attribute in attributes.AttributeValuesPerElement)
      {
        int _deid = attribute.ElementID;
        CustomElement customElement = customGraph.GetElementByDEID(_deid);

        if (attribute.Values != null)
        {
          customElement.AssociationStatus = attribute.Values[associationStatusIndex] == null ? string.Empty : attribute.Values[associationStatusIndex].ToString();
          customElement.AssetGroup = attribute.Values[assetGroupIndex] == null ? string.Empty : attribute.Values[assetGroupIndex].ToString();
          customElement.AssetType = attribute.Values[assetTypeIndex] == null ? string.Empty : attribute.Values[assetTypeIndex].ToString();
        }

        if (attribute.AggregatedElementsValues != null && attribute.AggregatedElementsValues.Any())
        {
          for (int i = 0; i < attribute.AggregatedElementsValues.Count; i++)
          {
            Guid _associatedObjectGuid = attribute.AggregatedElementsValues[i][associatedObjectGuidIndex] == null ? Guid.Empty : Guid.Parse(attribute.AggregatedElementsValues[i][associatedObjectGuidIndex].ToString());
            string _associationStatus = attribute.AggregatedElementsValues[i][associationStatusIndex] == null ? string.Empty : attribute.AggregatedElementsValues[i][associationStatusIndex].ToString();
            string _assetGroup = attribute.AggregatedElementsValues[i][assetGroupIndex] == null ? string.Empty : attribute.AggregatedElementsValues[i][assetGroupIndex].ToString();
            int sourceID = attribute.AggregatedElementsValues[i][sourceIDIndex] == null ? -1 : Convert.ToInt32(attribute.AggregatedElementsValues[i][sourceIDIndex]);
            string assetType = attribute.AggregatedElementsValues[i][assetTypeIndex] == null ? string.Empty : attribute.AggregatedElementsValues[i][assetTypeIndex].ToString();
            
            CustomAggregation customAggregation = new(Deid: _deid, GlobalId: _associatedObjectGuid, SourceID: sourceID)
            {
              AssociationStatus = _associationStatus,
              AssetGroup = _assetGroup,
              AssetType = assetType
            };

            if (listOfLineId.Contains(sourceID))
              customAggregation.CustomType = CustomAggregation.AggregationTypeEnum.Edge;
            else if (_associationStatus.Contains("Container", StringComparison.OrdinalIgnoreCase))
              customAggregation.CustomType = CustomAggregation.AggregationTypeEnum.Container;
            else
              customAggregation.CustomType = CustomAggregation.AggregationTypeEnum.Junction;

            customElement.Aggregations.Add(customAggregation);
          }
        }
      }
    }

    /// <summary>
    /// Write the nfo field in database
    /// </summary>
    /// <param name="SearchSource">Source Id of the selected source</param>
    /// <param name="GroupName">Asset Group Name</param>
    /// <param name="TypeName">Asset Type Name</param>
    internal static void FillInfoFields(string SearchSource, string GroupName, string TypeName)
    {
      /* Info field format 
      - Count aggregated objects
      - Count aggregated objects for a specified FeatureClass, AssetGroup, AssetType, if an information is missing, it counts what is possible

      Format: INFO#1:<NbAggregations>|INFO#2:<Count> <SpecifiedAssetType>
      */

      string searchingAsset = string.IsNullOrEmpty(TypeName) ? string.IsNullOrEmpty(GroupName) ? SearchSource : GroupName : TypeName;
      foreach (CustomElement element in customGraph.Elements)
      {
        if (string.IsNullOrEmpty(searchingAsset))
          element.Info = $"{element.NbAggregations}|";
        else
        {
          int searchSourceID = -1;
          if (SearchSource == cAssociation)
            searchSourceID = GlobalNetworkSources.FirstOrDefault(a => a.UsageType == SourceUsageType.Association).ID;
          else
            searchSourceID = GlobalNetworkSources.FirstOrDefault(a => a.Name.Contains(SearchSource)).ID;

          int totalSpecified = element.CountSpecificAssetGroupAssetType(SearchSource: searchSourceID, AssetGroupName: GroupName, AssetTypeName: TypeName);

          element.Info = $"{element.NbAggregations}|{totalSpecified} {searchingAsset}";
        }
      }

      var ChangedDiagramInfos = customGraph.GetChangedDiagramElementInfo(out DiagramElementFilter filter);
      try
      {
        GlobalDiagram.SetDiagramElementInfo(filter, ChangedDiagramInfos);
      }
      catch (Exception ex)
      {
        string message = ExceptionFormat(ex);
        if (message.Contains("0x80045317")) // Server Timeout
        {
          const int cMaxInfoToUpdateAtTheSameTime = 10000;
          List<DiagramElementInfo> infoToSave = [];
          foreach (DiagramElementInfo diagramElementInfo in ChangedDiagramInfos)
          {
            infoToSave.Add(diagramElementInfo);
            if (infoToSave.Count >= cMaxInfoToUpdateAtTheSameTime)
            {
              GlobalDiagram.SetDiagramElementInfo(filter, infoToSave);
              infoToSave = [];
            }
          }
          if (infoToSave.Count > 0)
          {
            GlobalDiagram.SetDiagramElementInfo(filter, infoToSave);
          }
        }
        else
          throw;
      }
    }
  }
}
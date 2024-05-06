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
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Desktop.Mapping.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static BringUpSubnetworkNamesOnDiagramEdges.CommonTools;
using Dialogs = ArcGIS.Desktop.Framework.Dialogs;

namespace BringUpSubnetworkNamesOnDiagramEdges
{  /// <summary>
   /// This add-in demonstrates how to retrieve attributes on network elements associated with diagram features whether these network elements are aggregated in the diagram.It also exemplifies how to fill the new Info field added with utility network version 7 using the retrieved attributes.
   /// In particular, how to verify that all the aggregated network elements belong to a same subnetwork and save the name of the related subnetwork in the Info field.
   /// **Note:** This code sample is generic and can be used for any utility network dataset once upgraded to version 7. However, for the rendering of the diagram edges based on the Info field values and, in particular, to display them in a specific color regarding their subnetwork name, you need to set up a custom diagram layer definition on the diagram template.  
   /// **Comment:** This add-in sample code works with ArcGIS Pro 3.3 and utility network version 7. For prior ArcGIS Pro and utility network versions, we provide another add-in code sample command called ToggleSwitches that allows to get quite similar results. The difference between the two add-ins mainly concerns the management of the diagram edge colors.
   /// With the ToggleSwitches add-in, when users click the Color Subnetwork command, the subnetwork name for each diagram edge in the active diagram is retrieved using a quite complex code.This custom code exports the content of the diagram, parses the resulting JSON structures, and query the associated network elements to determine the subnetwork name related to each diagram edge.Then, each edge is added to a list regarding its subnetwork name and the color rendering per subnetwork is fully managed by code for each diagram edge list. This code is also the one used when applying the Toggle Switches command, once the status of the selected switch(es) has just toggled and the related subnetwork(s) updated to get the subnetwork name changes immediately reflected on the diagram edge colors.
   /// With the BringUpSubnetworkNameOnDiagramEdge add-in, the color of the diagram edges per subnetwork is no longer managed by code.It is directly set up on the diagram layer definition at the diagram template level and based on the subnetwork name stored in the new Info field for each diagram edge.This field is updated each time users click the Update Info add-in command.The custom code for this command takes advantage of the new SetDiagramElementInfo method added with ArcGIS Pro SDK for .NET at 3.3 to fill up the Info field values.The subnetwork name stored in this field during the process is also retrieved in an easier way starting at 3.3 thanks to the new GetFeatureAttributes SDK method that directly returns the desired attribute values for the network elements represented in your diagram whether they are aggregated.The code related to the Toggle Switch command that is provided with this add-in is the same as in the previous add-in for its first part; that is, the part of the code that toggles the switch position, validate the network topology and update the related subnetworks.Then, for the 2nd part of the code, it simply chains the new 3.3 code developed for the Update Info command to store the updated subnetwork name in the Info field for each diagram edge. When the diagram map refreshes at the end of the Toggle Switches process, the template layer definition re-applies and the diagram edge color automatically reflects any subnetwork name changes in the Info field.
   /// </summary>
   /// <remarks>
   /// 1. In Visual Studio click the Build menu. Then select Build Solution. 
   /// 2. Click Start button to open ArcGIS Pro.  ArcGIS Pro will open. 
   /// 3. Open C:\Data\NetworkDiagrams\FillsUpDiagramFeatureInfoField_UNv7\FillsUpDiagramFeatureInfoField_UNv7.aprx 
   /// 4. Click on the Map tab on the ribbon. Then, in the Navigate group, expand Bookmarks and click North 7 SbNtwk Ctrls. 
   /// ![UI](Screenshots/Screenshot1.png)
   /// 5. The network map centers on the North Substation in which you can see 7 distribution subnetwork controllers that display with a gray symbol at the end of the blue Medium Voltage Conductor lines. Hold down the Shift keyboard key and use the Select Feature tool to select each of these 7 distribution subnetwork controllers. 
   /// ![UI](Screenshots/Screenshot2.png)
   /// 6. In the Navigate group, expand Bookmarks and click South 5 SbNtwk Ctrls. 
   /// ![UI](Screenshots/Screenshot3.png)
   /// 7. Still with the Shift keyboard key held down, click each of the 5 distribution subnetwork controllers in the south substation to add them to the current selection. You end with 12 Distribution subnetwork controllers selected in the network map: 
   /// ![UI](Screenshots/Screenshot4.png)
   /// 8. Click on the Utility Network tab in the ribbon. Then, in the Diagram group, click the New down arrow and pick up SwitchingFromDistributionSubnetworkCtrl in the list. 
   /// ![UI](Screenshots/Screenshot5.png)
   /// **Note:** You can have a look to the model builder called SwitchingFromDistributionSubnetworkCtrl in the CutomDiagramTemplates toolbox installed with this project if you want to know more about the network diagram rules configured for this template.
   /// ![UI](Screenshots/Screenshot6.png)
   /// A new diagram map opens representing a simplification of the 12 subnetworks associated with the 12 subnetwork controllers selected in the network map as input.
   /// This diagram mainly focuses on critical devices, such as switches.  The other portions of the subnetworks are aggregated under these remaining switches or under reduction edges that connect these switches.
   /// 10. Click on the Add-in tab on the ribbon and in the Bring Up Info group, click Update Info.
   /// ![UI](Screenshots/Screenshot7.png)
   /// The active diagram refreshes and shows different colors per subnetwork on the diagram edges. 
   /// ![UI](Screenshots/Screenshot8.png)
   /// 11. In the Contents pane, expand the Temporary diagram layer and scroll down until you see the Reduction Edges sublayer. 
   /// 12. Right click the Reduction Edges sublayer and click Attribute Table. Info, the fourth field in the open table, is the field that the Update Info command has just filled up. It stores text values that group two different information separated using a backslash for each reduction edge. The 1st information is the subnetwork name related to the reduction edge. The 2nd information is the count of network elements that each reduction edge aggregates. 
   /// ![UI](Screenshots/Screenshot9.png)
   /// If you have a look to the way the symbology is set up on the reduction edges, you can see that there is an Arcade script set up to extract the 1st information value from this Info field; that is, the subnetwork name, and uses it to colorize the diagram edges in different colors. 
   /// ![UI](Screenshots/Screenshot10.png)
   /// In the same way, if you have a look to the labeling properties currently set for the reduction edges, you can see that there is another Arcade script configured to extract the 2nd information value: that is, the count of the network elements that each reduction edge aggregates. This count displays in gray at the middle of each reduction edge. 
   /// ![UI](Screenshots/Screenshot11.png)
   /// 13. Zoom in on the diagram area highlighted in dark blue like in the screenshot below: 
   /// ![UI](Screenshots/Screenshot12.png)
   /// 14. Select the Medium Voltage Switch which Asset ID is MV-SW-24 along the red RMT003 subnetwork like in the following screenshot; this is the one just above the switch labeled RMT001:RMT003 
   /// ![UI](Screenshots/Screenshot13.png)
   /// 15. Click on the Add-in tab on the ribbon and in the Bring Up Info group, click Toggle Switches.
   /// ![UI](Screenshots/Screenshot14.png)
   /// The process chains the following steps: the switch status is toggled (it moved from Closed to Open) 
   /// ![UI](Screenshots/Screenshot15.png)
   /// Then, the topology is validated and saved, the related subnetwork is updated, and the Info field values are updated to reflect subnetwork name changes. 
   /// ![UI](Screenshots/Screenshot15a.png)
   /// There are some diagram edges previously related to the red RMT003 subnetwork that become de-energized; they became out of any subnetwork and display as empty lines. If you use the Explore tool and click one of these diagram edges, the Pop-up dialog that opens shows Unknow before the backslash that delimits the 1st information stored in the Info field 
   /// ![UI](Screenshots/Screenshot16.png)
   /// 16. Select the medium voltage switch just below, the first of the blue RMT001 subnetwork that is currently open; this is, the one previously labeled as RMT001:RMT003. 
   /// ![UI](Screenshots/Screenshot17.png)
   /// 17. In the Bring Up Info group, click on the Toggle Switches tool. The same process executes. The network features previously de-energized are now fed by the subnetwork controller CB:Line Side/RMT001; the diagram edges which represent features that now belong to this RMT001 subnetwork are colorized in blue. 
   /// ![UI](Screenshots/Screenshot18.png)
   /// 18. Select the two medium voltage switches for which you've changed the status since the beginning of this workflow. 
   /// ![UI](Screenshots/Screenshot19.png)
   /// 19. In the Toggle Switches group, click on the Toggle Switches tool. The status of the two selected switches are toggled and the two related subnetworks are updated. The utility network is back to its initial state. 
   /// ![UI](Screenshots/Screenshot20.png)
   /// </remarks>
  internal class BringUpSubnetworkNamesOnDiagramEdgesModule : Module
  {
    private static BringUpSubnetworkNamesOnDiagramEdgesModule _this = null;
    private const string cDeviceStatusFieldName = "NormalOperatingStatus";
    private const string cTierName = "Electric Distribution";
    private const string cDomainNetworkName = "Electric";
    private const string cAssetGroupFieldValue = "37";
    private const string cSubnetworkName = "SubnetworkName";
    private const string cSupportedSubnetworkName = "SupportedSubnetworkName";
    private const int cOpen = 1;
    private const int cClose = 0;
    // SourceId:
    // 1 Association
    // 2 System Junction
    private const int cAssociationSourceId = 1;
    private const int cSystemJunctionSourceId = 2;
    internal const string cUnknown = "Unknown";

    internal static IReadOnlyList<NetworkSource> _sources = null;
    private static ManageCustomElement customGraph;


    /// <summary>
    /// Retrieve the singleton instance to this module here
    /// </summary>
    public static BringUpSubnetworkNamesOnDiagramEdgesModule Current => _this ??= (BringUpSubnetworkNamesOnDiagramEdgesModule)FrameworkApplication.FindModule("BringUpSubnetworkNamesOnDiagramEdges_Module");

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
    /// <summary>
    /// Initialize Active Map View Changed
    /// </summary>
    /// <returns>boolean</returns>
    protected override bool Initialize()
    {
      ActiveMapViewChangedEvent.Subscribe(OnActiveMapViewChanged);
      return base.Initialize();
    }

    /// <summary>
    /// Uninitialize Active Map View Changed
    /// </summary>
    protected override void Uninitialize()
    {
      ActiveMapViewChangedEvent.Unsubscribe(OnActiveMapViewChanged);
      base.Uninitialize();
    }

    /// <summary>
    /// Set Enclosure flag
    /// </summary>
    /// <param name="obj">ActiveMapViewChangedEventArgs</param>
    private void OnActiveMapViewChanged(ActiveMapViewChangedEventArgs obj)
    {
      if ((obj == null && obj.IncomingView == null && obj.IncomingView.Map == null) || (obj?.IncomingView?.Map == null))
        return;

      if (obj.IncomingView.Map.MapType != MapType.Map && obj.IncomingView.Map.MapType != MapType.NetworkDiagram)
        return;

      // This tool only works with Utility Network version 7 or higher
      QueuedTask.Run(() =>
      {
        UtilityNetwork un = GetUtilityNetworkFromActiveMap(obj.IncomingView.Map);
        if (un == null)
        {
          IsUNV7Flag = false;
          return;
        }

        IsUNV7Flag = IsUNV7(un);
      });
    }

    #endregion Overrides

    /// <summary>
    /// Get Selected Guid From Active Map
    /// </summary>
    /// <returns>Collection of GUID</returns>
    internal static List<Guid> GetSelectedGuidFromActiveMap()
    {
      List<Guid> listIds = [];

      Map map = MapView.Active.Map;

      Dictionary<MapMember, List<long>> selected = map.GetSelection().ToDictionary();

      foreach (var v in selected)
      {
        if (v.Key is StandaloneTable standalone)
        {
          using Table originTable = standalone.HasJoins ? standalone.GetTable().GetJoin().GetOriginTable() : standalone.GetTable();

          FieldDescription field = standalone.GetFieldDescriptions().FirstOrDefault(a => a.Type == FieldType.GlobalID);

          QueryFilter queryFilter = new() { ObjectIDs = v.Value, SubFields = field.Name };

          using RowCursor cursor = originTable.Search(queryFilter);
          while (cursor.MoveNext())
          {
            Row row = cursor.Current;
            listIds.Add(row.GetGlobalID());
          }
        }
        else if (v.Key is BasicFeatureLayer bfl)
        {
          using Table originTable = bfl.HasJoins ? bfl.GetTable().GetJoin().GetOriginTable() : bfl.GetTable();

          QueryFilter queryFilter = new() { ObjectIDs = v.Value, SubFields = originTable.GetDefinition().GetGlobalIDField() };

          using RowCursor cursor = originTable.Search(queryFilter);
          while (cursor.MoveNext())
          {
            Row row = cursor.Current;
            listIds.Add(row.GetGlobalID());
          }
        }
      }

      return listIds;
    }

    /// <summary>
    /// Show diagram in a new map
    /// </summary>
    /// <param name="Diagram">diagram to show</param>
    internal static async void ShowDiagram(NetworkDiagram Diagram)
    {
      // Create a diagram layer from a NetworkDiagram (myDiagram)
      DiagramLayer diagramLayer = await QueuedTask.Run<DiagramLayer>(() =>
      {

        // Creates the diagram map
        var newMap = MapFactory.Instance.CreateMap(Diagram.Name, ArcGIS.Core.CIM.MapType.NetworkDiagram, MapViewingMode.Map);
        if (newMap == null)
          return null;

        // Opens the diagram map
        var mapPane = ArcGIS.Desktop.Core.ProApp.Panes.CreateMapPaneAsync(newMap, MapViewingMode.Map);
        if (mapPane == null)
          return null;

        //Adds the diagram to the map
        return newMap.AddDiagramLayer(Diagram);
      });
    }

    /// <summary>
    /// Run the Toggle Switches process, it can be canceled
    /// </summary>
    /// <param name="cps">Cancelable Progressor Source to show the progression</param>
    /// <param name="mapExtent">Map Extent to zomm to the precedent extent</param>
    /// <returns>An error comment if needed, empty of no error</returns>
    internal static async Task<string> RunCancelableToggleSwitches(CancelableProgressorSource cps, Envelope mapExtent)
    {
      string status = "";
      GlobalIsRunning = true;
      await QueuedTask.Run(() =>
      {
        cps.Progressor.Max = 6;

        cps.Progressor.Value = 0;
        cps.Progressor.Status = (cps.Progressor.Value * 100 / cps.Progressor.Max) + @" % Completed";
        cps.Progressor.Message = "Step 1 – Toggle switch status";

        // Changes the selected switches attributes
        status = ToggleSwitchesExecute();
      }, cps.Progressor);

      await QueuedTask.Run(() =>
      {
        if (string.IsNullOrEmpty(status) && !cps.Progressor.CancellationToken.IsCancellationRequested)
        {
          cps.Progressor.Value += 1;
          cps.Progressor.Status = (cps.Progressor.Value * 100 / cps.Progressor.Max) + @" % Completed";
          cps.Progressor.Message = "Step 2 –  Validate and save edits";

          // ValidateNetworkTopology need an Edit Operation if there are some editions not saved
          Project.Current.SaveEditsAsync();
          GlobalUtilityNetwork.ValidateNetworkTopology();
          // ValidateNetworkTopology create an edit operation which must be saved 
          Project.Current.SaveEditsAsync();

          cps.Progressor.Value += 1;
          cps.Progressor.Status = (cps.Progressor.Value * 100 / cps.Progressor.Max) + @" % Completed";
          cps.Progressor.Message = "Step 3 – Update the dirty subnetwork(s)";

          UtilityNetworkDefinition unDef = GlobalUtilityNetwork.GetDefinition();
          Tier tier = unDef.GetDomainNetwork(cDomainNetworkName).GetTier(cTierName);

          SubnetworkManager subManager = GlobalUtilityNetwork.GetSubnetworkManager();

          // Updates the dirty subnetwork
          try
          {
            subManager.UpdateAllSubnetworks(tier, true);
          }
          catch (Exception ex)
          { status = ExceptionFormat(ex); }
        }
      }, cps.Progressor);

      await QueuedTask.Run(() =>
      {
        if (string.IsNullOrEmpty(status) && !cps.Progressor.CancellationToken.IsCancellationRequested)
        {
          cps.Progressor.Value += 1;
          cps.Progressor.Status = (cps.Progressor.Value * 100 / cps.Progressor.Max) + @" % Completed";
          cps.Progressor.Message = "Step 4 – Update Info Field per subnetwork";

          FillDiagramInfo();

          cps.Progressor.Value += 1;
          cps.Progressor.Status = (cps.Progressor.Value * 100 / cps.Progressor.Max) + @" % Completed";
          cps.Progressor.Message = "Step 5 – Redraw the Network Diagram";

          MapView.Active.Redraw(true);
          MapView.Active.ZoomTo(mapExtent);

          // Resets the map selection
          if (GlobalMapSelection != null)
          {
            MapView.Active.Map.ClearSelection();
            MapView.Active.Map.SetSelection(GlobalMapSelection);
          }
        }
      }, cps.Progressor);

      await QueuedTask.Run(() =>
      {
        CleanModule();
      });

      GlobalIsRunning = false;

      return status;
    }

    /// <summary>
    /// Internal function to toggle the switch positions
    /// </summary>
    /// <returns>An error comment if needed, empty of no error</returns>
    private static string ToggleSwitchesExecute()
    {
      bool canUpdate = false;

      // Loads the diagram elements and the current diagram selection
      if (!IsDiagramUsable(GetAllElements: false, GetSelection: true))
        return "It is not a diagram map";

      if (GlobalSelectedJunctionIDs.Count == 0)
        return "There is no junction selected";

      Dictionary<long, List<Guid>> guidBySourceId = [];
      // Retrieves the selected junction GlobalIds
      foreach (long oid in GlobalSelectedJunctionIDs)
      {
        DiagramJunctionElement junctionElement = GlobalDiagramJunctionElements.FirstOrDefault(a => a.ObjectID == oid);
        if (junctionElement != null)
        {

          if (!guidBySourceId.TryGetValue(junctionElement.AssociatedSourceID, out List<Guid> guidList))
          {
            guidList = [];
            guidBySourceId.Add(junctionElement.AssociatedSourceID, guidList);
          }

          if (!guidList.Contains(junctionElement.AssociatedGlobalID))
            guidList.Add(junctionElement.AssociatedGlobalID);
        }
      }

      IReadOnlyList<NetworkSource> theSources = GlobalUtilityNetwork.GetDefinition().GetNetworkSources();

      List<string> searchFields = [cDeviceStatusFieldName, "ObjectId", "AssetGroup", "GlobalID"];

      foreach (var v in guidBySourceId)
      {
        NetworkSource source = theSources.FirstOrDefault(a => a.ID == v.Key);
        if (source != null)
        {
          // Gets a cursor on the junction GUID list, and gets the qualified field names
          using RowCursor sel = GetRowCursorFromSourceNameAndGuidList(SourceName: source.Name.Replace(" ", ""), SearchGuid: v.Value, ListSearchFields: searchFields, WhereField: "GlobalId", FieldsName: out List<Tuple<string, string>> FieldsName);
          int deviceStatusIndex = -1;
          int assetGroupIndex = -1;
          // Retrieves the indexed fields
          foreach (Tuple<string, string> findTuple in FieldsName)
          {
            if (findTuple.Item1 == cDeviceStatusFieldName)
              deviceStatusIndex = sel.FindField(findTuple.Item2);
            else if (findTuple.Item1 == "AssetGroup")
              assetGroupIndex = sel.FindField(findTuple.Item2);
          }

          if (deviceStatusIndex >= 0) // Only run when there is a device status field name on the features that are going to be edited
          {
            var modifyStringsOperation = new EditOperation
            {
              Name = $"Modify string field '{cDeviceStatusFieldName}' in source {source.Name}."
            };

            List<long> oidSet = [];
            while (sel.MoveNext())
            {
              string AssetGroupValue = sel.Current[assetGroupIndex].ToString();
              // Checks that the input feature corresponds to the expected Asset Group 
              if (!string.IsNullOrEmpty(AssetGroupValue) && AssetGroupValue == cAssetGroupFieldValue)
              {
                int deviceStatus = Convert.ToInt16(sel.Current[deviceStatusIndex]?.ToString());
                Guid globalIdValue = sel.Current.GetGlobalID();
                long oid = sel.Current.GetObjectID();

                // Sets up a new dictionary with the fields to modify
                var modifiedAttributes = new Dictionary<string, object>
                {
                    // Adds the name of the string field and the new attribute value to the dictionary
                    {cDeviceStatusFieldName, deviceStatus == cClose ? cOpen : cClose}
                };

                // sPut the modify operation on the editor stack
                modifyStringsOperation.Modify(sel.Current, modifiedAttributes);
                oidSet.Add(oid);
              }
            }

            if (oidSet.Count > 0)
            {  // Runs the ModifyStringsOperation to apply the changes
              modifyStringsOperation.Execute();
              canUpdate = true;
            }
            else
              modifyStringsOperation.Abort();
          }
        }
      }

      return canUpdate ? "" : $"This command only applies to medium voltage switches. Please make sure there is at least one selected medium voltage switch in the active diagram before its execution.";
    }

    /// <summary>
    /// Fill the custom graph Info field
    /// </summary>
    internal static void FillDiagramInfo()
    {
      _sources = GlobalUtilityNetwork.GetDefinition().GetNetworkSources();

      GlobalDiagram.ClearDiagramElementInfo();
      CreateCustomGraph();

      WriteToFieldInfo(true, new() { BrowseContainers = true, BrowseEdges = true, BrowseJunctions = true });
    }

    /// <summary>
    /// Fill the diagram Info field
    /// </summary>
    /// <param name="AddAggregationCount"></param>
    /// <param name="Filter"></param>
    internal static void WriteToFieldInfo(bool AddAggregationCount, DiagramElementFilter Filter)
    {
      foreach (CustomElement element in customGraph.GetFilteredElements(Filter))
      {
        string subnetName = string.IsNullOrEmpty(element.SubnetworkName) ? string.IsNullOrEmpty(element.AggregatedSubnetworkName) ? cUnknown : element.AggregatedSubnetworkName : element.SubnetworkName;

        if (AddAggregationCount)
        {
          int totalSystemJunction = element.AggregationsBySourceId([cSystemJunctionSourceId]).Count;
          int total = element.NbAggregations - totalSystemJunction;

          if (total > 0)
            element.Info = $"{subnetName}\\{total}";
          else
            element.Info = subnetName;
        }
        else
        {
          element.Info = subnetName;
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

    /// <summary>
    /// Create Custom Graph to manage the different element
    /// </summary>
    internal static void CreateCustomGraph()
    {
      Dictionary<int, bool> useSupported = [];

      foreach (NetworkSource source in _sources)
      {
        useSupported.Add(source.ID, source.UsageType == SourceUsageType.Assembly);
      }

      customGraph = new();

      DiagramElementQueryResult deqr = GlobalDiagram.QueryDiagramElements(new DiagramElementQueryByElementTypes()
      {
        QueryDiagramContainerElement = true,
        QueryDiagramEdgeElement = true,
        QueryDiagramJunctionElement = true
      });

      IList<DiagramElementInfo> diagramElementInfos = GlobalDiagram.GetDiagramElementInfo(new DiagramElementFilter() { BrowseJunctions = true, BrowseEdges = true, BrowseContainers = true });

      foreach (var item in deqr.DiagramJunctionElements)
      {
        customGraph.Add(new(Element: item, Info: diagramElementInfos.FirstOrDefault(a => a.ElementID == item.ID)?.Info));
      }

      foreach (var item in deqr.DiagramEdgeElements)
      {
        customGraph.Add(new(Element: item, Info: diagramElementInfos.FirstOrDefault(a => a.ElementID == item.ID)?.Info));
      }

      foreach (var item in deqr.DiagramContainerElements)
      {
        customGraph.Add(new(Element: item, Info: diagramElementInfos.FirstOrDefault(a => a.ElementID == item.ID)?.Info));
      }

      string[] searchFieldNames =
      [
        "GlobalId",
        cSubnetworkName,
        cSupportedSubnetworkName,
        "DiagramElement::SourceID"
      ];

      DiagramElementsAttributes attributes = GlobalDiagram.GetFeatureAttributes(filter: new DiagramElementFilter() { BrowseContainers = true, BrowseEdges = true, BrowseJunctions = true }, attributeNames: searchFieldNames, useCodedValueNames: false, addAggregatedElementValues: true);

      int associatedObjectGuidIndex = 0;
      int subnetworkNameFeatureIndex = 1;
      int subnetworkNameAssemblyIndex = 2;
      int sourceIDIndex = 3;

      foreach (var attribute in attributes.AttributeValuesPerElement)
      {
        int _deid = attribute.ElementID;
        CustomElement customElement = customGraph.GetElementByDEID(_deid);

        if (attribute.Values != null)
        {
          int sourceID = attribute.Values[sourceIDIndex] == null ? -1 : Convert.ToInt32(attribute.Values[sourceIDIndex]);

          customElement.SubnetworkName = string.Empty;
          if (useSupported.TryGetValue(sourceID, out bool useIt))
          {
            if (useIt)
            {
              if (attribute.Values[subnetworkNameAssemblyIndex] != null)
                customElement.SubnetworkName = attribute.Values[subnetworkNameAssemblyIndex].ToString();
            }
            else if (attribute.Values[subnetworkNameFeatureIndex] != null)
              customElement.SubnetworkName = attribute.Values[subnetworkNameFeatureIndex].ToString();
          }
        }

        if (attribute.AggregatedElementsValues != null && attribute.AggregatedElementsValues.Any())
        {
          for (int i = 0; i < attribute.AggregatedElementsValues.Count; i++)
          {
            Guid _associatedObjectGuid = attribute.AggregatedElementsValues[i][associatedObjectGuidIndex] == null ? Guid.Empty : Guid.Parse(attribute.AggregatedElementsValues[i][associatedObjectGuidIndex].ToString());
            int sourceID = attribute.AggregatedElementsValues[i][sourceIDIndex] == null ? -1 : Convert.ToInt32(attribute.AggregatedElementsValues[i][sourceIDIndex]);

            string _subnetworkName = string.Empty;
            if (useSupported.TryGetValue(sourceID, out bool useIt))
            {
              if (useIt)
              {
                if (attribute.AggregatedElementsValues[i][subnetworkNameAssemblyIndex] != null)
                  _subnetworkName = attribute.AggregatedElementsValues[i][subnetworkNameAssemblyIndex].ToString();
              }
              else if (attribute.AggregatedElementsValues[i][subnetworkNameFeatureIndex] != null)
                _subnetworkName = attribute.AggregatedElementsValues[i][subnetworkNameFeatureIndex].ToString();
            }

            CustomAggregation aggregation = new(Deid: _deid, GlobalId: _associatedObjectGuid, SourceID: sourceID)
            {
              SubnetworkName = _subnetworkName
            };
            customElement.Aggregations.Add(aggregation);
          }
        }
      }

      foreach (CustomElement element in customGraph.GetElementsBySourcesID([cAssociationSourceId]))
      {
        DiagramEdgeElement diagramEdgeElement = element.DiagramElement as DiagramEdgeElement;
        Tuple<CustomElement, CustomElement> extremities = customGraph.GetExtremities(diagramEdgeElement.FromID, diagramEdgeElement.ToID);
        string fromSubnetwork = extremities.Item1.AggregatedSubnetworkName;
        string toSubnetwork = extremities.Item2.AggregatedSubnetworkName;

        if (fromSubnetwork.Equals(cUnknown, StringComparison.OrdinalIgnoreCase))
          fromSubnetwork = "";
        if (toSubnetwork.Equals(cUnknown, StringComparison.OrdinalIgnoreCase))
          toSubnetwork = "";

        if (string.IsNullOrEmpty(fromSubnetwork) && string.IsNullOrEmpty(toSubnetwork))
        {
          element.SubnetworkName = cUnknown;
        }
        else if (string.IsNullOrEmpty(fromSubnetwork))
        {
          element.SubnetworkName = toSubnetwork;
        }
        else if (string.IsNullOrEmpty(toSubnetwork))
        {
          element.SubnetworkName = fromSubnetwork;
        }
        else if (fromSubnetwork.Equals(toSubnetwork, StringComparison.OrdinalIgnoreCase))
        {
          element.SubnetworkName = fromSubnetwork;
        }
        else if (fromSubnetwork.Contains(toSubnetwork, StringComparison.OrdinalIgnoreCase))
        {
          element.SubnetworkName = fromSubnetwork;
        }
        else if (toSubnetwork.Contains(fromSubnetwork, StringComparison.OrdinalIgnoreCase))
        {
          element.SubnetworkName = toSubnetwork;
        }
        else
        {
          element.SubnetworkName = $"{fromSubnetwork}::{toSubnetwork}";
        }
      }
    }

    internal static bool _isDiagramGenerated = false;

    internal static string status = "";
    internal static string _ActiveTemplate = "";

    /// <summary>
    /// Run create diagram with an information progressor
    /// </summary>
    /// <param name="cps">Progressor information</param>
    /// <returns>Error string if failed else empty string</returns>
    public static void RunNewDiagram(string SelectedTemplate)
    {
      if (string.IsNullOrEmpty(SelectedTemplate))
        return;

      _ActiveTemplate = SelectedTemplate;
      status = ""; if (_isDiagramGenerated || GlobalIsRunning || string.IsNullOrEmpty(_ActiveTemplate))
        return;

      _isDiagramGenerated = true;
      GlobalIsRunning = true;
      try
      {
        using var pd = new ProgressDialog("New Diagram", 3, false);
        ProgressorSource cps = new(pd);
        QueuedTask.Run(() =>
        {
          cps.Progressor.Max = 3;

          cps.Progressor.Value = 0;
          cps.Progressor.Status = (cps.Progressor.Value * 100 / cps.Progressor.Max) + @" % Completed";
          cps.Progressor.Message = "Step 1 – Generate Diagram";

          if (GlobalDiagramManager == null)
          {
            GlobalUtilityNetwork = GetUtilityNetworkFromActiveMap();
            if (GlobalUtilityNetwork == null)
            {
              _isDiagramGenerated = false;
              GlobalIsRunning = false;
              return;
            }

            GlobalDiagramManager = GlobalUtilityNetwork.GetDiagramManager();
            if (GlobalDiagramManager == null)
            {
              _isDiagramGenerated = false;
              GlobalIsRunning = false;
              return;
            }
          }

          var m_Template = GlobalDiagramManager.GetDiagramTemplate(SelectedTemplate);
          if (m_Template == null)
          {
            _isDiagramGenerated = false;
            GlobalIsRunning = false;
            return;
          }

          var listIds = GetSelectedGuidFromActiveMap();
          if (listIds.Count == 0)
          {
            _isDiagramGenerated = false;
            GlobalIsRunning = false;
            return;
          }

          try
          {
            GlobalDiagram = GlobalDiagramManager.CreateNetworkDiagram(diagramTemplate: m_Template, globalIDs: listIds);
          }
          catch (Exception ex)
          {
            status = ExceptionFormat(ex);
          }

          if (string.IsNullOrEmpty(status))
          {
            cps.Progressor.Value += 1;
            cps.Progressor.Status = (cps.Progressor.Value * 100 / cps.Progressor.Max) + @" % Completed";
            cps.Progressor.Message = "Step 2 –  Fill the diagram feature Info field";

            _sources = GlobalUtilityNetwork.GetDefinition().GetNetworkSources();
            CreateCustomGraph();

            WriteToFieldInfo(true, new() { BrowseContainers = true, BrowseEdges = true, BrowseJunctions = true });
          }

          if (string.IsNullOrEmpty(status))
          {
            cps.Progressor.Value += 1;
            cps.Progressor.Status = (cps.Progressor.Value * 100 / cps.Progressor.Max) + @" % Completed";
            cps.Progressor.Message = "Step 3 –  Open the diagram";

            try
            {
              ShowDiagram(GlobalDiagram);
            }
            catch (Exception ex)
            {
              status = ExceptionFormat(ex);
            }
          }

          if (!string.IsNullOrEmpty(status))
            Dialogs.MessageBox.Show(status);

          _isDiagramGenerated = false;
          GlobalIsRunning = false;

        }, cps.Progressor);
      }
      catch (Exception ex)
      {
        status = ExceptionFormat(ex);
      }

    }


  }
}

/*

   Copyright 2022 Esri

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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static ToggleSwitches.CommonTools;

namespace ToggleSwitches
{
  /// <summary>
  /// This add-in demonstrates toggling switches in an electric distribution network and the visual network diagram feedback.  
  /// Attributes cannot be manually edited from network diagrams; nor the attributes stored in the diagram feature classes, nor those coming from the join with the associated network source classes.
  /// However, any editable attributes on the joined network feature class can be edited by code. In this add-in sample, you will learn about how to edit the Status attribute available on Switch devices.
  /// This code sample is tied to the sample network dataset referenced in the workflow steps detailed below. But, it can be easily adapted for any utility network dataset.
  ///
  /// Community Sample data (see under the "Resources" section for downloading sample data) has a CommunitySampleData-NetworkDiagrams zip file that contains a project that can be used with this sample.
  /// </summary>
  /// <remarks>
  /// 1. In Visual Studio click the Build menu.  Then select Build Solution.  
  /// 1. Click Start button to open ArcGIS Pro.  
  /// 1. ArcGIS Pro will open.  
  /// 1. Open C:\Data\NetworkDiagrams\SDKSampleNetwork\SDKSampleNetwork.aprx
  /// 1. Click on the Map tab on the ribbon. Then, in the Navigate group, expand Bookmarks and click Full Extent.
  /// 1. Click on the Utility Network tab in the ribbon. Then, in the Diagram group, click Find Diagrams.
  /// 1. In the Find Diagrams pane, search for the diagram stored as ToggleSwitches_Test and double click it so it opens
  /// 1. Click on the Add-in tab on the ribbon  
  /// 1. In the Toggle Switches group, click on the Color Subnetwork tool so the diagram edges are colorized according the subnetwork to which they belong.
  /// 1. Select the medium voltage switch for which the Asset Identifier is 11 like in the following screenshot; this is, the one just above the switch labelled RMT001:RMT003:
  ///     ![UI](Screenshots/ToogleSwitches1.png)
  ///     Its Device Status is actually Closed.
  /// 1. In the Toggle Switches group, click on the Toggle Switches tool.
  /// The process chains the following steps: the switch status is toggled (it moved from Closed to Open), the topology is validated and saved, the related subnetwork is updated, the diagram edge colors are updated to reflect the color of the subnetwork to which they belong.
  /// There are some diagram edges previously related to the blue subnetwork that become de-energized; they became out of any subnetwork and display as dash red lines.
  ///     ![UI](Screenshots/ToogleSwitches2.png)		
  /// 1. Select the medium voltage switch just below for which the Asset Identifier is 30 like in the following screenshot; this is the one previously labelled as RMT001:RMT003:
  ///     ![UI](Screenshots/ToogleSwitches3.png)
  ///     Its Device Status is actually Open.	
  /// 1. In the Toggle Switches group, click on the Toggle Switches tool.
  /// The same process executes. The network features previously de-energized are now fed by the subnetwork controller CB:Line Side/RMT001; the diagram edges which represent features that now belong to this RMT001 subnetwork are colorized in green.
  ///     ![UI](Screenshots/ToogleSwitches4.png)
  /// 1. Select the two medium voltage switches for which you've changed the status since the beginning of this workflow.
  /// 1. In the Toggle Switches group, click on the Toggle Switches tool.
  /// The status of the two selected switches are toggled and the two related subnetworks are updated. The utility network is back to its initial state.
  /// </remarks>
  internal class ToggleSwitchesModule : Module
  {
    private static ToggleSwitchesModule _this = null;
    private const string cDeviceStatusFieldName = "CurrentDeviceStatus";
    private const string cTierName = "Electric Distribution";
    private const string cDomainNetworkName = "Electric";
    private const string cAssetGroupFieldValue = "37";

    /// <summary>
    /// Retrieve the singleton instance to this module here
    /// </summary>
    public static ToggleSwitchesModule Current
    {
      get
      {
        return _this ?? (_this = (ToggleSwitchesModule)FrameworkApplication.FindModule("ToggleSwitches_Module"));
      }
    }

    /// <summary>
    /// Run the toggle switches process, it can be cancel
    /// </summary>
    /// <param name="cps">Cancelable Progressor Source to show the progression</param>
    /// <param name="mapExtent">Map Extent to zomm to the precedent extent</param>
    /// <returns>An error comment if needed, empty of no error</returns>
    internal static async Task<string> RunCancelableToggleSwwitches(CancelableProgressorSource cps, Envelope mapExtent)
    {
      string status = "";
      await QueuedTask.Run(() =>
      {
        cps.Progressor.Max = 6;

        cps.Progressor.Value += 1;
        cps.Progressor.Status = (cps.Progressor.Value * 100 / cps.Progressor.Max) + @" % Completed";
        cps.Progressor.Message = "Step 1 – Toggle switch status";

        // change selected switches attributes
        status = ToggleSwitchesExecute();
      }, cps.Progressor);

      await QueuedTask.Run(() =>
      {
        if (String.IsNullOrEmpty(status) && !cps.Progressor.CancellationToken.IsCancellationRequested)
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

          // Update subnetwork
          try
          { subManager.UpdateAllSubnetworks(tier, true); }
          catch (Exception ex)
          { status = ExceptionFormat(ex); }
        }
      }, cps.Progressor);

      await QueuedTask.Run(() =>
      {
        if (String.IsNullOrEmpty(status) && !cps.Progressor.CancellationToken.IsCancellationRequested)
        {
          cps.Progressor.Value += 1;
          cps.Progressor.Status = (cps.Progressor.Value * 100 / cps.Progressor.Max) + @" % Completed";
          cps.Progressor.Message = "Step 4 – Color the diagram edges per subnetwork";
          ColorEdges.ExecuteReductionEdgeColorBySubnetwork(GetDiagramLayerFromMap(MapView.Active.Map));

          cps.Progressor.Value += 1;
          cps.Progressor.Status = (cps.Progressor.Value * 100 / cps.Progressor.Max) + @" % Completed";
          cps.Progressor.Message = "Step 5 – Redraw the Network Diagram";

          MapView.Active.Redraw(true);
          MapView.Active.ZoomTo(mapExtent);

          // re set the selection
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

      return status;
    }

    /// <summary>
    /// Internal execution fonction to toggle switches
    /// </summary>
    /// <returns>An error comment if needed, empty of no error</returns>
    private static string ToggleSwitchesExecute()
    {
      bool canUpdate = false;

      // Load all Diagram Element and selection
      if (!IsDiagramUsable(GetAllElements: false, GetSelection: true))
        return "";

      if (GlobalSelectedJunctionIDs.Count == 0)
      {
        return "There are no junction selected";
      }

      Dictionary<long, List<Guid>> guidBySourceId = new Dictionary<long, List<Guid>>();
      // retrieve the junctions GlobalId
      foreach (long oid in GlobalSelectedJunctionIDs)
      {
        DiagramJunctionElement junctionElement = GlobalDiagramJunctionElements.FirstOrDefault(a => a.ObjectID == oid);
        if (junctionElement != null)
        {

          if (!guidBySourceId.TryGetValue(junctionElement.AssociatedSourceID, out List<Guid> guidList))
          {
            guidList = new List<Guid>();
            guidBySourceId.Add(junctionElement.AssociatedSourceID, guidList);
          }

          if (!guidList.Contains(junctionElement.AssociatedGlobalID))
            guidList.Add(junctionElement.AssociatedGlobalID);
        }
      }

      IReadOnlyList<NetworkSource> theSources = GlobalUtilityNetwork.GetDefinition().GetNetworkSources();

      List<string> searchFields = new List<string> { cDeviceStatusFieldName, "ObjectId", "AssetGroup" };

      foreach (NetworkSource source in theSources)
      {
        if (guidBySourceId.TryGetValue(source.ID, out List<Guid> guidList))
        {
          // Get a cursor of guid list, get the qualified fields name
          using (RowCursor sel = GetRowCursorFromSourceNameAndGuidList(SourceName: source.Name.Replace(" ", ""), SearchGuid: guidList, ListSearchFields: searchFields, WhereField: "GlobalId", FieldsName: out List<Tuple<string, string>> FieldsName))
          {
            int deviceStatusIndex = -1;
            int assetGroupIndex = -1;
            // retrieved the fields indexes
            foreach (Tuple<string, string> findTuple in FieldsName)
            {
              if (findTuple.Item1 == cDeviceStatusFieldName)
                deviceStatusIndex = sel.FindField(findTuple.Item2);
              else if (findTuple.Item1 == "AssetGroup")
                assetGroupIndex = sel.FindField(findTuple.Item2);
            }

            if (deviceStatusIndex >= 0) // run only if there is a device status
            {
              var modifyStringsOperation = new EditOperation
              {
                Name = String.Format("Modify string field '{0}' in source {1}.", cDeviceStatusFieldName, source.Name)
              };

              ICollection<long> oidSet = new List<long>();
              while (sel.MoveNext())
              {
                string AssetGroupValue = sel.Current[assetGroupIndex].ToString();
                // verify if the Asset Group is correct
                if (!String.IsNullOrEmpty(AssetGroupValue) && AssetGroupValue == cAssetGroupFieldValue)
                {
                  string deviceStatus = sel.Current[deviceStatusIndex]?.ToString();
                  Guid globalIdValue = sel.Current.GetGlobalID();
                  long oid = sel.Current.GetObjectID();

                  // set up a new dictionary with fields to modify
                  var modifiedAttributes = new Dictionary<string, object>
          {
                    // add the name of the string field and the new attribute value to the dictionary
                    {cDeviceStatusFieldName, deviceStatus == "2" ? 1 : 2}
          };

                  // put the modify operation on the editor stack
                  modifyStringsOperation.Modify(sel.Current, modifiedAttributes);
                  oidSet.Add(oid);
                }
              }

              if (oidSet.Count > 0)
              {  // execute the modify operation to apply the changes
                modifyStringsOperation.Execute();
                canUpdate = true;
              }
              else
                modifyStringsOperation.Abort();
            }
          }
        }
      }

      return canUpdate ? "" : "This command only applies to medium voltage switches. Please make sure there is at least one selected medium voltage switch in the active diagram before its execution.";
    }

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

    #endregion Overrides

  }
}

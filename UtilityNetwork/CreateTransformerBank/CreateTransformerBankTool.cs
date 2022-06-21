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
using System.Threading.Tasks;
using System.Windows;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Core.Geometry;
using ArcGIS.Core.Data.UtilityNetwork;
using UtilityNetworkSamples;
using ArcGIS.Desktop.Mapping.Events;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Core.Data;

namespace CreateTransformerBank
{
  internal class CreateTransformerBankTool : MapTool
  {
    // Utility Network Constants - Change these to match your data model as needed

    const string AssemblyNetworkSourceName = "Electric Assembly";
    const string DeviceNetworkSourceName = "Electric Device";

    const string TransformerBankAssetGroupName = "Medium Voltage Transformer Bank";
    const string TransformerBankAssetTypeName = "Overhead Three Phase";

    const string TransformerAssetGroupName = "Medium Voltage Transformer";
    const string TransformerAssetTypeName = "Overhead Single Phase";

    const string FuseAssetGroupName = "Medium Voltage Fuse";
    const string FuseAssetTypeName = "Overhead Cutout Fused Disconnect";

    const string ArresterAssetGroupName = "Medium Voltage Arrester";
    const string ArresterAssetTypeName = "MV Line Arrester";

    const string PhaseFieldName = "phasescurrent";
    const int APhase = 4;
    const int BPhase = 2;
    const int CPhase = 1;

    const string DeviceStatusFieldName = "currentdevicestatus";
    const int DeviceStatusClosed = 2;
    const int DeviceStatusOpen = 1;

    // These numbers are used to space out the features created within the bank.
    const double XOffset = 1.0; 
    const double YOffset = 1.0;

   
    public CreateTransformerBankTool()
    {
      IsSketchTool = true;
      UseSnapping = true;
      // Select the type of construction tool you wish to implement.  
      // Make sure that the tool is correctly registered with the correct component category type in the daml 
      SketchType = SketchGeometryType.Point;
    }

    /// <summary>
    /// Called when the sketch finishes. This is where we will create the sketch operation and then execute it.
    /// </summary>
    /// <param name="geometry">The geometry created by the sketch.</param>
    /// <returns>A Task returning a Boolean indicating if the sketch complete event was successfully handled.</returns>
    protected override async Task<bool> OnSketchCompleteAsync(Geometry geometry)
    {
      if (geometry == null) return false;

      // Create an edit operation
      var createOperation = new EditOperation()
      {
        Name = "Create Transformer Bank",
        SelectNewFeatures = true
      };

      bool success = false;
      string errorMessage = "";

      await QueuedTask.Run(() =>
      {
        Map map = GetMap();

        using (UtilityNetwork utilityNetwork = GetUtilityNetwork())
        {
          if (utilityNetwork == null)
          {
            errorMessage = "Please select a layer that participates in a utility network.";
          }
          else
          {
            if (!ValidateDataModel(utilityNetwork))
            {
              errorMessage = "This sample is designed for a different utility network data model";
            }
            else
            {
              using (UtilityNetworkDefinition utilityNetworkDefinition = utilityNetwork.GetDefinition())
              // Get the NetworkSource, FeatureClass, AssetGroup, and AssetTypes for all of the features we want to create
              // The existence of these values has already been confirmed in the ValidateDataModel() routine

              // TransformerBank
              using (NetworkSource transformerBankNetworkSource = GetNetworkSource(utilityNetworkDefinition, AssemblyNetworkSourceName))
              using (FeatureClass transformerBankFeatureClass = utilityNetwork.GetTable(transformerBankNetworkSource) as FeatureClass)
              using (AssetGroup transformerBankAssetGroup = transformerBankNetworkSource.GetAssetGroup(TransformerBankAssetGroupName))
              using (AssetType transformerBankAssetType = transformerBankAssetGroup.GetAssetType(TransformerBankAssetTypeName))

              // Transformer
              using (NetworkSource deviceNetworkSource = GetNetworkSource(utilityNetworkDefinition, DeviceNetworkSourceName))
              using (FeatureClass deviceFeatureClass = utilityNetwork.GetTable(deviceNetworkSource) as FeatureClass)
              using (AssetGroup transformerAssetGroup = deviceNetworkSource.GetAssetGroup(TransformerAssetGroupName))
              using (AssetType transformerAssetType = transformerAssetGroup.GetAssetType(TransformerAssetTypeName))

              // Arrester
              using (AssetGroup arresterAssetGroup = deviceNetworkSource.GetAssetGroup(ArresterAssetGroupName))
              using (AssetType arresterAssetType = arresterAssetGroup.GetAssetType(ArresterAssetTypeName))

              // Fuse
              using (AssetGroup fuseAssetGroup = deviceNetworkSource.GetAssetGroup(FuseAssetGroupName))
              using (AssetType fuseAssetType = fuseAssetGroup.GetAssetType(FuseAssetTypeName))
              {
                MapPoint clickPoint = geometry as MapPoint;

                // Create a transformer bank

                RowToken token = createOperation.Create(transformerBankFeatureClass, CreateAttributes(transformerBankAssetGroup, transformerBankAssetType, clickPoint));
                RowHandle transformerBankHandle = new RowHandle(token);

                // Create three transformers, one for each phase

                MapPoint transformerPointA = CreateOffsetMapPoint(clickPoint, -1 * XOffset, YOffset);
                token = createOperation.Create(deviceFeatureClass, CreateDeviceAttributes(transformerAssetGroup, transformerAssetType, transformerPointA, APhase));
                RowHandle transformerHandleA = new RowHandle(token);

                MapPoint transformerPointB = CreateOffsetMapPoint(clickPoint, 0, YOffset);
                token = createOperation.Create(deviceFeatureClass, CreateDeviceAttributes(transformerAssetGroup, transformerAssetType, transformerPointB, BPhase));
                RowHandle transformerHandleB = new RowHandle(token);

                MapPoint transformerPointC = CreateOffsetMapPoint(clickPoint, XOffset, YOffset);
                token = createOperation.Create(deviceFeatureClass, CreateDeviceAttributes(transformerAssetGroup, transformerAssetType, transformerPointC, CPhase));
                RowHandle transformerHandleC = new RowHandle(token);

                // Create containment associations between the bank and the transformers
                AssociationDescription containmentAssociationDescription = new AssociationDescription(AssociationType.Containment, transformerBankHandle, transformerHandleA, false);
                createOperation.Create(containmentAssociationDescription);

                containmentAssociationDescription = new AssociationDescription(AssociationType.Containment, transformerBankHandle, transformerHandleB, false);
                createOperation.Create(containmentAssociationDescription);

                containmentAssociationDescription = new AssociationDescription(AssociationType.Containment, transformerBankHandle, transformerHandleC, false);
                createOperation.Create(containmentAssociationDescription);

                // Find the high-side terminal for transformers
                TerminalConfiguration transformerTerminalConfiguration = transformerAssetType.GetTerminalConfiguration();
                IReadOnlyList<Terminal> terminals = transformerTerminalConfiguration.Terminals;
                Terminal highSideTerminal = terminals.First(x => x.IsUpstreamTerminal == true);
                long highSideTerminalID = highSideTerminal.ID;

                // Create three fuses, one for each phase

                MapPoint fusePointA = CreateOffsetMapPoint(clickPoint, -1 * XOffset, 2 * YOffset);
                token = createOperation.Create(deviceFeatureClass, CreateDeviceAttributes(fuseAssetGroup, fuseAssetType, fusePointA, APhase));
                RowHandle fuseHandleA = new RowHandle(token);

                MapPoint fusePointB = CreateOffsetMapPoint(clickPoint, 0, 2 * YOffset);
                token = createOperation.Create(deviceFeatureClass, CreateDeviceAttributes(fuseAssetGroup, fuseAssetType, fusePointB, BPhase));
                RowHandle fuseHandleB = new RowHandle(token);

                MapPoint fusePointC = CreateOffsetMapPoint(clickPoint, XOffset, 2 * YOffset);
                token = createOperation.Create(deviceFeatureClass, CreateDeviceAttributes(fuseAssetGroup, fuseAssetType, fusePointC, CPhase));
                RowHandle fuseHandleC = new RowHandle(token);

                // Create containment associations between the bank and the fuses
                containmentAssociationDescription = new AssociationDescription(AssociationType.Containment, transformerBankHandle, fuseHandleA, false);
                createOperation.Create(containmentAssociationDescription);

                containmentAssociationDescription = new AssociationDescription(AssociationType.Containment, transformerBankHandle, fuseHandleB, false);
                createOperation.Create(containmentAssociationDescription);

                containmentAssociationDescription = new AssociationDescription(AssociationType.Containment, transformerBankHandle, fuseHandleC, false);
                createOperation.Create(containmentAssociationDescription);

                // Connect the high-side transformer terminals to the fuses (connect the A-phase transformer to the A-phase fuse, and so on)
                AssociationDescription connectivityAssociationDescription = new AssociationDescription(AssociationType.JunctionJunctionConnectivity, transformerHandleA, highSideTerminalID, fuseHandleA);
                createOperation.Create(connectivityAssociationDescription);

                connectivityAssociationDescription = new AssociationDescription(AssociationType.JunctionJunctionConnectivity, transformerHandleB, highSideTerminalID, fuseHandleB);
                createOperation.Create(connectivityAssociationDescription);

                connectivityAssociationDescription = new AssociationDescription(AssociationType.JunctionJunctionConnectivity, transformerHandleC, highSideTerminalID, fuseHandleC);
                createOperation.Create(connectivityAssociationDescription);

                // Create three arresters, one for each phase

                MapPoint arresterPointA = CreateOffsetMapPoint(clickPoint, -1 * XOffset, 3 * YOffset);
                token = createOperation.Create(deviceFeatureClass, CreateDeviceAttributes(arresterAssetGroup, arresterAssetType, arresterPointA, APhase));
                RowHandle arresterHandleA = new RowHandle(token);

                MapPoint arresterPointB = CreateOffsetMapPoint(clickPoint, 0, 3 * YOffset);
                token = createOperation.Create(deviceFeatureClass, CreateDeviceAttributes(arresterAssetGroup, arresterAssetType, arresterPointB, BPhase));
                RowHandle arresterHandleB = new RowHandle(token);

                MapPoint arresterPointC = CreateOffsetMapPoint(clickPoint, XOffset, 3 * YOffset);
                token = createOperation.Create(deviceFeatureClass, CreateDeviceAttributes(arresterAssetGroup, arresterAssetType, arresterPointC, CPhase));
                RowHandle arresterHandleC = new RowHandle(token);

                // Create containment associations between the bank and the arresters
                containmentAssociationDescription = new AssociationDescription(AssociationType.Containment, transformerBankHandle, arresterHandleA, false);
                createOperation.Create(containmentAssociationDescription);

                containmentAssociationDescription = new AssociationDescription(AssociationType.Containment, transformerBankHandle, arresterHandleB, false);
                createOperation.Create(containmentAssociationDescription);

                containmentAssociationDescription = new AssociationDescription(AssociationType.Containment, transformerBankHandle, arresterHandleC, false);
                createOperation.Create(containmentAssociationDescription);

                // Create connectivity associations between the fuses and the arresters (connect the A-phase fuse the A-phase arrester, and so on)
                connectivityAssociationDescription = new AssociationDescription(AssociationType.JunctionJunctionConnectivity, fuseHandleA, arresterHandleA);
                createOperation.Create(connectivityAssociationDescription);

                connectivityAssociationDescription = new AssociationDescription(AssociationType.JunctionJunctionConnectivity, fuseHandleB, arresterHandleB);
                createOperation.Create(connectivityAssociationDescription);

                connectivityAssociationDescription = new AssociationDescription(AssociationType.JunctionJunctionConnectivity, fuseHandleC, arresterHandleC);
                createOperation.Create(connectivityAssociationDescription);

                // Execute the edit operation, which creates all of the rows and associations
                success = createOperation.Execute();

                if (!success)
                {
                  errorMessage = createOperation.ErrorMessage;
                }
              }
            }
          }
        }
      });

      if (!success)
      {
        MessageBox.Show(errorMessage, "Create Transformer Bank Tool");
      }
      return success;
    }

    // Get the Utility Network from the currently active layer
    private UtilityNetwork GetUtilityNetwork()
    {
      UtilityNetwork utilityNetwork = null;

      if (MapView.Active != null)
      {
        IReadOnlyList<Layer> selectedLayers = MapView.Active.GetSelectedLayers();
        if (selectedLayers.Count > 0)
        {
          utilityNetwork = UtilityNetworkUtils.GetUtilityNetworkFromLayer(selectedLayers[0]);
        }
      }
      return utilityNetwork;
    }

    // Get the current map
    private Map GetMap()
    {
      if (MapView.Active == null) return null;
      return MapView.Active.Map;
    }

    // CreateOffsetMapPoint - creates a new MapPoint offset from a given base point.
    private MapPoint CreateOffsetMapPoint(MapPoint basePoint, double xOffset, double yOffset)
    {
      MapPointBuilderEx mapPointBuilder = new MapPointBuilderEx();
      mapPointBuilder.X = basePoint.X + xOffset;
      mapPointBuilder.Y = basePoint.Y + yOffset;
      mapPointBuilder.Z = basePoint.Z;
      mapPointBuilder.HasZ = true;
      return mapPointBuilder.ToGeometry();
    }

    // CreateAttributes - creates a dictionary of attribute-value pairs
    private Dictionary<string, object> CreateAttributes(AssetGroup assetGroup, AssetType assetType, MapPoint mapPoint)
    {
      var attributes = new Dictionary<string, object>();
      attributes.Add("SHAPE", mapPoint);
      attributes.Add("ASSETGROUP", assetGroup.Code);
      attributes.Add("ASSETTYPE", assetType.Code);
      return attributes;
    }

    private Dictionary<string, object> CreateDeviceAttributes(AssetGroup assetGroup, AssetType assetType, MapPoint mapPoint, int phase)
    {
      var attributes = CreateAttributes(assetGroup, assetType, mapPoint);
      attributes.Add(DeviceStatusFieldName, DeviceStatusClosed);
      attributes.Add(PhaseFieldName, phase);
      return attributes;
    }

    // ValidateDataModel - This sample is hard-wired to a particular version of the Naperville data model.
    // This routine checks to make sure we are using the correct one
    private bool ValidateDataModel(UtilityNetwork utilityNetwork)
    {
      bool dataModelIsValid = false;

      try
      {
        using (UtilityNetworkDefinition utilityNetworkDefinition = utilityNetwork.GetDefinition())

        using (NetworkSource transformerBankNetworkSource = GetNetworkSource(utilityNetworkDefinition, AssemblyNetworkSourceName))
        using (AssetGroup transformerBankAssetGroup = transformerBankNetworkSource.GetAssetGroup(TransformerBankAssetGroupName))
        using (AssetType transformerBankAssetType = transformerBankAssetGroup.GetAssetType(TransformerBankAssetTypeName))

        // Transformer
        using (NetworkSource deviceNetworkSource = GetNetworkSource(utilityNetworkDefinition, DeviceNetworkSourceName))
        using (AssetGroup transformerAssetGroup = deviceNetworkSource.GetAssetGroup(TransformerAssetGroupName))
        using (AssetType transformerAssetType = transformerAssetGroup.GetAssetType(TransformerAssetTypeName))

        // Arrester
        using (AssetGroup arresterAssetGroup = deviceNetworkSource.GetAssetGroup(ArresterAssetGroupName))
        using (AssetType arresterAssetType = arresterAssetGroup.GetAssetType(ArresterAssetTypeName))

        // Fuse
        using (AssetGroup fuseAssetGroup = deviceNetworkSource.GetAssetGroup(FuseAssetGroupName))
        using (AssetType fuseAssetType = fuseAssetGroup.GetAssetType(FuseAssetTypeName))
        {
          // Find the upstream terminal on the transformer
          TerminalConfiguration terminalConfiguration = transformerAssetType.GetTerminalConfiguration();
          Terminal upstreamTerminal = null;
          foreach(Terminal terminal in terminalConfiguration.Terminals)
          {
            if (terminal.IsUpstreamTerminal)
            {
              upstreamTerminal = terminal;
              break;
            }
          }

          // Find the terminal on the fuse
          Terminal fuseTerminal = fuseAssetType.GetTerminalConfiguration().Terminals[0];

          // Find the terminal on the arrester
          Terminal arresterTerminal = arresterAssetType.GetTerminalConfiguration().Terminals[0];

          // All of our asset groups and asset types exist.  Now we have to check for rules.

          IReadOnlyList<Rule> rules = utilityNetworkDefinition.GetRules();
          if (ContainmentRuleExists(rules, transformerBankAssetType, transformerAssetType) &&
            ContainmentRuleExists(rules, transformerBankAssetType, fuseAssetType) &&
            ContainmentRuleExists(rules, transformerBankAssetType, arresterAssetType) &&
            ConnectivityRuleExists(rules, transformerAssetType, upstreamTerminal, fuseAssetType, fuseTerminal) &&
            ConnectivityRuleExists(rules, fuseAssetType, fuseTerminal, arresterAssetType, arresterTerminal))
          {
            dataModelIsValid = true;
          }
        }
      }
      catch { }

      return dataModelIsValid;
    }

    private bool ContainmentRuleExists(IReadOnlyList<Rule> rules, AssetType sourceAssetType, AssetType destinationAssetType)
    {
      foreach(Rule rule in rules)
      {
        if (RuleType.Containment == rule.Type)
        {
          // For containment rules, the first rule element represents the container and the second rule element 
          // represents the content
          // Note that AssetType objects must be compared by using their Handles.
           if (rule.RuleElements[0].AssetType.Handle == sourceAssetType.Handle &&
            rule.RuleElements[1].AssetType.Handle == destinationAssetType.Handle)
          {
            return true;
          }
        }
      }
      return false;
    }

    private bool ConnectivityRuleExists(IReadOnlyList<Rule> rules, AssetType sourceAssetType, Terminal sourceTerminal, AssetType destinationAssetType, Terminal destinationTerminal)
    {
      foreach (Rule rule in rules)
      {
        if (RuleType.JunctionJunctionConnectivity == rule.Type)
        {
          // Connectivity rules can exist in either direction, so you have to look for both possibilities
          RuleElement ruleElement1 = rule.RuleElements[0];
          RuleElement ruleElement2 = rule.RuleElements[1];

          // Note that AssetType objects must be compared by using their Handles.
          if ((ruleElement1.AssetType.Handle == sourceAssetType.Handle && ruleElement1.Terminal == sourceTerminal && ruleElement2.AssetType.Handle == destinationAssetType.Handle && ruleElement2.Terminal == destinationTerminal) 
            ||
            (ruleElement1.AssetType.Handle == destinationAssetType.Handle && ruleElement1.Terminal == destinationTerminal && ruleElement2.AssetType.Handle == sourceAssetType.Handle && ruleElement2.Terminal == sourceTerminal))
          {
            return true;
          }
        }
      }
      return false;
    }

    private NetworkSource GetNetworkSource(UtilityNetworkDefinition utilityNetworkDefinition, string networkSourceName)
    {
      string networkSourceNameWithoutSpaces = networkSourceName.Replace(" ", "");
      IReadOnlyList<NetworkSource> networkSources = utilityNetworkDefinition.GetNetworkSources();
      foreach (NetworkSource networkSource in networkSources)
      {
        if (networkSource.Name.Replace(" ", "") == networkSourceNameWithoutSpaces) return networkSource;
      }
      return null; // Not found
    }
  }
}

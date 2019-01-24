/*

   Copyright 2019 Esri

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
using System.Threading.Tasks;
using System.Windows;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Core.Geometry;
using ArcGIS.Core.Data;
using ArcGIS.Core.Data.UtilityNetwork;
using UtilityNetworkSamples;
using ArcGIS.Desktop.Mapping.Events;
using ArcGIS.Desktop.Framework.Threading.Tasks;

namespace CreateTransformerBank
{
  internal class CreateTransformerBankTool : MapTool
  {
    // Utility Network Constants - Change these to match your data model as needed
    const string DomainNetworkName = "Electric Distribution Network";

    const string AssemblyNetworkSourceName = "Electric Distribution Assembly";
    const string DeviceNetworkSourceName = "Electric Distribution Device";

    const string TransformerBankAssetGroupName = "Transformer Bank";
    const string TransformerBankAssetTypeName = "Overhead Three Phase Delta";

    const string TransformerAssetGroupName = "Transformer";
    const string TransformerAssetTypeName = "Overhead Single Phase";

    const string FuseAssetGroupName = "Fuse";
    const string FuseAssetTypeName = "Overhead Cutout Fuse Non Load Breaking";

    const string ArresterAssetGroupName = "Arrester";
    const string ArresterAssetTypeName = "Medium Voltage Line Arrester";

    const string PhaseFieldName = "phasescurrent";
    const int APhase = 4;
    const int BPhase = 2;
    const int CPhase = 1;

    const string DeviceStatusFieldName = "devicestatus";
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
       //SketchType = SketchGeometryType.Line;
      // SketchType = SketchGeometryType.Polygon;
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
      var createOperation = new EditOperation();
      createOperation.Name = "Create Transformer Bank";
      createOperation.SelectNewFeatures = true;

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
            using (UtilityNetworkDefinition utilityNetworkDefinition = utilityNetwork.GetDefinition())

            // Get the NetworkSource, AssetGroup, and AssetTypes for all of the features we want to create
            // If this was production code, you would want to check the return values to make sure that these asset groups and asset types existed.

            // TransformerBank
            using (NetworkSource transformerBankNetworkSource = utilityNetworkDefinition.GetNetworkSource(AssemblyNetworkSourceName))
            using (AssetGroup transformerBankAssetGroup = transformerBankNetworkSource.GetAssetGroup(TransformerBankAssetGroupName))
            using (AssetType transformerBankAssetType = transformerBankAssetGroup.GetAssetType(TransformerBankAssetTypeName))

            // Transformer
            using (NetworkSource deviceNetworkSource = utilityNetworkDefinition.GetNetworkSource(DeviceNetworkSourceName))
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
              Layer transformerBankLayer = GetLayerForEdit(map, AssemblyNetworkSourceName, TransformerBankAssetGroupName);
              RowToken token = createOperation.CreateEx(transformerBankLayer, CreateAttributes(transformerBankAssetGroup, transformerBankAssetType, clickPoint));
              RowHandle transformerBankHandle = new RowHandle(token);

              // Create three transformers, one for each phase
              Layer transformerLayer = GetLayerForEdit(map, DeviceNetworkSourceName, TransformerAssetGroupName);

              MapPoint transformerPointA = CreateOffsetMapPoint(clickPoint, -1 * XOffset, YOffset);
              token = createOperation.CreateEx(transformerLayer, CreateDeviceAttributes(transformerAssetGroup, transformerAssetType, transformerPointA, APhase));
              RowHandle transformerHandleA = new RowHandle(token);

              MapPoint transformerPointB = CreateOffsetMapPoint(clickPoint, 0, YOffset);
              token = createOperation.CreateEx(transformerLayer, CreateDeviceAttributes(transformerAssetGroup, transformerAssetType, transformerPointB, BPhase));
              RowHandle transformerHandleB = new RowHandle(token);

              MapPoint transformerPointC = CreateOffsetMapPoint(clickPoint, XOffset, YOffset);
              token = createOperation.CreateEx(transformerLayer, CreateDeviceAttributes(transformerAssetGroup, transformerAssetType, transformerPointC, CPhase));
              RowHandle transformerHandleC = new RowHandle(token);

              // Create containment associations between the bank and the transformers
              ContainmentAssociationDescription containmentAssociationDescription = new ContainmentAssociationDescription(transformerBankHandle, transformerHandleA, false);
              createOperation.Create(containmentAssociationDescription);

              containmentAssociationDescription = new ContainmentAssociationDescription(transformerBankHandle, transformerHandleB, false);
              createOperation.Create(containmentAssociationDescription);

              containmentAssociationDescription = new ContainmentAssociationDescription(transformerBankHandle, transformerHandleC, false);
              createOperation.Create(containmentAssociationDescription);

              // Create three arresters, one for each phase
              Layer arresterLayer = GetLayerForEdit(map, DeviceNetworkSourceName, ArresterAssetGroupName);

              MapPoint arresterPointA = CreateOffsetMapPoint(clickPoint, -1 * XOffset, 2 * YOffset);
              token = createOperation.CreateEx(arresterLayer, CreateDeviceAttributes(arresterAssetGroup, arresterAssetType, arresterPointA, APhase));
              RowHandle arresterHandlA = new RowHandle(token);

              MapPoint arresterPointB = CreateOffsetMapPoint(clickPoint, 0, 2 * YOffset);
              token = createOperation.CreateEx(arresterLayer, CreateDeviceAttributes(arresterAssetGroup, arresterAssetType, arresterPointB, BPhase));
              RowHandle arresterHandleB = new RowHandle(token);

              MapPoint arresterPointC = CreateOffsetMapPoint(clickPoint, XOffset, 2 * YOffset);
              token = createOperation.CreateEx(arresterLayer, CreateDeviceAttributes(arresterAssetGroup, arresterAssetType, arresterPointC, CPhase));
              RowHandle arresterHandleC = new RowHandle(token);

              // Create containment associations between the bank and the arresters
              containmentAssociationDescription = new ContainmentAssociationDescription(transformerBankHandle, arresterHandlA, false);
              createOperation.Create(containmentAssociationDescription);

              containmentAssociationDescription = new ContainmentAssociationDescription(transformerBankHandle, arresterHandleB, false);
              createOperation.Create(containmentAssociationDescription);

              containmentAssociationDescription = new ContainmentAssociationDescription(transformerBankHandle, arresterHandleC, false);
              createOperation.Create(containmentAssociationDescription);

              // Find the high-side terminal for transformers
              TerminalConfiguration transformerTerminalConfiguration = transformerAssetType.GetTerminalConfiguration();
              IReadOnlyList<Terminal> terminals = transformerTerminalConfiguration.Terminals;
              Terminal highSideTerminal = terminals.First(x => x.IsUpstreamTerminal == true);
              long highSideTerminalID = highSideTerminal.ID;

              // Connect the high-side transformer terminals to the arresters (connect the A-phase transformer to the A-phase arrester, and so on)
              ConnectivityAssociationDescription connectivityAssociationDescription = new ConnectivityAssociationDescription(transformerHandleA, highSideTerminalID, arresterHandlA);
              createOperation.Create(connectivityAssociationDescription);

              connectivityAssociationDescription = new ConnectivityAssociationDescription(transformerHandleB, highSideTerminalID, arresterHandleB);
              createOperation.Create(connectivityAssociationDescription);

              connectivityAssociationDescription = new ConnectivityAssociationDescription(transformerHandleC, highSideTerminalID, arresterHandleC);
              createOperation.Create(connectivityAssociationDescription);

              // Create three fuses, one for each phase
              Layer fuseLayer = GetLayerForEdit(map, DeviceNetworkSourceName, FuseAssetGroupName);

              MapPoint fusePointA = CreateOffsetMapPoint(clickPoint, -1 * XOffset, 3 * YOffset);
              token = createOperation.CreateEx(fuseLayer, CreateDeviceAttributes(fuseAssetGroup, fuseAssetType, fusePointA, APhase));
              RowHandle fuseHandleA = new RowHandle(token);

              MapPoint fusePointB = CreateOffsetMapPoint(clickPoint, 0, 3 * YOffset);
              token = createOperation.CreateEx(fuseLayer, CreateDeviceAttributes(fuseAssetGroup, fuseAssetType, fusePointB, BPhase));
              RowHandle fuseHandleB = new RowHandle(token);

              MapPoint fusePointC = CreateOffsetMapPoint(clickPoint, XOffset, 3 * YOffset);
              token = createOperation.CreateEx(fuseLayer, CreateDeviceAttributes(fuseAssetGroup, fuseAssetType, fusePointC, CPhase));
              RowHandle fuseHandleC = new RowHandle(token);

              // Create containment associations between the bank and the fuses
              containmentAssociationDescription = new ContainmentAssociationDescription(transformerBankHandle, fuseHandleA, false);
              createOperation.Create(containmentAssociationDescription);

              containmentAssociationDescription = new ContainmentAssociationDescription(transformerBankHandle, fuseHandleB, false);
              createOperation.Create(containmentAssociationDescription);

              containmentAssociationDescription = new ContainmentAssociationDescription(transformerBankHandle, fuseHandleC, false);
              createOperation.Create(containmentAssociationDescription);

              // Create connectivity associations between the fuses and the arresters (connect the A-phase fuse the A-phase arrester, and so on)
              connectivityAssociationDescription = new ConnectivityAssociationDescription(fuseHandleA, arresterHandlA);
              createOperation.Create(connectivityAssociationDescription);

              connectivityAssociationDescription = new ConnectivityAssociationDescription(fuseHandleB, arresterHandleB);
              createOperation.Create(connectivityAssociationDescription);

              connectivityAssociationDescription = new ConnectivityAssociationDescription(fuseHandleC, arresterHandleC);
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
        MapViewEventArgs mapViewEventArgs = new MapViewEventArgs(MapView.Active);
        IReadOnlyList<Layer> selectedLayers = mapViewEventArgs.MapView.GetSelectedLayers();
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


    // Get the layer to create our feature on
    // It looks for either:
    //    - A feature layer for the given feature class
    //    - a subtype group layer for the given feature class, returning the feature layer child that matches the provided subtype
    private Layer GetLayerForEdit(Map map, string featureClassName, string subtypeName)
    {
      foreach (Layer layer in map.GetLayersAsFlattenedList())
      {
        if (layer is SubtypeGroupLayer)
        {
          if (layer.Name == featureClassName)
          {
            CompositeLayer compositeLayer = layer as SubtypeGroupLayer;
            foreach (Layer subtypeLayer in compositeLayer.Layers)
            {
              if (subtypeLayer.Name == subtypeName)
              {
                return subtypeLayer;
              }
            }
          }
        }
        else if (layer is FeatureLayer)
        {
          if (layer.Name == featureClassName)
          {
            return layer;
          }
        }
      }
      return null;
    }

    // CreateOffsetMapPoint - creates a new MapPoint offset from a given base point.
    private MapPoint CreateOffsetMapPoint(MapPoint basePoint, double xOffset, double yOffset)
    {
      using (MapPointBuilder mapPointBuilder = new MapPointBuilder())
      {
        mapPointBuilder.X = basePoint.X + xOffset;
        mapPointBuilder.Y = basePoint.Y + yOffset;
        mapPointBuilder.Z = basePoint.Z;
        mapPointBuilder.HasZ = true;
        return mapPointBuilder.ToGeometry();
      }
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

  }
}

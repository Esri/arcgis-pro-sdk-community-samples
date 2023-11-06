using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Data.UtilityNetwork;
using ArcGIS.Core.Data.UtilityNetwork.Trace;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Extensions;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Element = ArcGIS.Core.Data.UtilityNetwork.Element;

namespace AlternativeEnergizationAddIn
{
  //   Copyright 2019 Esri
  //   Licensed under the Apache License, Version 2.0 (the "License");
  //   you may not use this file except in compliance with the License.
  //   You may obtain a copy of the License at

  //       https://www.apache.org/licenses/LICENSE-2.0

  //   Unless required by applicable law or agreed to in writing, software
  //   distributed under the License is distributed on an "AS IS" BASIS,
  //   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
  //   See the License for the specific language governing permissions and
  //   limitations under the License. 
  internal class ShowCurrentIsolationOutageTool : MapTool
  {
    MapView _activeMap = null;    
    IReadOnlyList<Result> results = null;

    public ShowCurrentIsolationOutageTool()
    {
      IsSketchTool = true;
      SketchType = SketchGeometryType.Point;
      SketchOutputMode = SketchOutputMode.Map;
    }

    protected override Task OnToolActivateAsync(bool active)
    {
      return base.OnToolActivateAsync(active);
    }

    protected async override Task<bool> OnSketchCompleteAsync(Geometry geometry)
    {
      bool foundInitialElement = false;
      Utilities._sharedDownstreamProtectiveElements = null;
      Utilities._sharedInitialIsolationElements = null;
      Utilities._sharedOpenPointElements = null;
      Utilities._sharedStartingPointElement = null;
      Utilities._sharedStartingPointGeometry = null;

      MapView activeMap = MapView.Active;
      if (activeMap == null)
      {
        // Shouldn't happen
        MessageBox.Show("No active map");
        return false;
      }
      _activeMap = activeMap;
      try
      {
        await QueuedTask.Run(async() =>
        {
          try
          {
            SelectionSet featureSelections = activeMap.GetFeaturesEx(geometry);
            Dictionary<MapMember, List<long>> featureLayerToObjectIDMappings = featureSelections.ToDictionary();
            if (featureLayerToObjectIDMappings.Count == 0)
            {
              MessageBox.Show("Select a utility network feature.");
              foundInitialElement = false;
              return;
            }
            foreach (KeyValuePair<MapMember, List<long>> featureLayerToObjectIDMapping in featureLayerToObjectIDMappings)
            {
              if (featureLayerToObjectIDMapping.Key is FeatureLayer featureLayer)
              {
                using FeatureClass featureClass = featureLayer.GetFeatureClass();


                using UtilityNetwork utilityNetwork = Utilities.GetUtilityNetwork(featureClass);
                if (utilityNetwork == null)
                  continue;


                if (featureLayerToObjectIDMapping.Value.Count > 1)
                {
                  MessageBox.Show("Select only one utility network point feature.");
                  foundInitialElement = false;
                  return;
                }

                // get the element on which the user clicked as set it as a starting point
                Utilities.GetElement(utilityNetwork, featureClass, featureLayerToObjectIDMapping.Value[0], out Element element);

                if (element != null)
                {
                    if (element.NetworkSource.UsageType != SourceUsageType.Assembly && element.NetworkSource.UsageType != SourceUsageType.StructureBoundary && element.NetworkSource.UsageType != SourceUsageType.SubnetLine)
                    {
                        foundInitialElement = true;

                        // set as starting point
                        Utilities._sharedStartingPointGeometry = geometry;                        
                        Utilities._sharedStartingPointElement = element;
                        Utilities._sharedFeatureLayer = featureLayer;

                        _activeMap.Map.ClearSelection();

                        if (foundInitialElement == true)
                        {
                            using ProgressDialog progressDialog = new ProgressDialog("Running isolation trace...");
                            string errorMessage = string.Empty;

                            progressDialog.Show();
                            await QueuedTask.Run(() =>
                            {
                                UtilityNetwork utilityNetwork = Utilities.GetUtilityNetwork(Utilities._sharedFeatureLayer.GetFeatureClass());

                                // run isolation trace and include outage results
                                RunIsolationTrace(utilityNetwork, Utilities._sharedStartingPointElement);
                            });
                            break;
                        }
                        
                    }
                }
              }
            }
            if (Utilities._sharedFeatureLayer == null)
            {
              MessageBox.Show("Select a utility network feature.");
              return;
            }
          }
          catch (Exception e)
          {
            MessageBox.Show($"Exception: {e.Message}");
            return;
          }
        });

        
      }
      catch (Exception ex)
      {
        MessageBox.Show($@"Exception in ShowCurrentIsolationOutageTool: {ex.Message}");
      }
      return true;
    }

    private async void RunIsolationTrace(UtilityNetwork utilityNetwork, Element startingPointElement)
    {
      TraceManager traceManager = utilityNetwork.GetTraceManager();
      IsolationTracer tracer = traceManager.GetTracer<IsolationTracer>();

      List<Element> startingPointList = new List<Element>();
      startingPointList.Add(startingPointElement);

      TraceArgument traceArgument = new TraceArgument(startingPointList);
      UtilityNetworkDefinition undef = utilityNetwork.GetDefinition();

        try
        {
            DomainNetwork dn = undef.GetDomainNetwork(Utilities.domainNetworkName);

            TraceConfiguration traceConfiguration = dn.GetTier(Utilities.tierName).GetTraceConfiguration();
            traceConfiguration.Filter.Barriers = new CategoryComparison(CategoryOperator.IsEqual, Utilities.isolationCategory);
            traceConfiguration.IncludeIsolatedFeatures = true;

            traceArgument.Configuration = traceConfiguration;
            results = tracer.Trace(traceArgument);
        }
        catch (Exception e)
        {
            MessageBox.Show(e.Message);
            return;
        }

      // break the results into individual lists by class
      List<long> deviceElementsOIDS = new List<long>();
      List<long> junctionElementsOIDS = new List<long>();
      List<long> lineElementsOIDS = new List<long>();
      List<long> junctionObjectElementsOIDS = new List<long>();
      List<long> edgeObjectElementsOIDS = new List<long>();

      try
      {
        foreach (Result result in results)
        {
          if (result is ElementResult er)
          {
            IReadOnlyList<Element> elements = er.Elements;
            foreach (Element element2 in elements)
            {
              if (element2.NetworkSource.UsageType == SourceUsageType.Device)
              {
                deviceElementsOIDS.Add(element2.ObjectID);
              }
              else if (element2.NetworkSource.UsageType == SourceUsageType.Junction)
              {
                junctionElementsOIDS.Add(element2.ObjectID);
              }
              else if (element2.NetworkSource.UsageType == SourceUsageType.Line)
              {
                lineElementsOIDS.Add(element2.ObjectID);
              }
              else if (element2.NetworkSource.UsageType == SourceUsageType.JunctionObject)
              {
                junctionObjectElementsOIDS.Add(element2.ObjectID);
              }
              else if (element2.NetworkSource.UsageType == SourceUsageType.EdgeObject)
              {
                edgeObjectElementsOIDS.Add(element2.ObjectID);
              }
            }
            List<FeatureLayer> layers = _activeMap.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>().ToList<FeatureLayer>();

            // Loop through the layers and if you find a match, run a query and apply the selection
            foreach (FeatureLayer layer in layers)
            {
              if (layer.GetFeatureClass().GetName() == Utilities.deviceClass)
              {
                if (deviceElementsOIDS.Count > 0)
                {
                  // select all devices
                  await QueuedTask.Run(() =>
                  {
                    QueryFilter filter = new QueryFilter()
                    {
                      ObjectIDs = deviceElementsOIDS
                    };

                    layer.Select(filter, SelectionCombinationMethod.Add);
                  });
                }
              }
              else if (layer.GetFeatureClass().GetName() == Utilities.junctionClass)
              {
                if (junctionElementsOIDS.Count > 0)
                {
                  //select all junctions
                  await QueuedTask.Run(() =>
                  {
                    QueryFilter filter = new QueryFilter()
                    {
                      ObjectIDs = junctionElementsOIDS
                    };
                    layer.Select(filter, SelectionCombinationMethod.Add);
                  });
                }
              }
              else if (layer.GetFeatureClass().GetName() == Utilities.lineClass)
              {
                if (lineElementsOIDS.Count > 0)
                {
                  // select all lines
                  await QueuedTask.Run(() =>
                  {
                    QueryFilter filter = new QueryFilter()
                    {
                      ObjectIDs = lineElementsOIDS
                    };
                    layer.Select(filter, SelectionCombinationMethod.Add);
                  });
                }
              }
              else if (layer.GetFeatureClass().GetName() == Utilities.junctionObjectClass)
              {
                if (junctionObjectElementsOIDS.Count > 0)
                {
                  // select all junction objects
                  await QueuedTask.Run(() =>
                  {
                    QueryFilter filter = new QueryFilter()
                    {
                      ObjectIDs = junctionObjectElementsOIDS
                    };
                    layer.Select(filter, SelectionCombinationMethod.Add);
                  });
                }
              }
              else if (layer.GetFeatureClass().GetName() == Utilities.edgeObjectClass)
              {
                if (edgeObjectElementsOIDS.Count > 0)
                {
                  // select all edge objects
                  await QueuedTask.Run(() =>
                  {
                    QueryFilter filter = new QueryFilter()
                    {
                      ObjectIDs = edgeObjectElementsOIDS
                    };
                    layer.Select(filter, SelectionCombinationMethod.Add);
                  });
                }
              }
            }
          }
        }
      }
      catch (Exception ex)
      {
        throw new Exception($@"Exception in RunIsolationTrace: {ex.Message}");
      }
    }
  }
}
using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Data.UtilityNetwork;
using ArcGIS.Core.Data.UtilityNetwork.Trace;
using ArcGIS.Core.Geometry;
using ArcGIS.Core.Internal.CIM;
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
using System.Windows.Media;
using Element = ArcGIS.Core.Data.UtilityNetwork.Element;
using Geometry = ArcGIS.Core.Geometry.Geometry;
using QueryFilter = ArcGIS.Core.Data.QueryFilter;

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
  internal class ActivateAlternativeTool : Button
  {
    MapView _activeMap = null;
    FeatureLayer _featureLayer = null;
    IReadOnlyList<Result> results = null;

    protected async override void OnClick()
    {
      try
      {
        if (Utilities._sharedStartingPointGeometry != null)
        {
          bool foundInitialElement = false;
          MapView activeMap = MapView.Active;
          if (activeMap == null)
          {
            // Shouldn't happen
            MessageBox.Show("No active map");
            return;
          }
          _activeMap = activeMap;
          await QueuedTask.Run(() =>
          {
            try
            {              
                  using FeatureClass featureClass = Utilities._sharedFeatureLayer.GetFeatureClass();
                  using UtilityNetwork utilityNetwork = Utilities.GetUtilityNetwork(featureClass);
                                    
                  if (Utilities._sharedStartingPointElement != null)
                  {
                    foundInitialElement = true;

                    // clear any map selections or graphics
                    _activeMap.Map.ClearSelection();
                    var graphicsLayer = MapView.Active.Map.GetLayersAsFlattenedList().OfType<ArcGIS.Desktop.Mapping.GraphicsLayer>().FirstOrDefault();
                    if (graphicsLayer != null)
                    {
                      graphicsLayer.RemoveElements();
                    }
                    _featureLayer = Utilities._sharedFeatureLayer;
                  }
                
              
              if (_featureLayer == null)
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

          if (foundInitialElement == true)
          {
            using ProgressDialog progressDialog = new("Running isolation trace...");
            string errorMessage = string.Empty;

            progressDialog.Show();

            await QueuedTask.Run(() =>
            {
              UtilityNetwork utilityNetwork = Utilities.GetUtilityNetwork(Utilities._sharedFeatureLayer.GetFeatureClass());

              // run connected trace and include outage results
              RunConnectedTrace(utilityNetwork, Utilities._sharedStartingPointElement);
            });
          }
          return;
        }
        else
        {
          MessageBox.Show("Use the Isolation Outage tool to first select a feature.");
        }
      }
      catch (Exception ex)
      {
        MessageBox.Show($@"Exception in ActivateAlternativeTool: {ex.Message}");
      }
    }

    private async void RunConnectedTrace(UtilityNetwork utilityNetwork, Element sharedStartingPointElement)
    {
      TraceManager traceManager = utilityNetwork.GetTraceManager();
      ConnectedTracer tracer = traceManager.GetTracer<ConnectedTracer>();

      List<Element> startingPointList = new List<Element>();
      startingPointList.Add(sharedStartingPointElement);

      TraceArgument traceArgument = new TraceArgument(startingPointList);

      // combine all the possible barrier elements into one list
      List<Element> combinedElements = new List<Element>();
        try
        {
                if (Utilities._sharedDownstreamProtectiveElements != null)
                {
                    foreach (Element element in Utilities._sharedDownstreamProtectiveElements)
                    {
                        combinedElements.Add(element);
                    }
                }
                if (Utilities._sharedInitialIsolationElements != null)
                {
                    foreach (Element element in Utilities._sharedInitialIsolationElements)
                    {
                        combinedElements.Add(element);
                    }
                }
                if (Utilities._sharedOpenPointElements != null)
                {
                    foreach (Element element in Utilities._sharedOpenPointElements)
                    {
                        combinedElements.Add(element);
                    }
                }
        }
        catch { }
      
      traceArgument.Barriers = combinedElements;

      TraceConfiguration traceConfiguration = new TraceConfiguration();

      UtilityNetworkDefinition undef = utilityNetwork.GetDefinition();

        try
        {
            traceConfiguration.Filter.Barriers = new NetworkAttributeComparison(undef.GetNetworkAttribute(Utilities.networkAttributeForOpenPoint), (Operator)ComparisonOperator.IsEqual, Utilities.openPointValue);

            traceArgument.Configuration = traceConfiguration;

            results = tracer.Trace(traceArgument);
        }
        catch (Exception e)
        {
            MessageBox.Show(e.Message);
            return;
        }

      // break the results into objectid collections by class
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

            // Loop through the layers and try to find a match
            // then query for the oids and make them a selection
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
        throw new Exception ($@"Exception in RunConnectedTrace: {ex.Message}");
      }
    }
  }
}

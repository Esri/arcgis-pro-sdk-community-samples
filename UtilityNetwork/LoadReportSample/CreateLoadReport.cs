//   Copyright 2019 Esri
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at

//       http://www.apache.org/licenses/LICENSE-2.0

//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArcGIS.Core.Data;
using ArcGIS.Core.Data.UtilityNetwork;
using ArcGIS.Core.Data.UtilityNetwork.Trace;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Core.Geoprocessing;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Internal.Mapping;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Desktop.Mapping.Events;
using UtilityNetworkSamples;

namespace LoadReportSample
{
  /// <summary>
  /// This add-in demonstrates the creation of a simple electric distribution report.  It traces downstream from a given point and adds up the count of customers and total load per phase.  This sample
  /// is meant to be a demonstration on how to use the Utility Network portions of the SDK.  The report display is rudimentary.  Look elsewhere in the SDK for better examples on how to display data.
  /// 
  /// Rather than coding special logic to pick a starting point, this sample leverages the existing Set Trace Locations tool that is included with the product.
  /// That tool writes rows to a table called UN_Temp_Starting_Points, which is stored in the default project workspace.  This sample reads rows from that table and uses them as starting points
  /// for our downstream trace.
  /// </summary>
  /// <remarks>
  /// Instructions for use
  /// 1. Select a utility network layer or a feature layer that participates in a utility network
  /// 2. Click on the SDK Samples tab on the Utility Network tab group
  /// 3. Click on the Starting Points tool to create a starting point on the map
  /// 4. Click on the Create Load Report tool
  /// </remarks>

  internal class CreateLoadReport : Button
  {
    // Constants - used with the starting points table created by the Set Trace Locations tool

    private const string StartingPointsTableName = "UN_Temp_Starting_Points";
    private const string PointsSourceIDFieldName = "SourceID";
    private const string PointsAssetGroupFieldName = "AssetGroupCode";
    private const string PointsAssetTypeFieldName = "AssetType";
    private const string PointsGlobalIDFieldName = "FEATUREGLOBALID";
    private const string PointsTerminalFieldName = "TERMINALID";
    private const string PointsPercentAlong = "PERCENTALONG";

    // Constants - used with the Esri Electric Distribution Data Model

    private const string ElectricDomainNetwork = "ElectricDistribution";
    private const string MediumVoltageTier = "Medium Voltage Radial";
    private const string ServicePointCategory = "ServicePoint";

    private const short DeviceStatusOpened = 1;
    private const short DeviceStatusClosed = 2;

    // Network Attributes - there are a number of different sample databases with
    // different attribute names.  The sample tries to be flexible about which names it
    // will use
    private static readonly string[] PhaseAttributeNames = { "Phases Current", "Distribution Phases Current" };
    private static readonly string[] LoadAttributeNames = { "Customer Load", "Service Load", "Power Rating" };
    private static readonly string[] DeviceStatusAttributeNames = { "Operational Device Status", "Electric Device Status" };

    private const short APhase = 4;
    private const short BPhase = 2;
    private const short CPhase = 1;

    /// <summary>
    /// OnClick
    /// 
    /// This is the implementation of our button.  We pass the selected layer to GenerateReport() which does the bulk of the work.
    /// We then display the results, along with error messages, in a MessageBox.
    /// 
    /// </summary>

    protected override async void OnClick()
    {
      // Start by checking to make sure we have a single feature layer selected

      if (MapView.Active == null)
      {
        MessageBox.Show("Please select a utility network layer.", "Create Load Report");
        return;
      }

      MapViewEventArgs mapViewEventArgs = new MapViewEventArgs(MapView.Active);
      if (mapViewEventArgs.MapView.GetSelectedLayers().Count != 1)
      {
        MessageBox.Show("Please select a utility network layer.", "Create Load Report");
        return;
      }

      Layer selectionLayer = mapViewEventArgs.MapView.GetSelectedLayers()[0];
      if (!(selectionLayer is UtilityNetworkLayer) && !(selectionLayer is FeatureLayer) && !(selectionLayer is SubtypeGroupLayer))
      {
        MessageBox.Show("Please select a utility network layer.", "Create Load Report");
        return;
      }

      // Generate our report.  The LoadTraceResults class is used to pass back results from the worker thread to the UI thread that we're currently executing.
      LoadTraceResults traceResults = await QueuedTask.Run<LoadTraceResults>(() =>
      {
        return GenerateReport(selectionLayer);
      });

      // Assemble a string to show in the message box

      string traceResultsString;
      if (traceResults.Success)
      {
        traceResultsString = String.Format("Customers per Phase:\n   A: {0}\n   B: {1}\n   C: {2}\n\nLoad per Phase:\n   A: {3}\n   B: {4}\n   C: {5}\n\n{6}",
          traceResults.NumberServicePointsA.ToString(), traceResults.NumberServicePointsB.ToString(), traceResults.NumberServicePointsC.ToString(),
          traceResults.TotalLoadA.ToString(), traceResults.TotalLoadB.ToString(), traceResults.TotalLoadC.ToString(),
          traceResults.Message);
      }
      else
      {
        traceResultsString = traceResults.Message;
      }

      // Show our results

      MessageBox.Show(traceResultsString, "Create Load Report");
    }

    private NetworkAttribute GetAttribute(UtilityNetworkDefinition utilityNetworkDefinition, string[] nameCandidates)
    {
      // Returns a Network Attribute that matches one of the names passed in as the nameCandidates argument

      IReadOnlyList<NetworkAttribute> networkAttributes = utilityNetworkDefinition.GetNetworkAttributes();
      foreach(NetworkAttribute networkAttribute in networkAttributes)
      {
        if (nameCandidates.Any(x => x == networkAttribute.Name))
        {
          return networkAttribute;
        }
        else
        {
          networkAttribute.Dispose();
        }
      }
      return null;
    }

    /// <summary>
    /// GenerateReport
    /// 
    /// This routine takes a feature layer that references a feature class that participates in a utility network.
    /// It returns a set of data to display on the UI thread.
    /// 
    /// 
    /// </summary>

    public LoadTraceResults GenerateReport(Layer selectedLayer)
    {
      // Create a new results object.  We use this class to pass back a set of data from the worker thread to the UI thread

      LoadTraceResults results = new LoadTraceResults();

      // Initialize a number of geodatabase objects

      using (UtilityNetwork utilityNetwork = UtilityNetworkUtils.GetUtilityNetworkFromLayer(selectedLayer))
      {
        if (utilityNetwork == null)
        {
          results.Message = "Please select a utility network layer.";
          results.Success = false;
        }
        else
        {
          using (Geodatabase utilityNetworkGeodatabase = utilityNetwork.GetDatastore() as Geodatabase)
          using (UtilityNetworkDefinition utilityNetworkDefinition = utilityNetwork.GetDefinition())
          using (Geodatabase defaultGeodatabase = new Geodatabase(new FileGeodatabaseConnectionPath(new Uri(Project.Current.DefaultGeodatabasePath))))
          using (TraceManager traceManager = utilityNetwork.GetTraceManager())
          {
            // First check to make sure we have a feature service workspace.  Utility Network functionality requires this.
            if (utilityNetworkGeodatabase.GetGeodatabaseType() != GeodatabaseType.Service)
            {
              results.Message = "A feature service workspace connection is required.";
              results.Success = false;
              return results;
            }

            // Get a row from the starting points table in the default project workspace.  This table is created the first time the user creates a starting point
            // If the table is missing or empty, a null row is returned

            using (Row startingPointRow = GetStartingPointRow(defaultGeodatabase, ref results))
            {
              if (startingPointRow != null)
              {

                // Convert starting point row into network element

                Element startingPointElement = GetElementFromPointRow(startingPointRow, utilityNetwork, utilityNetworkDefinition);

                // Obtain a tracer object

                DownstreamTracer downstreamTracer = traceManager.GetTracer<DownstreamTracer>();

                // Get the network attributes that we will use in our trace

                using (NetworkAttribute phasesNetworkAttribute = GetAttribute(utilityNetworkDefinition, PhaseAttributeNames))
                using (NetworkAttribute loadNetworkAttribute = GetAttribute(utilityNetworkDefinition, LoadAttributeNames))
                using (NetworkAttribute deviceStatusNetworkAttribute = GetAttribute(utilityNetworkDefinition, DeviceStatusAttributeNames))
                {
                  if (phasesNetworkAttribute == null || loadNetworkAttribute == null || deviceStatusNetworkAttribute == null)
                  {
                    results.Success = false;
                    results.Message = "This add-in requires network attributes for phase, service load, and device status.\n";
                    return results;
                  }


                  // Get the Tier for Medium Voltage Radial

                  DomainNetwork electricDomainNetwork = utilityNetworkDefinition.GetDomainNetwork(ElectricDomainNetwork);
                  Tier mediumVoltageTier = electricDomainNetwork.GetTier(MediumVoltageTier);


                  // Set up the trace configuration

                  TraceConfiguration traceConfiguration = new TraceConfiguration();

                  // Configure the trace to use the electric domain network

                  traceConfiguration.DomainNetwork = electricDomainNetwork;

                  // Take the default TraceConfiguration from the Tier for Traversability

                  Traversability tierTraceTraversability = mediumVoltageTier.TraceConfiguration.Traversability;
                  traceConfiguration.Traversability.FunctionBarriers = tierTraceTraversability.FunctionBarriers;
                  traceConfiguration.IncludeBarriersWithResults = mediumVoltageTier.TraceConfiguration.IncludeBarriersWithResults;
                  traceConfiguration.Traversability.Scope = tierTraceTraversability.Scope;
                  ConditionalExpression baseCondition = tierTraceTraversability.Barriers as ConditionalExpression;

                  // Create a condition to only return features that have the service point category

                  ConditionalExpression servicePointCategoryCondition = new CategoryComparison(CategoryOperator.IsEqual, ServicePointCategory);

                  // Create function to sum loads on service points where phase = A

                  ConditionalExpression aPhaseCondition = new NetworkAttributeComparison(phasesNetworkAttribute, Operator.DoesNotIncludeTheValues, APhase);
                  Add aPhaseLoad = new Add(loadNetworkAttribute, servicePointCategoryCondition);


                  // Create function to sum loads on service points where phase = B

                  ConditionalExpression bPhaseCondition = new NetworkAttributeComparison(phasesNetworkAttribute, Operator.DoesNotIncludeTheValues, BPhase);
                  Add bPhaseLoad = new Add(loadNetworkAttribute, servicePointCategoryCondition);

                  // Create function to sum loads on service points where phase = C

                  ConditionalExpression cPhaseCondition = new NetworkAttributeComparison(phasesNetworkAttribute, Operator.DoesNotIncludeTheValues, CPhase);
                  Add cPhaseLoad = new Add(loadNetworkAttribute, servicePointCategoryCondition);

                  // Set the output condition to only return features that have the service point category

                  traceConfiguration.OutputCondition = servicePointCategoryCondition;

                  // Create starting point list and trace argument object

                  List<Element> startingPointList = new List<Element>() { startingPointElement };
                  TraceArgument traceArgument = new TraceArgument(startingPointList);
                  traceArgument.Configuration = traceConfiguration;

                  // Trace on the A phase

                  traceConfiguration.Traversability.Barriers = new Or(baseCondition, aPhaseCondition);
                  traceConfiguration.Functions = new List<Function>() { aPhaseLoad };
                  traceArgument.Configuration = traceConfiguration;

                  try
                  {
                    IReadOnlyList<Result> resultsA = downstreamTracer.Trace(traceArgument);

                    ElementResult elementResult = resultsA.OfType<ElementResult>().First();
                    results.NumberServicePointsA = elementResult.Elements.Count;

                    FunctionOutputResult functionOutputResult = resultsA.OfType<FunctionOutputResult>().First();
                    results.TotalLoadA = (double)functionOutputResult.FunctionOutputs.First().Value;
                  }
                  catch (ArcGIS.Core.Data.GeodatabaseUtilityNetworkException e)
                  {
                    //No A phase connectivity to source
                    if (!e.Message.Equals("No subnetwork source was discovered."))
                    {
                      results.Success = false;
                      results.Message += e.Message;
                    }
                  }

                  // Trace on the B phase

                  traceConfiguration.Traversability.Barriers = new Or(baseCondition, bPhaseCondition);
                  traceConfiguration.Functions = new List<Function>() { bPhaseLoad };
                  traceArgument.Configuration = traceConfiguration;

                  try
                  {
                    IReadOnlyList<Result> resultsB = downstreamTracer.Trace(traceArgument);

                    ElementResult elementResult = resultsB.OfType<ElementResult>().First();
                    results.NumberServicePointsB = elementResult.Elements.Count;

                    FunctionOutputResult functionOutputResult = resultsB.OfType<FunctionOutputResult>().First();
                    results.TotalLoadB = (double)functionOutputResult.FunctionOutputs.First().Value;
                  }
                  catch (ArcGIS.Core.Data.GeodatabaseUtilityNetworkException e)
                  {
                    // No B phase connectivity to source
                    if (!e.Message.Equals("No subnetwork source was discovered."))
                    {
                      results.Success = false;
                      results.Message += e.Message;
                    }
                  }

                  // Trace on the C phase
                  traceConfiguration.Traversability.Barriers = new Or(baseCondition, cPhaseCondition);
                  traceConfiguration.Functions = new List<Function>() { cPhaseLoad };
                  traceArgument.Configuration = traceConfiguration;

                  try
                  {
                    IReadOnlyList<Result> resultsC = downstreamTracer.Trace(traceArgument);

                    ElementResult elementResult = resultsC.OfType<ElementResult>().First();
                    results.NumberServicePointsC = elementResult.Elements.Count;

                    FunctionOutputResult functionOutputResult = resultsC.OfType<FunctionOutputResult>().First();
                    results.TotalLoadC = (double)functionOutputResult.FunctionOutputs.First().Value;
                  }
                  catch (ArcGIS.Core.Data.GeodatabaseUtilityNetworkException e)
                  {
                    // No C phase connectivity to source
                    if (!e.Message.Equals("No subnetwork source was discovered."))
                    {
                      results.Success = false;
                      results.Message += e.Message;
                    }
                  }
                }

                // append success message to the output string

                results.Message += "Trace successful.";
                results.Success = true;
              }
            }
          }
        }
      }
      return results;
    }


    /// <summary>
    /// GetStartingPointRow
    /// 
    /// This routine opens up the starting points table and tries to read a row.  This table is created in 
    /// the default project workspace when the user first creates a starting point.
    /// 
    /// If the table doesn't exist or is empty, we add an error to our results object a null row.
    /// If the table contains one row, we just return the row
    /// If the table contains more than one row, we return the first row, and log a warning message
    ///		(this tool only works with one starting point)
    /// 
    /// </summary>

    private Row GetStartingPointRow(Geodatabase defaultGeodatabase, ref LoadTraceResults results)
    {
      try
      {
        using (FeatureClass startingPointsFeatureClass = defaultGeodatabase.OpenDataset<FeatureClass>(StartingPointsTableName))
        using (RowCursor startingPointsCursor = startingPointsFeatureClass.Search())
        {
          if (startingPointsCursor.MoveNext())
          {
            Row row = startingPointsCursor.Current;
            
            if (startingPointsCursor.MoveNext())
            {
              // If starting points table has more than one row, append warning message
              results.Message += "Multiple starting points found.  Only the first one was used.";
              startingPointsCursor.Current.Dispose();
            }
            return row;
            
          }
          else
          {
            // If starting points table has no rows, exit with error message
            results.Message += "No starting points found.  Please create one using the Set Trace Locations tool.\n";
            results.Success = false;
            return null;
          }
        }
      }
      // If we cannot open the feature class, an exception is thrown
      catch (Exception)
      {
        results.Message += "No starting points found.  Please create one using the Set Trace Locations tool.\n";
        results.Success = false;
        return null;
      }
    }

    /// <summary>
    /// This routine takes a row from the starting (or barrier) point table and converts it to a [Network] Element that we can use for tracing
    /// </summary>
    /// <param name="pointRow">The Row (either starting point or barrier point)</param>
    /// <param name="utilityNetwork">Utility Network to be used</param>
    /// <param name="definition">Utility Network definition to be used</param>
    /// <returns>newly created element</returns>
    private static Element GetElementFromPointRow(Row pointRow,
        UtilityNetwork utilityNetwork, UtilityNetworkDefinition definition)
    {
      // Fetch the SourceID, AssetGroupCode, AssetType, GlobalID, and TerminalID values from the starting point row
      int sourceID = (int)pointRow[PointsSourceIDFieldName];
      int assetGroupID = (int)pointRow[PointsAssetGroupFieldName];
      int assetTypeID = (int)pointRow[PointsAssetTypeFieldName];
      Guid globalID = new Guid(pointRow[PointsGlobalIDFieldName].ToString());
      int terminalID = (int)pointRow[PointsTerminalFieldName];
      double percentAlong = (double)pointRow[PointsPercentAlong];

      // Fetch the NetworkSource, AssetGroup, and AssetType objects
      NetworkSource networkSource = definition.GetNetworkSources().First(x => x.ID == sourceID);
      AssetGroup assetGroup = networkSource.GetAssetGroups().First(x => x.Code == assetGroupID);
      AssetType assetType = assetGroup.GetAssetTypes().First(x => x.Code == assetTypeID);

      // Fetch the Terminal object from the ID
      Terminal terminal = null;

      if (assetType.IsTerminalConfigurationSupported())
      {
        TerminalConfiguration terminalConfiguration = assetType.GetTerminalConfiguration();
        terminal = terminalConfiguration.Terminals.First(x => x.ID == terminalID);
      }

      // Create and return a FeatureElement object
      // If we have an edge, set the PercentAlongEdge property; otherwise set the Terminal property
      if (networkSource.Type == SourceType.Edge)
      {
        Element element = utilityNetwork.CreateElement(assetType, globalID);
        element.PercentAlongEdge = percentAlong;
        return element;
      }
      else
      {
        Element element = utilityNetwork.CreateElement(assetType, globalID, terminal);
        return element;
      }
    }
  }
}


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
using ArcGIS.Core.Data.UtilityNetwork;
using ArcGIS.Core.Data.UtilityNetwork.Trace;
using ArcGIS.Core.Geometry;
using ArcGIS.Core.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TraceNetworkSample
{
  /// <summary>
  /// This console application demonstrates downstream trace from a Medium Voltage Transformer to Service Points on the Utility Network. Rather than coding particular logic to pick a starting point, the sample uses a known medium voltage transformer as a starting point for trace. The example illustrates a simple use case for using the Utility Network in the CoreHost application.
  /// The sample uses the NapervilleElectricSDKData.gdb, a file geodatabase available in the Community Sample data at C:\Data\UtilityNetwork (see under the "Resources" section for downloading sample data).
  /// You can also use any utility network data with this sample, although constant values may need to be changed.
  /// </summary>  
  /// <remarks>
  /// 1. In Visual Studio click the Build menu.  Then select Build Solution.  
  /// 1. Click Start button to run the sample.  
  /// 1. The trace results will be displayed as below
  /// ![UI](Screenshots/Screenshot1.png)  
  /// </remarks>
  internal class Program
  {
    //[STAThread] must be present on the Application entry point
    [STAThread]
    static void Main(string[] args)
    {
      //Call Host.Initialize before constructing any objects from ArcGIS.Core
      Host.Initialize();

      // Location of a geodatabase downloaded from the community sample data -  https://github.com/Esri/arcgis-pro-sdk-community-samples/releases/
      string geodatabasePath = @"C:\Data\UtilityNetwork\NapervilleElectricSDKData.gdb";

      // Guid of a known Medium Voltagee Transformer to begin a trace
      Guid traceStartGuid = Guid.Parse("{5D6574D1-340A-43A8-8BEE-7A87914CE477}");

      ExecuteTrace(traceStartGuid, geodatabasePath);
    }
    private static void ExecuteTrace(Guid traceStartGuid, string geodatabasePath)
    {
      using (Geodatabase geodatabase = new Geodatabase(new FileGeodatabaseConnectionPath(new Uri(geodatabasePath))))
      {
        IReadOnlyList<UtilityNetworkDefinition> utilityNetworkDefinitions = geodatabase.GetDefinitions<UtilityNetworkDefinition>();

        string utilityNetworkName = string.Empty;

        if (utilityNetworkDefinitions.Count < 0 || utilityNetworkDefinitions.Count > 1)
        {
          return;
        }

        // Get utility network name from the dataset
        foreach (UtilityNetworkDefinition definition in utilityNetworkDefinitions)
        {
          utilityNetworkName = definition.GetName();

          Console.WriteLine($"Utility network name: {utilityNetworkName}");
          definition.Dispose();
        }

        // Open utility network
        using (UtilityNetwork utilityNetwork = geodatabase.OpenDataset<UtilityNetwork>(utilityNetworkName))
        using (UtilityNetworkDefinition utilityNetworkDefinition = utilityNetwork.GetDefinition())
        {
          using (NetworkSource networkSource = utilityNetworkDefinition.GetNetworkSource("ElectricDevice"))
          using (AssetGroup assetGroup = networkSource.GetAssetGroup("Medium Voltage Transformer"))
          using (AssetType assetType = assetGroup.GetAssetType("Overhead Single Phase"))
          {
            DomainNetwork domainNetwork = utilityNetworkDefinition.GetDomainNetwork("Electric");
            Tier sourceTier = domainNetwork.GetTier("Electric Distribution");
            TraceConfiguration traceConfiguration = sourceTier.GetTraceConfiguration();

            // Get downstream side of the terminal
            Terminal terminal = null;
            if (assetType.IsTerminalConfigurationSupported())
            {
              TerminalConfiguration terminalConfiguration = assetType.GetTerminalConfiguration();
              IReadOnlyList<Terminal> terminals = terminalConfiguration.Terminals;
              terminal = terminals.First(t => !t.IsUpstreamTerminal);
            }

            // Create an element to begin a trace
            Element startingPointElement = utilityNetwork.CreateElement(assetType, traceStartGuid, terminal);

            List<Element> startingPoints = new List<Element>();
            startingPoints.Add(startingPointElement);


            // Get trace manager 
            using (TraceManager traceManager = utilityNetwork.GetTraceManager())
            {
              // Set trace configurations 
              TraceArgument traceArgument = new TraceArgument(startingPoints);
              traceArgument.Configuration = traceConfiguration;

              // Get downstream tracer
              Tracer tracer = traceManager.GetTracer<DownstreamTracer>();

              // Execuate downstream trace
              IReadOnlyList<Result> traceResults = tracer.Trace(traceArgument);

              // Display trace results in console
              foreach (Result result in traceResults)
              {
                if (result is ElementResult)
                {
                  ElementResult elementResult = result as ElementResult;
                  IReadOnlyList<Element> elements = elementResult.Elements;

                  Console.WriteLine("Trace result elements:");
                  foreach (Element element in elements)
                  {
                    Console.WriteLine($"\t OID: {element.ObjectID}, Name:{element.AssetType.Name}");
                  }
                }
                else if (result is FunctionOutputResult)
                {
                  FunctionOutputResult functionResult = result as FunctionOutputResult;
                  IReadOnlyList<FunctionOutput> functionOutputs = functionResult.FunctionOutputs;

                  Console.WriteLine("Trace result function outputs:");
                  foreach (FunctionOutput functionOut in functionOutputs)
                  {
                    Console.WriteLine($"\t Function result:{functionOut.Value}, name: {functionOut.Function}");
                  }
                }
                else if (result is AggregatedGeometryResult)
                {
                  AggregatedGeometryResult aggResults = result as AggregatedGeometryResult;
                  Polyline aggregatedLine = aggResults.Line as Polyline;
                  Multipoint aggregatedPoint = aggResults.Point as Multipoint;
                  Polygon aggregatedPolygon = aggResults.Polygon as Polygon;
                }
              }
            }
          }
        }
      }
    }
  }
}

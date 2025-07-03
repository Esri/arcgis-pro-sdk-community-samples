/*

   Copyright 2025 Esri

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

using ArcGIS.Core.Hosting;
using System;
using System.IO;
using System.Linq;
using BatchTracingCoreHost.Classes;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace BatchTracingCoreHost
{
    /// <summary>
    /// Standalone application that batch traces a utility network
    /// </summary>
    /// <remarks>
    /// 1. In Visual Studio click the Build menu.  Then select Build Solution.
    /// 1. Run the corresponding executable "BatchTracingCoreHost.exe" with a JSON file defining the configuration for the analysis you want to perform.
    /// 
    /// ## Sample Data
    /// 
    /// All examples were configured using the [Utility Network Foundation](https://www.esri.com/arcgis-blog/products/utility-network/electric-gas/utility-network-foundations/) data models from the ArcGIS Solutions team at the time the tools were developed. You will likely need to make adjustments to these files based on your own schema and data requirements.
    /// 
    /// This repository includes several examples you can use to get started:
    /// - [JSON Configuration files](./JSON%20Configurations): This directory contains a series of configuration files for different use cases
    /// - [Named Trace Configurations](./Trace%20Configurations): This directory contains the named trace configuration referenced in each JSON Configuration File
    /// 
    /// ## Analysis Types
    /// Each JSON file defines the type of analysis to be performed, and depending on the type of analysis there are additional parameters that are required. The different types of analysis are:
    /// - [Trace](trace.md) - Identify all the features connected to specific devices in your network.
    /// - [Partition](partition.md) - Parition your network into unique zones that cover specific types of lines or devices.
    /// - [Infer Subnetworks](infer.md) - Identify potential subnetworks and controllers for a tier in your network.
    /// 
    /// ---
    /// 
    /// # Output
    /// 
    /// ## Aggregated Geometry (Point, Line, Polygon)
    /// 
    /// ![Aggregated Geometry](Graphics/Aggregated%20Geometry.png "Aggregated geometry for the total drainage area for each outfall in a stormwater network.")
    /// 
    /// When configured, these tables will hold the aggregated geometry of the features returned by the trace. It will respect the Output Asset Type and Output Conditions of the trace configuration.
    /// 
    /// The corresponding table is cleared every time the tool is run, so if you configure multiple analysis each analysis should have its own table. Each trace has a single row containing all the geometries for that trace.
    /// 
    /// Fields
    /// - AnalysisName: Name of the analysis performed
    /// - TraceName: Name of the trace configuration that was executed
    /// 
    /// ---
    /// 
    /// ## Output Table
    /// 
    /// ![Ouptut Table](Graphics/Output%20Table.png "The output table shows all the elements returned by the trace.")
    /// 
    /// When configured, this table will include the information of the features returned by the trace. It will respect the Output Asset Type and Output Conditions of the trace configuration.
    /// 
    /// The output table is cleared every time the tool is run, so if you configure multiple analysis each analysis should have its own table. Each trace can produce many rows in this table.
    /// 
    /// Fields
    /// - AnalysisName: Name of the analysis performed
    /// - TraceName: Unique identifier from the starting feature of the trace (Batch Trace and Parition network), sequence number of the trace (Infer Subnetwork)
    /// - SourceIdentifier: Unique identifier from the result feature (Infer Subnetwork only)
    /// - NetworkSourceID: ID of the network source for the result feature
    /// - NetworkSourceName: Name of the network source for the result feature
    /// - ElementObjectID: Object ID of the result feature
    /// - ElementGuid: Global ID of the result feature
    /// - AssetGroupCode: Asset group code of the result feature
    /// - AssetGroupName: Asset group name of the result feature
    /// - AssetTypeCode: Asset type code of the result feature
    /// - AssetTypeName: Asset type name of the result feature
    /// - TerminalID: Terminal ID of the result feature, if applicable
    /// - TerminalName: Terminal name of the result feature, if applicable
    /// </remarks>
    class Program
    {
        //[STAThread] must be present on the Application entry point
        [STAThread]
        static void Main(string[] args)
        {
            //Call Host.Initialize before constructing any objects from ArcGIS.Core
            Host.Initialize();

            
                if(args.Length < 1)
                {
                    Console.WriteLine("Missing required argument: JSON Configuration file(s)");
                    return;
                }

            for (var i = 0; i < args.Length; i++)
            {
                var configurationFile = args[i];
                if (!configurationFile.Contains(".json", StringComparison.InvariantCultureIgnoreCase))
                {
                    Console.WriteLine("Argument {0} is not a JSON File: {1}", i, configurationFile);
                    continue;
                }

                try
                {
                    PerformAnalysis(configurationFile);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error running analysis number {0} for JSON File: {1}", i, configurationFile);
                    Console.WriteLine(ex);
                }
            }

            Console.WriteLine("{0}: All analysis complete", DateTime.Now.ToString());
        }

        private static void PerformAnalysis(string jsonConfigurationFile)
        {
            JsonNode jsonNode = null;
            Console.WriteLine("{0}: Reading configuration file: {1}", DateTime.Now.ToString(), jsonConfigurationFile);

            try
            {
                var jsonText = File.ReadAllText(jsonConfigurationFile);
                jsonNode = JsonNode.Parse(jsonText, default, new JsonDocumentOptions { AllowTrailingCommas = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Provided JSON Configuration file contains invalid JSON: {0}", jsonConfigurationFile);
                Console.WriteLine(ex);
            }

            if (jsonNode == null)
            {
                Console.WriteLine("Provided JSON Configuration file contains invalid JSON: {0}", jsonConfigurationFile);
                return;
            }

            var configuration = jsonNode.AsObject()
                .ToDictionary(property => property.Key, property =>
                {
                    var value = property.Value;
                    var valueKind = value.GetValueKind();
                    if (valueKind == JsonValueKind.String) return value.GetValue<string>();
                    else if (valueKind == JsonValueKind.Number) return value.GetValue<int>();
                    else if(valueKind == JsonValueKind.False || valueKind == JsonValueKind.True) return value.GetValue<bool>();
                    else return value.GetValue<object>();
                }
            );

            var type = Helpers.GetProperty<string>(configuration, "type");
            var analysisName = Helpers.GetProperty<string>(configuration, "analysisName");
            if (string.IsNullOrEmpty(analysisName))
                analysisName = Path.GetFileName(jsonConfigurationFile);

            Console.WriteLine("{0}: {1} - Performing analysis", DateTime.Now.ToString(), analysisName);

            switch(type)
            {
                case "Partition":
                    PartitionAnalysis.Execute(analysisName, configuration);
                    break;
                case "Trace":
                    BatchTrace.Execute(analysisName, configuration);
                    break;
                case "InferSubnetworks":
                    InferSubnetworks.Execute(analysisName, configuration);
                    break;
                default:
                    Console.WriteLine("{0}: {1} - Unrecognize analysis type: '{2}'", DateTime.Now.ToString(), analysisName, type);
                    return;
            }

            Console.WriteLine("{0}: {1} - Analysis complete", DateTime.Now.ToString(), analysisName);
        }
    }
}

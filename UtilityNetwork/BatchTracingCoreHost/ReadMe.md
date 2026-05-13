# Batch Tracing CoreHost application
The purpose of this is to demonstrate different techniques for using the ArcGIS Pro SDK to analyze networks using a standalone application. The behavior of the application is controlled by a JSON configuration file that defines what kind of analysis to perform. The analysis typically involves executing a named trace configuration over a series of features, then persisting the results for reporting or further analysis. The tool does not require subnetworks to run, however some named trace configurations may require subnetworks. It is not recommended to run this tool if you network contains unvalidated dirty areas.

```
Language:              C#
Subject:               Framework
Contributor:           ArcGIS Pro SDK Team <arcgisprosdk@esri.com>
Organization:          Esri, https://www.esri.com
Date:                  06/03/2025
ArcGIS Pro:            3.5
Visual Studio:         2022
.NET Target Framework: net8.0-windows
```

## Resources

[Community Sample Resources](https://github.com/Esri/arcgis-pro-sdk-community-samples#resources)


## How to use the sample

1. In Visual Studio click the Build menu.  Then select Build Solution.
1. Run the corresponding executable "BatchTracingCoreHost.exe" from a command prompt with a JSON file defining the configuration for the analysis you want to perform.
1. cmd> BatchTracingCoreHost.exe "*config*.json"
## Sample Data

All examples were configured using the [Utility Network Foundation](https://www.esri.com/arcgis-blog/products/utility-network/electric-gas/utility-network-foundations/) data models from the ArcGIS Solutions team at the time the tools were developed. You will likely need to make adjustments to these files based on your own schema and data requirements.

This repository includes several examples you can use to get started:
- [JSON Configuration files](./BatchTracingCoreHost/JSON%20Configurations): This directory contains a series of configuration files for different use cases
- [Named Trace Configurations](./BatchTracingCoreHost/Trace%20Configurations): This directory contains the named trace configuration referenced in each JSON Configuration File

## Analysis Types
Each JSON file defines the type of analysis to be performed, and depending on the type of analysis there are additional parameters that are required. The different types of analysis are:
- [Trace](trace.md) - Identify all the features connected to specific devices in your network.
- [Partition](partition.md) - Parition your network into unique zones that cover specific types of lines or devices.
- [Infer Subnetworks](infer.md) - Identify potential subnetworks and controllers for a tier in your network.

---

# Output

## Aggregated Geometry (Point, Line, Polygon)

![Aggregated Geometry](Graphics/Aggregated%20Geometry.png "Aggregated geometry for the total drainage area for each outfall in a stormwater network.")

When configured, these tables will hold the aggregated geometry of the features returned by the trace. It will respect the Output Asset Type and Output Conditions of the trace configuration.

The corresponding table is cleared every time the tool is run, so if you configure multiple analysis each analysis should have its own table. Each trace has a single row containing all the geometries for that trace.

Fields
- AnalysisName: Name of the analysis performed
- TraceName: Name of the trace configuration that was executed
- FunctionN (optional): If the named trace configuration or subnetwork definition contains Summaries or Functions they will be stored here

---

## Output Table

![Ouptut Table](Graphics/Output%20Table.png "The output table shows all the elements returned by the trace.")

When configured, this table will include the information of the features returned by the trace. It will respect the Output Asset Type and Output Conditions of the trace configuration.

The output table is cleared every time the tool is run, so if you configure multiple analysis each analysis should have its own table. Each trace can produce many rows in this table.

Fields
- AnalysisName: Name of the analysis performed
- TraceName: Unique identifier from the starting feature of the trace (Batch Trace and Parition network), sequence number of the trace (Infer Subnetwork)
- SourceIdentifier: Unique identifier from the result feature (Infer Subnetwork only)
- NetworkSourceID: ID of the network source for the result feature
- NetworkSourceName: Name of the network source for the result feature
- ElementObjectID: Object ID of the result feature
- ElementGuid: Global ID of the result feature
- AssetGroupCode: Asset group code of the result feature
- AssetGroupName: Asset group name of the result feature
- AssetTypeCode: Asset type code of the result feature
- AssetTypeName: Asset type name of the result feature
- TerminalID: Terminal ID of the result feature, if applicable
- TerminalName: Terminal name of the result feature, if applicable

---

<!-- End -->

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<img src="https://esri.github.io/arcgis-pro-sdk/images/ArcGISPro.png"  alt="ArcGIS Pro SDK for Microsoft .NET Framework" height = "20" width = "20" align="top"  >
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
[Home](https://github.com/Esri/arcgis-pro-sdk/wiki) | <a href="https://pro.arcgis.com/en/pro-app/latest/sdk/api-reference" target="_blank">API Reference</a> | [Requirements](https://github.com/Esri/arcgis-pro-sdk/wiki#requirements) | [Download](https://github.com/Esri/arcgis-pro-sdk/wiki#installing-arcgis-pro-sdk-for-net) | <a href="https://github.com/esri/arcgis-pro-sdk-community-samples" target="_blank">Samples</a>

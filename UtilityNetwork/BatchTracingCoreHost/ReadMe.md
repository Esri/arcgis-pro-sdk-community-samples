## BatchTracingCoreHost

<!-- TODO: Write a brief abstract explaining this sample -->
Standalone application that batch traces a utility network  
  


<a href="https://pro.arcgis.com/en/pro-app/sdk/" target="_blank">View it live</a>

<!-- TODO: Fill this section below with metadata about this sample-->
```
Language:              C#
Subject:               Console
Contributor:           ArcGIS Pro SDK Team <arcgisprosdk@esri.com>
Organization:          Esri, https://www.esri.com
Date:                  05/06/2025
ArcGIS Pro:            3.5
Visual Studio:         2022
.NET Target Framework: 4.6.1
```

## Resources

[Community Sample Resources](https://github.com/Esri/arcgis-pro-sdk-community-samples#resources)

### Samples Data

* Sample data for ArcGIS Pro SDK Community Samples can be downloaded from the [Releases](https://github.com/Esri/arcgis-pro-sdk-community-samples/releases) page.  

## How to use the sample
<!-- TODO: Explain how this sample can be used. To use images in this section, create the image file in your sample project's screenshots folder. Use relative url to link to this image using this syntax: ![My sample Image](FacePage/SampleImage.png) -->
1. In Visual Studio click the Build menu.  Then select Build Solution.
2. Run the corresponding executable "BatchTracingCoreHost.exe" with a JSON file defining the configuration for the analysis you want to perform.  
  
## Sample Data  
  
All examples were configured using the [Utility Network Foundation](https://www.esri.com/arcgis-blog/products/utility-network/electric-gas/utility-network-foundations/) data models from the ArcGIS Solutions team at the time the tools were developed. You will likely need to make adjustments to these files based on your own schema and data requirements.  
  
This repository includes several examples you can use to get started:  
- [JSON Configuration files](./JSON%20Configurations): This directory contains a series of configuration files for different use cases  
- [Named Trace Configurations](./Trace%20Configurations): This directory contains the named trace configuration referenced in each JSON Configuration File  
  
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
  

<!-- End -->

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<img src="https://esri.github.io/arcgis-pro-sdk/images/ArcGISPro.png"  alt="ArcGIS Pro SDK for Microsoft .NET Framework" height = "20" width = "20" align="top"  >
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
[Home](https://github.com/Esri/arcgis-pro-sdk/wiki) | <a href="https://pro.arcgis.com/en/pro-app/latest/sdk/api-reference" target="_blank">API Reference</a> | [Requirements](https://github.com/Esri/arcgis-pro-sdk/wiki#requirements) | [Download](https://github.com/Esri/arcgis-pro-sdk/wiki#installing-arcgis-pro-sdk-for-net) | <a href="https://github.com/esri/arcgis-pro-sdk-community-samples" target="_blank">Samples</a>

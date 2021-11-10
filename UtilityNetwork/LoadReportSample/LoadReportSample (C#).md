## LoadReportSample

<!-- TODO: Write a brief abstract explaining this sample -->
This add-in demonstrates the creation of a simple electric distribution report.  It traces downstream from a given point and adds up the count of customers and total load per phase.  This sample is meant to be a demonstration on how to use the Utility Network portions of the SDK.  The report display is rudimentary.  Look elsewhere in the SDK for better examples on how to display data.  Rather than coding special logic to pick a starting point, this sample leverages the existing Set Trace Locations tool that is included with the product.  That tool writes rows to a table called UN_Temp_Starting_Points, which is stored in the default project workspace.  This sample reads rows from that table and uses them as starting points for our downstream trace.  
  
Community Sample data (see under the "Resources" section for downloading sample data) has a UtilityNetworkSamples.aprx  project that contains a utility network that can be used with this sample.  This project can be found under the   
C:\Data\UtilityNetwork folder. Alternatively, you can also use any utility network data with this sample, although constant  values may need to be changed.  
  
  


<a href="http://pro.arcgis.com/en/pro-app/sdk/" target="_blank">View it live</a>

<!-- TODO: Fill this section below with metadata about this sample-->
```
Language:              C#
Subject:               UtilityNetwork
Contributor:           ArcGIS Pro SDK Team <arcgisprosdk@esri.com>
Organization:          Esri, http://www.esri.com
Date:                  11/01/2021
ArcGIS Pro:            2.9
Visual Studio:         2017, 2019
.NET Target Framework: 4.8
```

## Resources

[Community Sample Resources](https://github.com/Esri/arcgis-pro-sdk-community-samples#resources)

### Samples Data

* Sample data for ArcGIS Pro SDK Community Samples can be downloaded from the [Releases](https://github.com/Esri/arcgis-pro-sdk-community-samples/releases) page.  

## How to use the sample
<!-- TODO: Explain how this sample can be used. To use images in this section, create the image file in your sample project's screenshots folder. Use relative url to link to this image using this syntax: ![My sample Image](FacePage/SampleImage.png) -->
1. In Visual Studio click the Build menu.  Then select Build Solution.  
1. Click Start button to open ArcGIS Pro.  
1. ArcGIS Pro will open.  
1. Open C:\Data\UtilityNetwork\UtilityNetworkSamples.aprx or a map view that references a utility network with the correct schema  
1. Select a feature layer or subtype group layer that participates in a utility network or a utility network layer  
1. Click on the Load Report SDK Sample tab on the Utility Network tab group  
1. Click on the Starting Points tool to create a starting point on the map  
1. Click on the Create Load Report tool  
  


<!-- End -->

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<img src="https://esri.github.io/arcgis-pro-sdk/images/ArcGISPro.png"  alt="ArcGIS Pro SDK for Microsoft .NET Framework" height = "20" width = "20" align="top"  >
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
[Home](https://github.com/Esri/arcgis-pro-sdk/wiki) | <a href="https://pro.arcgis.com/en/pro-app/latest/sdk/api-reference" target="_blank">API Reference</a> | [Requirements](https://github.com/Esri/arcgis-pro-sdk/wiki#requirements) | [Download](https://github.com/Esri/arcgis-pro-sdk/wiki#installing-arcgis-pro-sdk-for-net) | <a href="https://github.com/esri/arcgis-pro-sdk-community-samples" target="_blank">Samples</a>

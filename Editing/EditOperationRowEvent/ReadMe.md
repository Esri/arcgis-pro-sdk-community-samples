## EditOperationRowEvent

<!-- TODO: Write a brief abstract explaining this sample -->
This sample shows how to reference and use the running EditOperation in row events.  
  


<a href="https://pro.arcgis.com/en/pro-app/sdk/" target="_blank">View it live</a>

<!-- TODO: Fill this section below with metadata about this sample-->
```
Language:              C#
Subject:               Editing
Contributor:           ArcGIS Pro SDK Team <arcgisprosdk@esri.com>
Organization:          Esri, https://www.esri.com
Date:                  10/01/2023
ArcGIS Pro:            3.2
Visual Studio:         2022
.NET Target Framework: net6.0-windows
```

## Resources

[Community Sample Resources](https://github.com/Esri/arcgis-pro-sdk-community-samples#resources)

### Samples Data

* Sample data for ArcGIS Pro SDK Community Samples can be downloaded from the [Releases](https://github.com/Esri/arcgis-pro-sdk-community-samples/releases) page.  

## How to use the sample
<!-- TODO: Explain how this sample can be used. To use images in this section, create the image file in your sample project's screenshots folder. Use relative url to link to this image using this syntax: ![My sample Image](FacePage/SampleImage.png) -->
This sample shows an example of the edit log being populated from either the default tools or a custom edit tool using the running Edit Operation within row events.  
  
1. Download the Community Sample data (see under the 'Resources' section for downloading sample data).  The sample data contains a map package called 'CrowdPlannerProject.ppkx' which is required for this sample.
2. Open this solution in Visual Studio.   
3. Click the build menu and select Build Solution.  
4. Launch the debugger to open ArCGIS Pro.  
5. Open the map package "CrowdPlannerProject.ppkx" located in the "C:\Data\CrowdPlanner" folder since this project contains all required data.  
6. Click on the Add-in tab and see that an 'Edit log' group has been added with two controls; 'Initialize' and 'Create Zone'.  
7. Click on the 'Initialize' button. This creates an edit log table that will record edits to the Crowdplanning layer.  
8. Dock the edit table with map so you can see records being added to the table as you edit the layer.  
9. With the default edit tools create some new poly polygons and make edits. You should see edit log records appear in the table.  
10. Undo an edit to see the correspnding row in the edit table be deleted.  
11. Click on the 'Create Zone' button. This creates a crowd planning polygon of a fixed size.  
  

<!-- End -->

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<img src="https://esri.github.io/arcgis-pro-sdk/images/ArcGISPro.png"  alt="ArcGIS Pro SDK for Microsoft .NET Framework" height = "20" width = "20" align="top"  >
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
[Home](https://github.com/Esri/arcgis-pro-sdk/wiki) | <a href="https://pro.arcgis.com/en/pro-app/latest/sdk/api-reference" target="_blank">API Reference</a> | [Requirements](https://github.com/Esri/arcgis-pro-sdk/wiki#requirements) | [Download](https://github.com/Esri/arcgis-pro-sdk/wiki#installing-arcgis-pro-sdk-for-net) | <a href="https://github.com/esri/arcgis-pro-sdk-community-samples" target="_blank">Samples</a>

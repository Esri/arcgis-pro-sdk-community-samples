## CrowdPlannerTool

<!-- TODO: Write a brief abstract explaining this sample -->
This sample shows the use of a construction tool to implement a crowd planning workflow.  
  


<a href="https://pro.arcgis.com/en/pro-app/sdk/" target="_blank">View it live</a>

<!-- TODO: Fill this section below with metadata about this sample-->
```
Language:              C#
Subject:               Editing
Contributor:           ArcGIS Pro SDK Team <arcgisprosdk@esri.com>
Organization:          Esri, https://www.esri.com
Date:                  11/04/2024
ArcGIS Pro:            3.4
Visual Studio:         2022
.NET Target Framework: net8.0-windows
```

## Resources

[Community Sample Resources](https://github.com/Esri/arcgis-pro-sdk-community-samples#resources)

### Samples Data

* Sample data for ArcGIS Pro SDK Community Samples can be downloaded from the [Releases](https://github.com/Esri/arcgis-pro-sdk-community-samples/releases) page.  

## How to use the sample
<!-- TODO: Explain how this sample can be used. To use images in this section, create the image file in your sample project's screenshots folder. Use relative url to link to this image using this syntax: ![My sample Image](FacePage/SampleImage.png) -->
1. Download the Community Sample data (see under the 'Resources' section for downloading sample data).  The sample data contains a map package called 'CrowdPlannerProject.ppkx' which is required for this sample.  Make sure that the Sample data is unzipped in c:\data and "C:\Data\CrowdPlanner" is available.
2. Open this solution in Visual Studio.    
3. Click the build menu and select Build Solution.  
4. Click the Start button to open ArCGIS Pro.  ArcGIS Pro will open.  
5. Open the map package "CrowdPlannerProject.ppkx" located in the "C:\Data\CrowdPlanner" folder since this project contains all required data.  
6. Click on the Add-in tab and see that a 'Crowd Planner Summary' button was added.  
7. The 'Crowd Planner Summary' button opens the 'Crowd Planner' pane.   
![UI](Screenshots/Screen1.png)  
8. Click the 'Populate Values' button and note that now the pane entry fields have been populated using data from the sample record in the Crowd Planning feature class.  
![UI](Screenshots/Screen2.png)  
9. Select the 'Edit' tab and create a new 'CrowdPlanning' feature by using the CP construction tool.  
![UI](Screenshots/Screen3.png)  
10. Digitize a new polygon and note that its attributes are automatically populated.   
![UI](Screenshots/Screen4.png)  
11. Finally you can also play with the various value adjustment buttons that are provided on the 'Crowd Planner' pane.  
![UI](Screenshots/Screen5.png)  
  

<!-- End -->

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<img src="https://esri.github.io/arcgis-pro-sdk/images/ArcGISPro.png"  alt="ArcGIS Pro SDK for Microsoft .NET Framework" height = "20" width = "20" align="top"  >
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
[Home](https://github.com/Esri/arcgis-pro-sdk/wiki) | <a href="https://pro.arcgis.com/en/pro-app/latest/sdk/api-reference" target="_blank">API Reference</a> | [Requirements](https://github.com/Esri/arcgis-pro-sdk/wiki#requirements) | [Download](https://github.com/Esri/arcgis-pro-sdk/wiki#installing-arcgis-pro-sdk-for-net) | <a href="https://github.com/esri/arcgis-pro-sdk-community-samples" target="_blank">Samples</a>

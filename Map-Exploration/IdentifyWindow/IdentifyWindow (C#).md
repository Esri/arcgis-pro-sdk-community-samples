## IdentifyWindow

<!-- TODO: Write a brief abstract explaining this sample -->
This sample illustrates working with ArcGIS Pro's map view and how to interact.  The sample provides the following functionality  
  
1. Show the layer for the current active map view.  
2. Select features on the current active map view.  
3. Display the attribute data for all selected features.  
4. Display a chart control with data driven by feature selection.  
  


<a href="https://pro.arcgis.com/en/pro-app/sdk/" target="_blank">View it live</a>

<!-- TODO: Fill this section below with metadata about this sample-->
```
Language:              C#
Subject:               Map-Exploration
Contributor:           ArcGIS Pro SDK Team <arcgisprosdk@esri.com>
Organization:          Esri, https://www.esri.com
Date:                  04/04/2024
ArcGIS Pro:            3.3
Visual Studio:         2022
.NET Target Framework: net8.0-windows
```

## Resources

[Community Sample Resources](https://github.com/Esri/arcgis-pro-sdk-community-samples#resources)

### Samples Data

* Sample data for ArcGIS Pro SDK Community Samples can be downloaded from the [Releases](https://github.com/Esri/arcgis-pro-sdk-community-samples/releases) page.  

## How to use the sample
<!-- TODO: Explain how this sample can be used. To use images in this section, create the image file in your sample project's screenshots folder. Use relative url to link to this image using this syntax: ![My sample Image](FacePage/SampleImage.png) -->
1. Download the Community Sample data (see under the 'Resources' section for downloading sample data).  The sample data contains a dataset called 'Interacting with Maps' with both 2D and 3D data.  Make sure that the Sample data is unzipped in c:\data and "C:\Data\Interacting with Maps" is available.
2. Open this solution in Visual Studio.    
3. Click the build menu and select Build Solution.  
4. This solution is using the **DotNetProjects.WpfToolkit.DataVisualization Nuget**.  If needed, you can install the Nuget from the "Nuget Package Manager Console" by using this script: "DotNetProjects.WpfToolkit.DataVisualization".  
5. Launch the debugger to open ArCGIS Pro.  ArcGIS Pro will open.  
6. Open the project "Interacting with Maps.aprx" in the "C:\Data\Interacting with Maps" folder since this project contains 2D and 3D data.  
7. Click on the Add-in tab and see that a 'Show my identify' button was added.  
8. The 'Show my identify' button opens the 'My Identify' pane.   
9. Click the 'Select' button and 'rubber band over the features on your map pane.  
![UI](Screenshots/Screen1.png)  
10. Select a single layer from the 'Select Layer' drop down.  
11. Both the grid and chart controls are now displaying data for the selected feature set  
![UI](Screenshots/Screen2.png)  
12. Switch to the Portland 3D City map view and perform the same feature selection on the map view and then the 'select layer' drop down selection on the 'My Identify' pane  
![UI](Screenshots/Screen3.png)  
  

<!-- End -->

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<img src="https://esri.github.io/arcgis-pro-sdk/images/ArcGISPro.png"  alt="ArcGIS Pro SDK for Microsoft .NET Framework" height = "20" width = "20" align="top"  >
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
[Home](https://github.com/Esri/arcgis-pro-sdk/wiki) | <a href="https://pro.arcgis.com/en/pro-app/latest/sdk/api-reference" target="_blank">API Reference</a> | [Requirements](https://github.com/Esri/arcgis-pro-sdk/wiki#requirements) | [Download](https://github.com/Esri/arcgis-pro-sdk/wiki#installing-arcgis-pro-sdk-for-net) | <a href="https://github.com/esri/arcgis-pro-sdk-community-samples" target="_blank">Samples</a>

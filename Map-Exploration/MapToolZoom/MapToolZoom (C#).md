## MapToolZoom

<!-- TODO: Write a brief abstract explaining this sample -->
ProGuide example for simple MapView interaction is demonstrated in this sample with a tool that allows to zoom in and out of the current MapView.  The left mouse click will zoom in and the right mouse click will zoom out.  Unlike the other Map Tool samples this example does not use the sketch capabilities of the MapTool base class instead the sample overrides mouse and keyboard events.    
  


<a href="https://pro.arcgis.com/en/pro-app/sdk/" target="_blank">View it live</a>

<!-- TODO: Fill this section below with metadata about this sample-->
```
Language:              C#
Subject:               Map-Exploration
Contributor:           ArcGIS Pro SDK Team <arcgisprosdk@esri.com>
Organization:          Esri, https://www.esri.com
Date:                  06/10/2022
ArcGIS Pro:            3.0
Visual Studio:         2022
.NET Target Framework: net6.0-windows
```

## Resources

[Community Sample Resources](https://github.com/Esri/arcgis-pro-sdk-community-samples#resources)

### Samples Data

* Sample data for ArcGIS Pro SDK Community Samples can be downloaded from the [Releases](https://github.com/Esri/arcgis-pro-sdk-community-samples/releases) page.  

## How to use the sample
<!-- TODO: Explain how this sample can be used. To use images in this section, create the image file in your sample project's screenshots folder. Use relative url to link to this image using this syntax: ![My sample Image](FacePage/SampleImage.png) -->
1. Download the Community Sample data (see under the 'Resources' section for downloading sample data).  The sample data contains a dataset called 'Interacting with Maps' with both 2D and 3D data.  Make sure that the Sample data is unzipped in c:\data and "C:\Data\Interacting with Maps" is available.  
1. Open this solution in Visual Studio.    
1. Click the build menu and select Build Solution.  
1. Launch the debugger to open ArCGIS Pro.   
1. Open the project "Interacting with Maps.aprx" in the "C:\Data\Interacting with Maps" folder since this project contains 2D and 3D data.  
1. Open the 2D crime map  
1. Click on the Add-in tab   
1. Click the 'Zoom In/Out' button and left click on the map somewhere off the map center.  
![UI](Screenshots/3MapTool2D.png)  
  
1. Validate the that the mouse click point is now at the center of the map view and that the view has zoomed in.  
![UI](Screenshots/3MapTool2D-2.png)  
  
1. Switch to the Portland 3D City map view and perform the zoom in/out on the 3D scene.  
![UI](Screenshots/3MapTool3D.png)  
  
1. Validate the zoom in/out functionality.  
![UI](Screenshots/3MapTool3D-2.png)  
  
1. Use the cursor up and cursor down keys and validate that the zoom in/out is working.  
  


<!-- End -->

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<img src="https://esri.github.io/arcgis-pro-sdk/images/ArcGISPro.png"  alt="ArcGIS Pro SDK for Microsoft .NET Framework" height = "20" width = "20" align="top"  >
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
[Home](https://github.com/Esri/arcgis-pro-sdk/wiki) | <a href="https://pro.arcgis.com/en/pro-app/latest/sdk/api-reference" target="_blank">API Reference</a> | [Requirements](https://github.com/Esri/arcgis-pro-sdk/wiki#requirements) | [Download](https://github.com/Esri/arcgis-pro-sdk/wiki#installing-arcgis-pro-sdk-for-net) | <a href="https://github.com/esri/arcgis-pro-sdk-community-samples" target="_blank">Samples</a>

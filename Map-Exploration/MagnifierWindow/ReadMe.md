## MagnifierWindow

<!-- TODO: Write a brief abstract explaining this sample -->
 This sample provides a map tool and a window with map control.  
 The position of the map control window is updated based on the current map tool position.  
 The map control content is created from the currently active MAP view and the map control shows a magnified view of where the mouse is positioned in the active MAP view.  
   


<a href="https://pro.arcgis.com/en/pro-app/sdk/" target="_blank">View it live</a>

<!-- TODO: Fill this section below with metadata about this sample-->
```
Language:              C#
Subject:               Map Exploration
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
 1. In Visual Studio click the Build menu. Then select Build Solution.  
 1. Launch the debugger to open ArcGIS Pro.  
 1. Open a 2D map view for example: C:\Data\FeatureTest\FeatureTest.aprx from the sample dataset.  
 1. Click on the Add-In tab  
 1. Click on the Magnifier tool  
 1. A map control window will open up that shows the same content as in your main 2D map view  
 1. Move the mouse in the main view and the camera in map control window will update to show you a magnified view of where you are in the main 2D map view. Additionally, the current geo-coordinates in the main view are also displayed in the lower left corner of the map control window  
 1. You can press the ESC key to deactivate the Magnifier tool  
![UI](Screenshots/Screen1.png)  
 NOTE - the magnifier tool only works in 2D map views but can be enhanced to work in 3D too.  
   


<!-- End -->

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<img src="https://esri.github.io/arcgis-pro-sdk/images/ArcGISPro.png"  alt="ArcGIS Pro SDK for Microsoft .NET Framework" height = "20" width = "20" align="top"  >
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
[Home](https://github.com/Esri/arcgis-pro-sdk/wiki) | <a href="https://pro.arcgis.com/en/pro-app/latest/sdk/api-reference" target="_blank">API Reference</a> | [Requirements](https://github.com/Esri/arcgis-pro-sdk/wiki#requirements) | [Download](https://github.com/Esri/arcgis-pro-sdk/wiki#installing-arcgis-pro-sdk-for-net) | <a href="https://github.com/esri/arcgis-pro-sdk-community-samples" target="_blank">Samples</a>

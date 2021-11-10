## IdentifyWithSketchTool

<!-- TODO: Write a brief abstract explaining this sample -->
This sample shows how to:   
* Add a custom sketch tool to work with map views  
* Use the sketch to create a new selection and zoom to the result in 2D or 3D  
* Create a custom identify tool for 2D and 3D  
  


<a href="http://pro.arcgis.com/en/pro-app/sdk/" target="_blank">View it live</a>

<!-- TODO: Fill this section below with metadata about this sample-->
```
Language:              C#
Subject:               Map Exploration
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
1. In Visual Studio click the Build menu. Then select Build Solution.  
1. Click Start button to open ArcGIS Pro.  
1. ArcGIS Pro will open.   
1. Open a project with a map view that has feature layers.  
1. Click on the Sketch tab and then on the 'Select And Zoom' button.  
1. On your map view 'sketch' (using the rubber band rectangle) an area containing features  
1. Features will be selected and the map view will be zoom to the selection area's extent  
1. Next click the 'Custom Identify' button and you will see the 'Identify Result' popup  
![UI](Screenshots/2DScreen.png)      
  
1. Now open a project that contains a scene with 3D features  
1. Click on the Sketch tab and then on the 'Select And Zoom' button  
1. On your 3D map view 'sketch' (using the rubber band rectangle) an area containing features  
1. Features will be selected and the map view will be zoom to the selection area's extent  
1. Next click the 'Custom Identify' button and you will see the 'Identify Result' pop-up  
![UI](Screenshots/3DScreen.png)      
  


<!-- End -->

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<img src="https://esri.github.io/arcgis-pro-sdk/images/ArcGISPro.png"  alt="ArcGIS Pro SDK for Microsoft .NET Framework" height = "20" width = "20" align="top"  >
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
[Home](https://github.com/Esri/arcgis-pro-sdk/wiki) | <a href="https://pro.arcgis.com/en/pro-app/latest/sdk/api-reference" target="_blank">API Reference</a> | [Requirements](https://github.com/Esri/arcgis-pro-sdk/wiki#requirements) | [Download](https://github.com/Esri/arcgis-pro-sdk/wiki#installing-arcgis-pro-sdk-for-net) | <a href="https://github.com/esri/arcgis-pro-sdk-community-samples" target="_blank">Samples</a>

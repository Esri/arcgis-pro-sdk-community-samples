## OverviewMapControl

<!-- TODO: Write a brief abstract explaining this sample -->
 This sample shows how to author a map control.    
 In this example we will be creating a map control that will act as an overview window of the active map.   
 When the view changes, the map control will display the changed view. You will also be able navigate inside the map control and see the Active map view mirror the map control's extent.  
   


<a href="https://pro.arcgis.com/en/pro-app/sdk/" target="_blank">View it live</a>

<!-- TODO: Fill this section below with metadata about this sample-->
```
Language:              C#
Subject:               Framework
Contributor:           ArcGIS Pro SDK Team <arcgisprosdk@esri.com>
Organization:          Esri, https://www.esri.com
Date:                  05/06/2025
ArcGIS Pro:            3.5
Visual Studio:         2022
.NET Target Framework: net8.0-windows
```

## Resources

[Community Sample Resources](https://github.com/Esri/arcgis-pro-sdk-community-samples#resources)

### Samples Data

* Sample data for ArcGIS Pro SDK Community Samples can be downloaded from the [Releases](https://github.com/Esri/arcgis-pro-sdk-community-samples/releases) page.  

## How to use the sample
<!-- TODO: Explain how this sample can be used. To use images in this section, create the image file in your sample project's screenshots folder. Use relative url to link to this image using this syntax: ![My sample Image](FacePage/SampleImage.png) -->
1. In Visual Studio click the Build menu. Then select Build Solution.
 2. Launch the debugger to open ArcGIS Pro.  
 3. ArcGIS Pro will open.   
 4. Open any project with map views. Make sure a map is active.  
 5. Within the "Map Control" tab click the "Show Map Control" button.  
 6. A dockpane will open up with an embedded Map Control displaying the overview of the Active map view. A red overview rectangle is displayed inside the map control showing the extent of the active map view.  
 7. Pan or zoom inside the active map view.  Notice that the Map conrol extent changes to reflect this.  
 8. Pan or zoom inside the map control. Notice that the active map view changes to reflect this.  
  
![UI](screenshots/mapcontrol.png)  
   

<!-- End -->

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<img src="https://esri.github.io/arcgis-pro-sdk/images/ArcGISPro.png"  alt="ArcGIS Pro SDK for Microsoft .NET Framework" height = "20" width = "20" align="top"  >
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
[Home](https://github.com/Esri/arcgis-pro-sdk/wiki) | <a href="https://pro.arcgis.com/en/pro-app/latest/sdk/api-reference" target="_blank">API Reference</a> | [Requirements](https://github.com/Esri/arcgis-pro-sdk/wiki#requirements) | [Download](https://github.com/Esri/arcgis-pro-sdk/wiki#installing-arcgis-pro-sdk-for-net) | <a href="https://github.com/esri/arcgis-pro-sdk-community-samples" target="_blank">Samples</a>

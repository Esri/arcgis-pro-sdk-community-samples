## CustomPopup

<!-- TODO: Write a brief abstract explaining this sample -->
 This sample shows how to author custom pop-up content to display in a pop-up window.   
 In this example we are generating html and javascript code using the Google Charts api to create rich and interactive content in the pop-up.   
 This example also shows how to add your own commands to the bottom of the pop-up window.   
   


<a href="https://pro.arcgis.com/en/pro-app/sdk/" target="_blank">View it live</a>

<!-- TODO: Fill this section below with metadata about this sample-->
```
Language:              C#
Subject:               Map-Exploration
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
1. In Visual Studio click the Build menu. Then select Build Solution.
 2. Launch the debugger to open ArcGIS Pro.  
 3. Open a map view. Click on the Add-In tab on the ribbon.  
 4. Within this tab there is a Custom Pop-up tool. Click it to activate the tool.  
 5. In the map click and drag a box around the features you want to identify.  
 6. The pop-up window should display and you should see a table showing the values for all the visible numeric fields in the layer.   
 It will also display a chart for those same fields.  
 7. As you click through the pop-up results the content is being generated dynamically for each feature.  
 8. The pop-up window also has a custom command "Show statistics" at the bottom of the window that when clicked shows additional information about the feature.  
  
![UI](screenshots/Popup.png)  
   

<!-- End -->

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<img src="https://esri.github.io/arcgis-pro-sdk/images/ArcGISPro.png"  alt="ArcGIS Pro SDK for Microsoft .NET Framework" height = "20" width = "20" align="top"  >
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
[Home](https://github.com/Esri/arcgis-pro-sdk/wiki) | <a href="https://pro.arcgis.com/en/pro-app/latest/sdk/api-reference" target="_blank">API Reference</a> | [Requirements](https://github.com/Esri/arcgis-pro-sdk/wiki#requirements) | [Download](https://github.com/Esri/arcgis-pro-sdk/wiki#installing-arcgis-pro-sdk-for-net) | <a href="https://github.com/esri/arcgis-pro-sdk-community-samples" target="_blank">Samples</a>

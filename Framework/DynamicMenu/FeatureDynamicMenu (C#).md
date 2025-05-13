## FeatureDynamicMenu

<!-- TODO: Write a brief abstract explaining this sample -->
 This sample shows how to display a dynamic context menu. The sample also have an embeddabel control on the map that allows you pick the layer you want to select.     
 When you select features with this tool, the oids of the features will be displayed in a dynamic context menu.   
 When you click one of the OIDs in this context menu, that feature will flash on the Map View and display a pop-up.   
   


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
 2. This solution is using the **Extended.Wpf.Toolkit NuGet**.  If needed, you can install the NuGet from the "NuGet Package Manager Console" by using this script: "Install-Package Extended.Wpf.Toolkit".  
 3. Click Start button to open ArcGIS Pro.  
 4. ArcGIS Pro will open.   
 5. Open a map view. The map should contain a few feature layers.  
 6. Click on the Add-In tab on the ribbon.  
 7. Within this tab there is a Display Dynamic Menu tool. Click it to activate the tool.  
 8. In the map click a point around which you want to identify features and display them in a dynamic menu.  
 9. A dynamic menu with the OIds of the features selected will display.  
 10. Click one of the items in the menu. You will see the feature flash on the map and their attributes will be displayed in a pop-up.  
![UI](Screenshots/DynamicMenu.png)  
   

<!-- End -->

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<img src="https://esri.github.io/arcgis-pro-sdk/images/ArcGISPro.png"  alt="ArcGIS Pro SDK for Microsoft .NET Framework" height = "20" width = "20" align="top"  >
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
[Home](https://github.com/Esri/arcgis-pro-sdk/wiki) | <a href="https://pro.arcgis.com/en/pro-app/latest/sdk/api-reference" target="_blank">API Reference</a> | [Requirements](https://github.com/Esri/arcgis-pro-sdk/wiki#requirements) | [Download](https://github.com/Esri/arcgis-pro-sdk/wiki#installing-arcgis-pro-sdk-for-net) | <a href="https://github.com/esri/arcgis-pro-sdk-community-samples" target="_blank">Samples</a>

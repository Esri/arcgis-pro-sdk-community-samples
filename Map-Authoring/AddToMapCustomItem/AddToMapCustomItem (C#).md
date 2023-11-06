## AddToMapCustomItem

<!-- TODO: Write a brief abstract explaining this sample -->
This sample illustrates how to add a custom item to the map using the IMappable and IMappebleEX Interfaces.  Item content can be added to ArcGIS Pro using the following application workflows:  
* "Add Data" dialog  
* "Add Data from Path" dialog  
* drag/drop of the item from the catalog window to a map  
* drag/drop of the item from the catalog window to the TOC  
* drag/drop of the item from windows explorer to a map  
* drag/drop of the item from windows explorer to the TOC  
  


<a href="https://pro.arcgis.com/en/pro-app/sdk/" target="_blank">View it live</a>

<!-- TODO: Fill this section below with metadata about this sample-->
```
Language:              C#
Subject:               Framework
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
1. Download the Community Sample data (see under the 'Resources' section for downloading sample data)
2. Make sure that the Sample data is unzipped in C:\Data  
3. The data used in this sample is located in this folder 'C:\Data\AddToMapCustomItem'  
4. In Visual Studio, build the solution.  
5. Click Start button to open ArcGIS Pro.  
6. In Pro, start with a new map.    
7. Activate the map so that there is an active map view.  
8. In the Catalog pane, create a new Folder Connection to the 'C:\Data\AddToMapCustomItem' folder.  
9. Notice the custom item in this folder called AlaskaCitiesXY.uxh. This file contains the coordinates of cities in Alaska.  
[UI](Screenshots/customItem.png)  
10. This custom item is defined in the solution in the "AddToMapCustomItem.cs" class file. Notice that the AddToMapCustomItem class implements the IMappable and IMappableEx interfaces.  
11. Place a breakpoint in the beginning of the `public string[] OnAddToMapEx(Map map)` method.  
12. Drag and drop the custom item (AlaskaCitiesXY.uxh) from the Catalog pane in Pro to the active Map view or to the TOC of the map view. This action will trigger the OnAddToMapEx callback and your breakpoint will be hit.  
13. 'CSVToPointFC' method converts the `AlaskaCitiesXY.uxh` file into a feature class in the project's file geodatabase. The geoprocessing tool `XYTableToPoint_management` is used to accomplish this.  
14. `LayerFactory.Instance.CreateLayer` method is then invoked to add this feature class to the active map view, using the Uri of the feature class in the File Geodatabase.  
[UI](Screenshots/CreateLayer.png)   
15. Alternatively, you can click the Add Data button on Pro's Map tab on the ribbon. Browse to the 'C:\Data\AddToMapCustomItem' in the Add data dialog that opens up.  
16. Select the AlaskaCitiesXY.uxh item located in this folder and click OK. The feature class will be added to the map view.  
[UI](Screenshots/AddToMap.png)      
17. This action will also invoke the OnAddToMapEx callback method. The Geoprocessing tool converts AlaskaCitiesXY.uxh custom item to a feature class and it will be added to the active map view.  
  

<!-- End -->

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<img src="https://esri.github.io/arcgis-pro-sdk/images/ArcGISPro.png"  alt="ArcGIS Pro SDK for Microsoft .NET Framework" height = "20" width = "20" align="top"  >
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
[Home](https://github.com/Esri/arcgis-pro-sdk/wiki) | <a href="https://pro.arcgis.com/en/pro-app/latest/sdk/api-reference" target="_blank">API Reference</a> | [Requirements](https://github.com/Esri/arcgis-pro-sdk/wiki#requirements) | [Download](https://github.com/Esri/arcgis-pro-sdk/wiki#installing-arcgis-pro-sdk-for-net) | <a href="https://github.com/esri/arcgis-pro-sdk-community-samples" target="_blank">Samples</a>

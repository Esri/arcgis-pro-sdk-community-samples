## ExcelDropHandler

<!-- TODO: Write a brief abstract explaining this sample -->
ArcGIS Pro Addin allows to Drag and drop *.xlsx file on ArcGIS Pro and execute the necessary Geoprocessing Tools automatically.   
  


<a href="https://pro.arcgis.com/en/pro-app/sdk/" target="_blank">View it live</a>

<!-- TODO: Fill this section below with metadata about this sample-->
```
Language:              C#
Subject:               Content
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
   
This sample uses Excel files in Pro.   To use Excel files in Pro, you need Microsoft Access Database Engine 2016. Refer to the [Work with Microsoft Excel files in ArcGIS Pro](https://pro.arcgis.com/en/pro-app/help/data/excel/work-with-excel-in-arcgis-pro.htm) help topic for more information on dowloading the required driver.  
  
1. Download the Community Sample data (see under the 'Resources' section for downloading sample data)
2. Make sure that the Sample data is unzipped in c:\data       
3. In Visual Studio click the Build menu. Then select Build Solution.  
4. Click Start button to open ArcGIS Pro.  
5. ArcGIS Pro will open.   
6. Open a blank Map project.  
7. ArcGIS Pro will display a map view.    
8. Add a new Local Scene view.  
9. Look at two eventhandler methods  
10. OnDragOver – The Pro framework calls this method when an Excel file is dragged onto the Map holding down the left mouse button.   
11. OnDrop – The Pro framework calls this method when the user releases the left mouse button and the Excel file is dropped on Pro.   
12. Take a closer look at the OnDrop logic where the code for the execution of the Geoprocessing Tool can be found.    
13. Drag and Drop Meteorites_UK.xls onto Pro in the Map View.    
14. View the results of the Meteorite strikes layer loaded into Pro with symbology applied    
![UI](Screenshots/2dScreen.png)  
15. Switch to the local scene view    
16. Drag and Drop EarthquakeDamage.xls on to ArcGIS Pro    
17. View those results.    
![UI](Screenshots/3dScreen.png)  
  

<!-- End -->

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<img src="https://esri.github.io/arcgis-pro-sdk/images/ArcGISPro.png"  alt="ArcGIS Pro SDK for Microsoft .NET Framework" height = "20" width = "20" align="top"  >
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
[Home](https://github.com/Esri/arcgis-pro-sdk/wiki) | <a href="https://pro.arcgis.com/en/pro-app/latest/sdk/api-reference" target="_blank">API Reference</a> | [Requirements](https://github.com/Esri/arcgis-pro-sdk/wiki#requirements) | [Download](https://github.com/Esri/arcgis-pro-sdk/wiki#installing-arcgis-pro-sdk-for-net) | <a href="https://github.com/esri/arcgis-pro-sdk-community-samples" target="_blank">Samples</a>

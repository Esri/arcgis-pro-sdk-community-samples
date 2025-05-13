## CIMExamples

<!-- TODO: Write a brief abstract explaining this sample -->
Shows the following CIM capabilities:  
  
1. Provide sample to create CIMUniqueValueRenderer from scratch  
2. Same as above but using the UniqueValueRendererDefinition class and the layer to configure the underlying Renderer  
3. How to create the equivalent of symbol levels in Pro.  
4. How to change out the Data Connection (equivalent to changing "DataSource" in ArcObjects)  
5. Change the selection color for the given feature layer  
  


<a href="https://pro.arcgis.com/en/pro-app/sdk/" target="_blank">View it live</a>

<!-- TODO: Fill this section below with metadata about this sample-->
```
Language:              C#
Subject:               Map Authoring
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
1. Download the Community Sample data (see under the 'Resources' section for downloading sample data). The sample data contains an ArcGIS Pro project and data to be used for this sample. Make sure that the Sample data is unzipped in c:\data and c:\data\Admin is available.
2. In Visual Studio click the Build menu. Then select Build Solution.  
3. Launch the debugger to open ArcGIS Pro.  
4. Open the project 'C:\Data\Admin\AdminSample.aprx'.  Please note that layer names and other specific data is required for this sample, hence this specific project is required.  
5. Click on the 'CIM Examples' tab.  
![CIMExamples](Screenshots/Screenshot1.png)  
6. Click the 'Renderer From Scratch' button to create a new render for the States layer:  
![CIMExamples](Screenshots/Screenshot2.png)  
7. Click the 'Renderer via Definition' button to create a new UniqueValueRendererDefinition for States using the 'TOTPOP2010' field:  
![CIMExamples](Screenshots/Screenshot3.png)   
8. Click the 'Create Symbol Levels' button to create the equivalent of symbol levels in Pro:  
![CIMExamples](Screenshots/Screenshot4.png)  
9. Click the 'Layer DataSource' button to change out the Data Connection to 'C:\Data\Admin\AdminSample.gdb':  
![CIMExamples](Screenshots/Screenshot5.png)  
10. Click the 'Layer Selection Color' button to change the selection color for the given feature layer:  
![CIMExamples](Screenshots/Screenshot6.png)    
11. Add a raster layer to your map.  For example: find and add 'CharlotteLAS' from the 'All Portal' connection, add the layer to your map, and then zoom to the newly added layer's extent.  
12. Click the 'Raster Stretch' button to create a CIMRasterStretchColorizer:  
![CIMExamples](Screenshots/Screenshot7.png)    
  

<!-- End -->

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<img src="https://esri.github.io/arcgis-pro-sdk/images/ArcGISPro.png"  alt="ArcGIS Pro SDK for Microsoft .NET Framework" height = "20" width = "20" align="top"  >
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
[Home](https://github.com/Esri/arcgis-pro-sdk/wiki) | <a href="https://pro.arcgis.com/en/pro-app/latest/sdk/api-reference" target="_blank">API Reference</a> | [Requirements](https://github.com/Esri/arcgis-pro-sdk/wiki#requirements) | [Download](https://github.com/Esri/arcgis-pro-sdk/wiki#installing-arcgis-pro-sdk-for-net) | <a href="https://github.com/esri/arcgis-pro-sdk-community-samples" target="_blank">Samples</a>

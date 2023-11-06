## MaskRaster

<!-- TODO: Write a brief abstract explaining this sample -->
This sample shows how to author a tool that can be used to mask raster pixel values in a ractangle  defined by the user. The masked output is saved as a new raster dataset in the project folder. This  sample only works on a single raster layer at a time.  
Note: You will need write access to the project folder in order to use this sample. The sample saves your input image before masking it so if you use a large image the save can take a while.  
  


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
3. The project used for this sample is 'C:\Data\RasterSample\RasterSample.aprx'  
4. In Visual Studio click the Build menu. Then select Build Solution.  
5. Click Start button to open ArcGIS Pro.  
6. ArcGIS Pro will open.   
7. Open the 'RasterSample.aprx' project or to use your own data open a map view and add a raster to the map.  
8. Select the raster layer in the TOC.  
9. Click on the Add-In tab on the ribbon.  
![UI](Screenshots/Screenshot1.png)  
10. Within this tab there is a Mask Raster Tool. Click it to activate the tool.  
11. In the map draw a rectangle around the area of the raster you want to mask.  
![UI](Screenshots/Screenshot2.png)  
12. A copy of the source raster dataset of the layer you selected will be saved in your project folder, the copy will be processed to mask pixels and a new layer will be added to your map using the processed copy.  
![UI](Screenshots/Screenshot3.png)  
13. You need to adjust the newly added raster layer's symbology to see the full masking effect.  
![UI](Screenshots/Screenshot4.png)  
14. Press the escape key if you want to deactivate the tool.  
  

<!-- End -->

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<img src="https://esri.github.io/arcgis-pro-sdk/images/ArcGISPro.png"  alt="ArcGIS Pro SDK for Microsoft .NET Framework" height = "20" width = "20" align="top"  >
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
[Home](https://github.com/Esri/arcgis-pro-sdk/wiki) | <a href="https://pro.arcgis.com/en/pro-app/latest/sdk/api-reference" target="_blank">API Reference</a> | [Requirements](https://github.com/Esri/arcgis-pro-sdk/wiki#requirements) | [Download](https://github.com/Esri/arcgis-pro-sdk/wiki#installing-arcgis-pro-sdk-for-net) | <a href="https://github.com/esri/arcgis-pro-sdk-community-samples" target="_blank">Samples</a>
